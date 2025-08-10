using System.Text.Json.Serialization;

namespace OSPC.Domain.Model
{
    public class Beatmap
    {
        public static int Unspecified = -1;
        
        public int Id { get; set; }
        [JsonPropertyName("beatmapset_id")]
        public int BeatmapSetId { get; set; }
        public required string Version { get; set; }
        [JsonPropertyName("difficulty_rating")]
        public float DifficultyRating { get; set; }
        [JsonPropertyName("cs")]
        public float CircleSize { get; set; }
        [JsonPropertyName("bpm")]
        public float BPM { get; set; }
        [JsonPropertyName("total_length")]
        public float Length { get; set; }
        [JsonPropertyName("drain")]
        public float HpDrain { get; set; }
        [JsonPropertyName("accuracy")]
        public float OD { get; set; }
        [JsonPropertyName("ar")]
        public float AR { get; set; }
    }
}
