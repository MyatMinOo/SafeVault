// Services/InputSanitizer.cs
using System.Text.RegularExpressions;
using System.Net;

namespace SafeVault.Api.Services
{
    public class InputSanitizer
    {
        // Remove script tags and content, limit length, and optionally remove suspicious chars
        private static readonly Regex ScriptRegex = new Regex(@"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex HtmlTagRegex = new Regex("<.*?>", RegexOptions.Singleline);

        public string SanitizeString(string input, int maxLength = 200)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // Remove script blocks
            var cleaned = ScriptRegex.Replace(input, string.Empty);
            // Optionally remove any tags
            cleaned = HtmlTagRegex.Replace(cleaned, string.Empty);
            // Remove control chars
            cleaned = Regex.Replace(cleaned, @"[\x00-\x1F\x7F]", string.Empty);
            // Trim and enforce max length
            cleaned = cleaned.Trim();
            if (cleaned.Length > maxLength) cleaned = cleaned.Substring(0, maxLength);
            return cleaned;
        }

        public string SanitizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return email;
            var cleaned = SanitizeString(email, maxLength: 100);
            // Optionally enforce simple email pattern or rely on DataAnnotations
            return cleaned;
        }

        // When rendering to pages, always HTML-encode:
        public string EncodeForHtml(string input) => input == null ? null : WebUtility.HtmlEncode(input);
    }
}
