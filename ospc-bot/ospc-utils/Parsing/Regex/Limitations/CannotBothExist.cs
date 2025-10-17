using System.Linq.Expressions;
using System.Text.RegularExpressions;

using OSPC.Utils.Helpers;
using OSPC.Utils.Parsing.Regex.Interfaces;

namespace OSPC.Utils.Parsing.Regex.Limitations
{
    public class CannotBothExist : IRegexGroupLimitation
    {
        public bool NeedsOtherGroups => true;
        public string GroupName { get; set; } = string.Empty;
        public string OtherGroup { get; set; } = string.Empty;

        public bool Passes(List<Group> groups)
            => !(groups.Any(g => g.Name == GroupName && g.Success) &&
                groups.Any(g => g.Name == OtherGroup && g.Success));

        public bool Passes(MatchCollection _) => false;

        public string ErrorMessage() => $"A match for both groups \'{GroupName}\' and \'{OtherGroup}\' isn't allowed";

        public static CannotBothExist Create(string groupName, string otherGroup)
            => new()
            {
                GroupName = groupName,
                OtherGroup = otherGroup
            };

        public static CannotBothExist Create(string otherGroup)
            => new()
            {
                OtherGroup = otherGroup
            };

        public static CannotBothExist Create<T, P>(Expression<Func<T, P>> exp)
            => new()
            {
                OtherGroup = GetMemberName(exp)
            };

        private static string GetMemberName<T, P>(Expression<Func<T, P>> exp)
            => ExpressionHelpers.GetMemberName(exp);
    }
}