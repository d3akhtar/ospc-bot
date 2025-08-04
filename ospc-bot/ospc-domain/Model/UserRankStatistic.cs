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
    }
}