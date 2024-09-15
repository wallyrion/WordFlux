namespace WordFLux.ClientApp.Utils;

public static class StringUtils
{
    public static string TruncateWithEllipsis(this string input, int maxLength = 10)
    {
        if (input.Length <= maxLength)
        {
            return input;
        }
    
        return input[..maxLength] + "...";
    }
}