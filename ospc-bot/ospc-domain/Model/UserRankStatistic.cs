using System.Text.Json;
using System.Text.Json.Serialization;

namespace OSPC.Domain.Model
{
    public class UserRankStatistic
    {
        [JsonPropertyName("pp")]
        public float Pp { get; set; }
        [JsonPropertyName("global_rank")]
        public int? Rank { get; set; }
        [JsonPropertyName("country_rank")]
        public int? CountryRank { get; set; }

        public static UserRankStatistic? FromJsonString(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement statistic = doc.RootElement.GetProperty("statistics");
            return JsonSerializer.Deserialize<UserRankStatistic>(statistic.GetRawText());
        }
    }
}
