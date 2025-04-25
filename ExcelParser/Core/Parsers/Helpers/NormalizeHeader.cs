namespace ExcelParser.Core.Parsers.Helpers;

public class NormalizeHeader
{
    public static string Normalize(string text)
    {
        return text?.Trim().Replace("\u00A0", " ").Trim() ?? string.Empty;
    }
}
