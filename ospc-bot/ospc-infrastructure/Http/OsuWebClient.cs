using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OSPC.Domain.Model;
using OSPC.Domain.Options;
using OSPC.Domain.Model.DTO;
using OSPC.Infrastructure.Caching;
using OSPC.Infrastructure.Database.Repository;

namespace OSPC.Infrastructure.Http
{
    public class OsuWebClient : IOsuWebClient
    {
        private HttpClient _httpClient;
        private readonly IOptions<OsuWebApiOptions> _osuWebApiOptions;
        private readonly IRedisService _redis;
        private readonly IBeatmapRepository _beatmapRepo;
        private readonly int requestsPerMinute = 60;
        private List<DateTime> requestsWithinMinute = new();
        
        public OsuWebClient(IRedisService redis, IBeatmapRepository beatmapRepo, IOptions<OsuWebApiOptions> osuWebApiOptions)
        {
            _osuWebApiOptions = osuWebApiOptions;
            _redis = redis;
            _beatmapRepo = beatmapRepo;

            _httpClient = new HttpClient();
        }

        public async Task<List<BeatmapPlaycount>> GetBeatmapPlaycountsForUser(int userId, int limit = 100, int offset = 0)
        {
            var accessToken = await GetAccessToken();
            var uri = _osuWebApiOptions.Value.BaseUrl + $"users/{userId}/beatmapsets/most_played?limit={limit}&offset={offset}";

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var beatmapPlaycounts = await SendRequestWithAccessToken<List<BeatmapPlaycount>>(request, accessToken);
            if (beatmapPlaycounts != null) {
                foreach (var bpc in beatmapPlaycounts) bpc.UserId = userId;
            }
            return beatmapPlaycounts!;
        }

        public async Task<User?> FindUserWithUsername(string username)
        {
            var accessToken = await GetAccessToken();
            var uri = _osuWebApiOptions.Value.BaseUrl + $"users/@{username}";

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await SendRequestWithAccessToken<User>(request, accessToken);
        }

        public async Task LoadUserPlayedMaps(int userId)
        {
            int limit = 50;
            int offset = 0;
            int count = 0;

            HashSet<int> beatmapSetIds = new();
            HashSet<int> beatmapIds = new();
            List<BeatmapSet> beatmapSets = new();

            List<BeatmapPlaycount> beatmapPlaycounts = new() { new BeatmapPlaycount() };
            while (beatmapPlaycounts.Count > 0) {
                beatmapPlaycounts = await GetBeatmapPlaycountsForUser(userId, limit, offset);
                offset += limit;
                count += beatmapPlaycounts.Count;
                foreach(var b in beatmapPlaycounts) {
                    // File.AppendAllText($"maps_{userId}.txt", b + "\n");
                    if (!beatmapSetIds.Contains(b.BeatmapSet!.Id)){
                        beatmapSetIds.Add(b.BeatmapSet.Id);
                        beatmapSets.Add(b.BeatmapSet);
                    }
                    beatmapIds.Add(b.BeatmapId);
                }

                List<Beatmap> beatmapInfoExtended = await GetBeatmapStats(
                    await _beatmapRepo.GetRemainingBeatmapIds(beatmapIds)
                );

                await _beatmapRepo.AddBeatmapSetsAsync(beatmapSets);
                await _beatmapRepo.AddBeatmapsAsync(beatmapInfoExtended);
                await _beatmapRepo.AddBeatmapPlaycountsAsync(beatmapPlaycounts);

                beatmapSetIds.Clear();
                beatmapSets.Clear();
            }

            Console.WriteLine($"Loaded {count} beatmaps for {userId}");
        }

        public async Task<UserRankStatistic?> GetUserRankStatistics(int userId)
        {
            UserRankStatistic? stat = await _redis.GetUserRankStatisticAsync(userId);
            if (stat != null) return stat;

            var accessToken = await GetAccessToken();
            var uri = _osuWebApiOptions.Value.BaseUrl + $"users/{userId}";
            Console.WriteLine($"uri: {uri}");
            var res = await SendRequestWithAccessToken(uri, accessToken);
            if (res.IsSuccessStatusCode) {
                var json = await res.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement statistic = doc.RootElement.GetProperty("statistics");
                stat = JsonSerializer.Deserialize<UserRankStatistic>(statistic.GetRawText());
                await _redis.SaveUserRankStatisticAsync(userId, stat!);
                return stat;
            } else return default;
        }

        public async Task<List<Beatmap>> GetBeatmapStats(IEnumerable<int> beatmapIds)
        {
            if (beatmapIds.Count() == 0) return new();

            var accessToken = await GetAccessToken();
            string queryParams = "?";
            int length = beatmapIds.Count();
            int i = 0;
            foreach (int id in beatmapIds){
                queryParams += $"ids[]={id}{(i < length - 1 ? "&":"")}";
                i++;
            }

            var uri = _osuWebApiOptions.Value.BaseUrl + "beatmaps" + queryParams;

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return (await SendRequestWithAccessToken<BeatmapListResponse>(request, accessToken))?.Beatmaps ?? [];
        }

        private async Task<ClientCredentials> GetClientCredentials()
        {
            var uri = $"https://osu.ppy.sh/oauth/token";

            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _osuWebApiOptions.Value.ClientId.ToString()),
                    new KeyValuePair<string, string>("client_secret", _osuWebApiOptions.Value.ClientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "public")
                })
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var res = await _httpClient.SendAsync(request);

            return (await res.Content.ReadFromJsonAsync<ClientCredentials>())!;
        }

        private async Task<string> GetAccessToken()
        {
            string? accessToken = await _redis.GetAccessTokenAsync();
            if (accessToken == null) {
                var credentials = await GetClientCredentials();
                await _redis.SetAccessTokenAsync(credentials.AccessToken, credentials.ExpiresIn);
                accessToken = credentials.AccessToken;
            }

            return accessToken;
        }

        private async Task<T?> SendRequestWithAccessToken<T>(HttpRequestMessage request, string accessToken) where T : class
        {
            Console.WriteLine($"Request uri = {request.RequestUri}");
            
            await HandleRateLimits();

            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            if (typeof(T) == typeof(List<Beatmap>)) Console.WriteLine("sending...");
            var res = await _httpClient.SendAsync(request);
            if (typeof(T) == typeof(List<Beatmap>)) Console.WriteLine($"Res: {await res.Content.ReadAsStringAsync()}");

            return res.IsSuccessStatusCode ?
                await res.Content.ReadFromJsonAsync<T>():null;
        }

        private async Task<HttpResponseMessage> SendRequestWithAccessToken(string uri, string accessToken)
        {
            Console.WriteLine($"Request uri = {uri}");
                        
            await HandleRateLimits();
            
            HttpRequestMessage request = new(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            return await _httpClient.SendAsync(request);
        }

        private async Task HandleRateLimits()
        {
            requestsWithinMinute.Add(DateTime.Now);
            for (int i = requestsWithinMinute.Count - 1; i >= 0; i--){
                if ((DateTime.Now - requestsWithinMinute[i]).TotalSeconds > 60){
                    requestsWithinMinute.RemoveAt(i);
                }
            }

            if (requestsWithinMinute.Count > requestsPerMinute){
                var latestRequestTime = requestsWithinMinute[0];
                var timeDifference = (int)(60 - (DateTime.Now - latestRequestTime).TotalSeconds) + 1;
                Console.WriteLine($"Hit rate limit of 60req/min, sleeping for: {timeDifference} sec");
                await Task.Delay(timeDifference * 1000);
            }
        }
    }
}
