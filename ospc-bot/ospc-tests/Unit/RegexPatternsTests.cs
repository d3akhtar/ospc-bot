using OSPC.Utils;

namespace OSPC.Tests.Unit
{
	public class RegexPatternsTests
	{
		[Fact]
		public void OsuBeatmapLinkRegexMatchSuccessTest() 
		{
			Random r = new();
			for (int i = 0; i < 1000; i++) {
				string input = $"https://osu.ppy.sh/beatmapsets/{r.Next()}#osu/{r.Next()}";
				Assert.Matches(RegexPatterns.OsuBeatmapLinkRegex, input);
			}
		}

		[Theory]
		[InlineData("https://osu.ppy.sh/beatmapsets/1961109")]
		[InlineData("https://osu.ppy.sh/users/6399568")]
		[InlineData("https://osu.ppy.sh/api/v2/beatmaps/14")]
		public void OsuBeatmapLinkRegexMatchFailTest(string input)
		{
			Assert.DoesNotMatch(RegexPatterns.OsuBeatmapLinkRegex, input);
		}
	}
}
