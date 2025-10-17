namespace OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues
{
    public record StringValue : NamedGroupMatchValue
    {
        public string Value;

        public StringValue(string name, string value) : base(name)
        {
            Value = value.Trim(' ', '\"');
        }
    }
}
