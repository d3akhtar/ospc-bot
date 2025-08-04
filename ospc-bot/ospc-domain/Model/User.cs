using System.Text.Json.Serialization;

namespace OSPC.Domain.Model
{
    public class User
    {
        private static string _profileUrlBase = "https://osu.ppy.sh/users/";
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; } = string.Empty;
        [JsonPropertyName("profile_colour")]
        public string ProfileColour { get; set; } = "00ff00";
        public string ProfileUrl => $"{_profileUrlBase}{Id}";
    }
}
