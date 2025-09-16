using OSPC.Domain.Model;

namespace OSPC.Tests.Unit
{
    public class BeatmapPlaycountTests
    {
        private readonly BeatmapPlaycount _testbpc = new BeatmapPlaycount
        {
            BeatmapId = 1,
            UserId = 1,
            Count = 100,
            Beatmap = new Beatmap()
            {
                Id = 1,
                DifficultyRating = 7.27f,
                BeatmapSetId = 1,
                Version = "Hard"
            },
            BeatmapSet = new()
            {
                Id = 1,
                Artist = "Feryquitous",
                Title = "Quon",
                Covers = new()
                {
                    SlimCover2x = "https://random.com"
                }
            }
        };

        [Fact]
        public void BeatmapPlaycountToStringTest()
        {
            string expected = $"**[100]** [Feryquitous - Quon [Hard]](https://osu.ppy.sh/beatmaps/1) [7.27★]";
            Assert.Equivalent(expected, _testbpc.ToString());
        }

        [Fact]
        public void BeatmapPlaycountToFullStringTest()
        {
            string expected = $"**[100]** [Feryquitous - Quon [Hard]](https://osu.ppy.sh/beatmaps/1) [7.27★]UserId: 1, BeatmapId: 1";
            Assert.Equivalent(expected, _testbpc.ToFullString());
        }
    }
}