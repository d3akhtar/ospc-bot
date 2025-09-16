using OSPC.Utils.Parsing;
using OSPC.Utils.Parsing.RegularExpressions.Results;
using OSPC.Tests.Attributes;
using OSPC.Utils;

namespace OSPC.Tests.Unit
{
	public static class BeatmapFilterUnitTestHelperExtensions
	{
		public static bool EmptyBeatmapFilter(this BeatmapFilter filter)
			=> filter.CircleSize is null &&
		        filter.BPM is null &&
		        filter.Length is null &&
		        filter.HpDrain is null &&
		        filter.OD is null &&
		        filter.AR is null &&
		        filter.DifficultyRating is null;
	}
	
	public class SearchParamsTests
	{
		[Theory]
		[InlineData("opensand")]
		[InlineData("\"opensand\"")]
		public void UsernameParseSingleWordTest(string input)
		{
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;

			Assert.Equivalent("opensand", value.Username);

			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[InlineData("\"open sand\"")]
		public void UsernameParseMultiWordTest(string input)
		{
			
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;

			Assert.Equivalent("open sand", value.Username);

			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}
		
		[Theory]
		[InlineData("-e")]
		[InlineData("--exact")]
		public void ExactFlagParseTest(string input)
		{			
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;
			Assert.True(value.Exact);

			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[InlineData("-q feryquitous")]
		[InlineData("--query feryquitous")]
		[InlineData("-q \"feryquitous\"")]
		[InlineData("--query \"feryquitous\"")]
		public void QueryParseSingleWordTest(string input)
		{
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;

			Assert.Equivalent("feryquitous", value.Query);

			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[InlineData("-q \"Inori Minase\"")]
		[InlineData("--query \"Inori Minase\"")]
		public void QueryParseMultiWordTest(string input)
		{
			
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;

			Assert.Equivalent("Inori Minase", value.Query);

			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[InlineData("-a feryquitous")]
		[InlineData("--artist feryquitous")]
		[InlineData("-a \"feryquitous\"")]
		[InlineData("--artist \"feryquitous\"")]
		public void ArtistParseSingleWordTest(string input)
		{
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;

			Assert.Equivalent("feryquitous", value.Artist);

			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[InlineData("-a \"Inori Minase\"")]
		[InlineData("--artist \"Inori Minase\"")]
		public void ArtistParseMultiWordTest(string input)
		{
			
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;

			Assert.Equivalent("Inori Minase", value.Artist);

			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[InlineData("-t feryquitous")]
		[InlineData("--title feryquitous")]
		[InlineData("-t \"feryquitous\"")]
		[InlineData("--title \"feryquitous\"")]
		public void TitleParseSingleWordTest(string input)
		{
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;

			Assert.Equivalent("feryquitous", value.Title);

			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[InlineData("-t \"Inori Minase\"")]
		[InlineData("--title \"Inori Minase\"")]
		public void TitleParseMultiWordTest(string input)
		{
			
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);
			
			SearchParams value = result.Value!;

			Assert.Equivalent("Inori Minase", value.Title);

			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.Null(value.Playcount);
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[JsonFileData("searchParamsTestData.json")]
		public void PlaycountComparisonParseSingleFilterTest(string input, SingleFilter expectedValue)
		{			
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);

			SearchParams value = result.Value!;

			Assert.IsType<SingleFilter>(value.Playcount);
			Assert.Equivalent(expectedValue, value.Playcount);
			
			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}

		[Theory]
		[JsonFileData("searchParamsTestData.json")]
		public void PlaycountComparisonParseBetweenFilterTest(string input, BetweenFilter expectedValue)
		{
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);

			SearchParams value = result.Value!;

			Assert.NotNull(value.Playcount);
			Assert.IsType<BetweenFilter>(value.Playcount);
			Assert.Equivalent(expectedValue, value.Playcount);
			
			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.True(string.IsNullOrEmpty(value.Title));
		 	Assert.True(value.BeatmapFilter!.EmptyBeatmapFilter());
		}
		
		[Theory]
		[JsonFileData("searchParamsTestData.json")]
		public void BeatmapFilterSinglePropertyParseTest(string input, BeatmapFilter expectedValue)
		{			
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);

			SearchParams value = result.Value!;

			Assert.NotNull(value.BeatmapFilter);
			Assert.Equivalent(expectedValue, value.BeatmapFilter);
			
			Assert.True(string.IsNullOrEmpty(value.Username));
			Assert.False(value.Exact);
			Assert.True(string.IsNullOrEmpty(value.Query));
			Assert.True(string.IsNullOrEmpty(value.Artist));
			Assert.True(string.IsNullOrEmpty(value.Title));
			Assert.Null(value.Playcount);
		}

		[Theory]
		[JsonFileData("searchParamsTestData.json")]
		public void SearchParamsParseComplexInputTest(string input, SearchParams expectedValue)
		{
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.True(result.Successful);
			Assert.NotNull(result.Value);

			Assert.Equivalent(expectedValue, result.Value);
		}

		[Theory]
		[JsonFileData("searchParamsTestData.json")]
		public void SearchParamsParseErrorResultTest(string input, string error)
		{
			ParseResult<SearchParams> result = SearchParams.Parse(input);

			Assert.False(result.Successful);
			Assert.Null(result.Value);

			Assert.Equivalent(error, result.Error);
		}
	}
}
