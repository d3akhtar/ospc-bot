namespace OSPC.Utils.Parsing.Regex.NamedGroupMatchValues
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
