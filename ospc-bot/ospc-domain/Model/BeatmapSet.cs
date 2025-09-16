using System.Text.Json.Serialization;

namespace OSPC.Domain.Model
{
    public class BeatmapSet
    {
        public int Id { get; set; }
        [JsonPropertyName("play_count")]
        public ulong PlayCount { get; set; }
        public required string Artist { get; set; }
        public required string Title { get; set; }
        public Covers Covers { get; set; } = new();
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
        public override string ToString()
            => $"[BeatmapSet] Id: {Id}, PlayCount: {PlayCount}, Artist: {Artist}, Title: {Title}";
    }

    public class Covers
    {
        public string Cover { get; set; } = string.Empty;
        [JsonPropertyName("cover@2x")]
        public string Cover2x { get; set; } = string.Empty;
        public string Card { get; set; } = string.Empty;
        [JsonPropertyName("card@2x")]
        public string Card2x { get; set; } = string.Empty;
        public string List { get; set; } = string.Empty;
        [JsonPropertyName("list@2x")]
        public string List2x { get; set; } = string.Empty;
        [JsonPropertyName("slimcover")]
        public string SlimCover { get; set; } = string.Empty;
        [JsonPropertyName("slimcover@2x")]
        public string SlimCover2x { get; set; } = string.Empty;
    }
}