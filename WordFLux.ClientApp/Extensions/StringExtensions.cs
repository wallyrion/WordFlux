using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace WordFLux.ClientApp.Extensions;

public static class StringExtensions
{
    public static RenderFragment? AsHtmlContent(this string? htmlString)
    {
        if (htmlString == null)
        {
            return null;
        }
        
        var str = ConvertAsterisksToBold(htmlString);

        return b => b.AddMarkupContent(0, str);
    }
    
    static string ConvertAsterisksToBold(string text)
    {
        // This pattern looks for text surrounded by asterisks (*)
        string pattern = @"\*(.*?)\*";

        // Replace the asterisks and the text between them with the <strong> HTML tag
        string result = Regex.Replace(text, pattern, "<strong>$1</strong>");

        return result;
    }
}