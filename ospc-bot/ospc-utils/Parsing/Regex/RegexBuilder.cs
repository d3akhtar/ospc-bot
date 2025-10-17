using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

using OSPC.Utils.Parsing.RegularExpressions.Interfaces;
using OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues;
using OSPC.Domain.Common;

namespace OSPC.Utils.Parsing.RegularExpressions
{
    public class RegexEvaluator
    {
        private enum RegexGroupType
        {
            Group,
            Flag,
            Comparison
        }

        private struct RegexGroup
        {
            public List<IRegexGroupLimitation> Limitations = new();
            public System.Text.RegularExpressions.Regex Regex;
            public RegexGroupType GroupType;
            public string Name = string.Empty;
            public NamedGroupMatchValue MatchValue;
            public bool Success = false;

            public RegexGroup(string groupName, string regexString, RegexGroupType groupType)
            {
                Name = groupName;
                Regex = new(regexString);
                GroupType = groupType;
                MatchValue = new NoMatchValue(Name);
            }

            public Result<MatchCollection> Match(string input)
            {
                var matches = Regex.Matches(input);

                foreach (var limitation in Limitations.Where(l => !l.NeedsOtherGroups))
                {
                    if (!limitation.Passes(matches))
                        return Errors.Parsing(limitation.ErrorMessage());
                }

                if (matches.Count > 0)
                {
                    Success = true;
                    if (GroupType == RegexGroupType.Comparison)
                    {
                        var numberComparisonValueResult = GetNumberComparisonValueFromMatchCollection(matches);
                        if (!numberComparisonValueResult.Successful)
                            return numberComparisonValueResult.Error!;
                        else
                            MatchValue = numberComparisonValueResult.Value!;
                    }
                    else
                        MatchValue = new StringValue(Name, matches[0].Groups[Name].Value);
                }

                return matches;
            }

            public string Clean(string input) => Regex.Replace(input, string.Empty);

            private Result<NumberComparisonValue> GetNumberComparisonValueFromMatchCollection(MatchCollection matches)
            {
                switch (matches.Count)
                {
                    case 1:
                        return new NumberComparisonValue(Name, matches[0].Groups[Name + "_comparison"].Value, matches[0].Groups[Name + "_value"].Value);
                    case 2:
                    {
                        var value = new NumberComparisonValue(Name,
                            matches[0].Groups[Name + "_comparison"].Value, matches[0].Groups[Name + "_value"].Value,
                            matches[1].Groups[Name + "_comparison"].Value, matches[1].Groups[Name + "_value"].Value);

                        var validationResult = value.ValidNumberComparisonValue();
                        if (!validationResult.Successful)
                            return validationResult.Error!;

                        return value;
                    }
                    default:
                        return Errors.Parsing("Cannot have more than 2 comparisons for a single attribute");
                }
            }
        }

        private readonly Dictionary<string, RegexGroup> _nameToRegexGroupMap = new();

        private readonly Dictionary<RegexGroupType, List<RegexGroup>> _allRegexGroupsForGroupType = new() {
            { RegexGroupType.Group, new() },
            { RegexGroupType.Flag, new() },
            { RegexGroupType.Comparison, new() }
        };

        private readonly List<IRegexGroupLimitation> _limitations = new();
        private readonly HashSet<string> _groupNames = new();
        private readonly HashSet<string> _numberComparisonGroups = new();

        private string _lastGroupAdded = string.Empty;

        public RegexEvaluator AddGroup(string name) => AddGroup(name, RegexFlagMatchOptions.Word | RegexFlagMatchOptions.Multi);
        public RegexEvaluator AddGroup(string name, RegexFlagMatchOptions matchOptions)
        {
            if (matchOptions == RegexFlagMatchOptions.NoInput)
                throw new ArgumentException($"Invalid match option for group");
            if (_groupNames.Contains(name))
                throw new ArgumentException($"Group \'{name}\' has already been added");
            _groupNames.Add(name);

            _lastGroupAdded = name;

            _nameToRegexGroupMap.Add(name, new RegexGroup(name, matchOptions.GetRegexString(name), RegexGroupType.Group));
            _allRegexGroupsForGroupType[RegexGroupType.Group].Add(_nameToRegexGroupMap[name]);

            return this;
        }

