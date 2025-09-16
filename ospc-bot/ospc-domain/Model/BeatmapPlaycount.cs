using System.Text.Json.Serialization;

namespace OSPC.Domain.Model
{
    public class BeatmapPlaycount
    {
        private const string _baseGetBeatmapUrl = "https://osu.ppy.sh/beatmaps/";
        [JsonIgnore]
        public int UserId { get; set; }
        [JsonPropertyName("beatmap_id")]
        public int BeatmapId { get; set; }
        public int Count { get; set; }
        public Beatmap? Beatmap { get; set; }
        [JsonPropertyName("beatmapset")]
        public BeatmapSet? BeatmapSet { get; set; }
        public override string ToString()
            => $"**[{Count}]** [{BeatmapSet?.Artist ?? "[unknown]"} - {BeatmapSet?.Title ?? "[unnamed]"} [{Beatmap?.Version ?? "[unknown]"}]]({_baseGetBeatmapUrl + BeatmapId.ToString()}) [{Beatmap?.DifficultyRating ?? 0}★]";

        public string ToFullString()
            => ToString() + $"UserId: {UserId}, BeatmapId: {BeatmapId}";
    }
}