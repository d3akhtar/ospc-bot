namespace OSPC.Parsing.Regex
{
    [Flags]
    public enum RegexFlagMatchOptions
    {
        NoInput = 0,
        Word = 1,
        Specials = 2,
        Numbers = 4,
        StrictlyNumbers = 8,
        WholeNumbers = 16,
        Positive = 32,
        Multi = 64,
    }
}