        public RegexEvaluator AddFlag(string name) => AddFlag(name, RegexFlagMatchOptions.Word | RegexFlagMatchOptions.Multi);
        public RegexEvaluator AddFlag(string name, RegexFlagMatchOptions matchOptions)
        {
            if (_groupNames.Contains(name))
                throw new ArgumentException($"Group \'{name}\' has already been added");
            _groupNames.Add(name);

            _lastGroupAdded = name;

            var regexStringBuilder = new StringBuilder();

            regexStringBuilder
                .Append(matchOptions == RegexFlagMatchOptions.NoInput ? $"(?<{name}>" : string.Empty)
                .Append('(')
                .Append('-').Append(name[0])
                .Append('|')
                .Append("--").Append(name)
                .Append(')')
                .Append(matchOptions == RegexFlagMatchOptions.NoInput ? string.Empty : " ")
                .Append(matchOptions.GetRegexString(name))
                .Append(matchOptions == RegexFlagMatchOptions.NoInput ? ")" : string.Empty);

            _nameToRegexGroupMap.Add(name, new RegexGroup(name, regexStringBuilder.ToString(), RegexGroupType.Flag));
            _allRegexGroupsForGroupType[RegexGroupType.Flag].Add(_nameToRegexGroupMap[name]);

            return this;
        }

        public RegexEvaluator AddNumberComparison(string name, params string[] abbreviations)
            => AddNumberComparison(name, RegexFlagMatchOptions.StrictlyNumbers | RegexFlagMatchOptions.Positive, abbreviations);

        public RegexEvaluator AddNumberComparison(string name, RegexFlagMatchOptions matchOptions, params string[] abbreviations)
        {
            if (!matchOptions.ValidComparisonOptions())
                throw new ArgumentException($"Invalid match option for number comparison: {matchOptions}");
            if (_groupNames.Contains(name))
                throw new ArgumentException($"Group \'{name}\' has already been added");

            _groupNames.Add(name);
            _numberComparisonGroups.Add(name);

            _lastGroupAdded = name;

            var regexStringBuilder = new StringBuilder();
            regexStringBuilder
                .Append('(')
                .Append($"?<{name}>")
                .Append(GetAliasGroupRegexString(abbreviations))
                .Append($"(?<{name}_comparison><|<=|=|>|>=)")
                .Append(')')
                .Append(matchOptions.GetRegexString($"{name}_value"));

            _nameToRegexGroupMap.Add(name, new RegexGroup(name, regexStringBuilder.ToString(), RegexGroupType.Comparison));
            _allRegexGroupsForGroupType[RegexGroupType.Comparison].Add(_nameToRegexGroupMap[name]);

            return this;
        }

        public RegexEvaluator AddLimitation(IRegexGroupLimitation limitation)
        {
            if (string.IsNullOrEmpty(limitation.GroupName))
                limitation.GroupName = _lastGroupAdded;

            string name = limitation.GroupName;
            if (!_groupNames.Contains(name))
                throw new ArgumentException($"Group name: {name} wasn't ever added");
            _limitations.Add(limitation);
            _nameToRegexGroupMap[name].Limitations.Add(limitation);
            return this;
        }

