using System.Text.RegularExpressions;
using OSPC.Utils.Parsing.RegularExpressions.Interfaces;

namespace OSPC.Utils.Parsing.RegularExpressions.Limitations
{
	public class LimitMatchCount : IRegexGroupLimitation
	{
		public bool NeedsOtherGroups => false;
		public string GroupName { get; set; } = string.Empty;
		public int MaxMatchCount { get; set; }

		public bool Passes(List<Group> groups)
		{
			var succeedingGroups = groups.Where(g => g.Name == GroupName).ToList();
			return succeedingGroups.Count <= MaxMatchCount;
		}

		public bool Passes(MatchCollection matches)
		{
			var succeedingGroups = new List<Group>();
			for (int i = 0; i < matches.Count; i++) {
				var namedGroup = matches[i].Groups[GroupName];
				if (namedGroup.Success) succeedingGroups.Add(namedGroup);
			}
	
			return succeedingGroups.Count <= MaxMatchCount;
		}

		public string ErrorMessage() => $"Number of matches for \'{GroupName}\' exceeded the maximum amount: {MaxMatchCount}";

		public static LimitMatchCount Create(string groupName, int limit)
			=> new() {
				GroupName = groupName,
				MaxMatchCount = limit
			};

		public static LimitMatchCount Create(int limit)
			=> new() {
				MaxMatchCount = limit
			};
	}
}
