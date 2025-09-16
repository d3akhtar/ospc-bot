using System.Text.RegularExpressions;

namespace OSPC.Utils.Parsing.RegularExpressions.Interfaces
{
    public interface IRegexGroupLimitation
    {
        public bool NeedsOtherGroups { get; }
        public string GroupName { get; set; }
        public bool Passes(List<Group> groups);
        public bool Passes(MatchCollection matchCollection);
        public string ErrorMessage();
    }
}