        public MatchResult Match(string input)
        {
            List<MatchCollection> matchCollections = new();
            List<NamedGroupMatchValue> matchedValues = new();

            foreach (var groupType in new RegexGroupType[] { RegexGroupType.Flag, RegexGroupType.Comparison, RegexGroupType.Group })
            {
                foreach (var regexGroup in _allRegexGroupsForGroupType[groupType])
                {
                    var matchCollection = regexGroup.Match(input);
                    input = regexGroup.Clean(input);

                    if (!matchCollection.Successful)
                        return MatchResult.Error(matchCollection.Error!);

                    if (regexGroup.Success)
                    {
                        matchedValues.Add(regexGroup.MatchValue);
                        matchCollections.Add(matchCollection.Value!);
                    }
                }
            }

            List<Group> successfulGroups = GetSucceedingNamedGroups(matchCollections);
            foreach (var limitation in _limitations.Where(l => l.NeedsOtherGroups))
            {
                if (!limitation.Passes(successfulGroups))
                    return MatchResult.Error(limitation.ErrorMessage());
            }

            return MatchResult.Success(matchedValues, input);
        }

        private List<Group> GetSucceedingNamedGroups(MatchCollection matches)
        {
            List<Group> successfulGroups = new();
            foreach (Match match in matches)
            {
                foreach (var name in _groupNames)
                {
                    if (match.Groups[name].Success && !name.Any(char.IsDigit))
                        successfulGroups.Add(match.Groups[name]);
                }
            }

            return successfulGroups;
        }

        private List<Group> GetSucceedingNamedGroups(List<MatchCollection> matchCollections)
        {
            List<Group> successfulGroups = new();
            foreach (MatchCollection collection in matchCollections)
            {
                foreach (var group in GetSucceedingNamedGroups(collection))
                {
                    successfulGroups.Add(group);
                }
            }

            return successfulGroups;
        }

        private string GetAliasGroupRegexString(string[] aliases)
        {
            string flags = string.Empty;
            if (aliases.Length == 1)
                flags = aliases[0];
            else
            {
                flags += "(";
                for (int i = 0; i < aliases.Length; i++)
                {
                    flags += aliases[i];
                    if (i != aliases.Length - 1)
                        flags += '|';
                }
                flags += ")";
            }

            return flags;
        }
    }

    public class RegexEvaluator<T> : RegexEvaluator where T : class, new()
    {
        public RegexEvaluator<T> AddGroup<P>(Expression<Func<T, P>> exp)
            => (RegexEvaluator<T>)AddGroup(GetMemberName(exp));

        public RegexEvaluator<T> AddGroup<P>(Expression<Func<T, P>> exp, RegexFlagMatchOptions options)
            => (RegexEvaluator<T>)AddGroup(GetMemberName(exp), options);

        public RegexEvaluator<T> AddFlag<P>(Expression<Func<T, P>> exp)
            => (RegexEvaluator<T>)AddFlag(GetMemberName(exp));

        public RegexEvaluator<T> AddFlag<P>(Expression<Func<T, P>> exp, RegexFlagMatchOptions options)
            => (RegexEvaluator<T>)AddFlag(GetMemberName(exp), options);

        public RegexEvaluator<T> AddNumberComparison<P>(Expression<Func<T, P>> exp, params string[] aliases)
            => (RegexEvaluator<T>)AddNumberComparison(GetMemberName(exp), aliases);

        public RegexEvaluator<T> AddNumberComparison<P>(Expression<Func<T, P>> exp, RegexFlagMatchOptions options, params string[] aliases)
            => (RegexEvaluator<T>)AddNumberComparison(GetMemberName(exp), options, aliases);

        public new RegexEvaluator<T> AddLimitation(IRegexGroupLimitation limitation)
            => (RegexEvaluator<T>)base.AddLimitation(limitation);

        private string GetMemberName<P>(Expression<Func<T, P>> exp)
        {
            MemberExpression? me = exp.Body switch
            {
                UnaryExpression u => u.Operand as MemberExpression,
                MemberExpression m => m,
                _ => throw new Exception()
            };

            if (me is null)
                throw new NullReferenceException(nameof(me));

            Span<char> name = stackalloc char[me.Member.Name.Length];
            me.Member.Name.AsSpan().CopyTo(name);
            name[0] = char.ToLower(name[0]);
            return new string(name);
        }
    }
}