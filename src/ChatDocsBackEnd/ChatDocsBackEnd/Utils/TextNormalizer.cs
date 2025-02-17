namespace ChatDocsBackEnd.Utils
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class TextNormalizer
    {
        // Compile regex patterns for performance
        private static readonly Regex MultipleWhitespacePattern = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex UrlPattern = new Regex(@"https?:\/\/\S+", RegexOptions.Compiled);
        private static readonly Regex EmailPattern = new Regex(@"[\w\.-]+@[\w\.-]+\.\w+", RegexOptions.Compiled);
        private static readonly Regex NonAlphanumericPattern = new Regex(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled);

        public static string NormalizeText(string text, NormalizationOptions options = null)
        {
            if (string.IsNullOrEmpty(text)) return text;

            options ??= new NormalizationOptions();
            var normalizedText = text;

            // 1. Unicode Normalization (NFKC - Compatibility Decomposition + Canonical Composition)
            normalizedText = normalizedText.Normalize(NormalizationForm.FormKC);

            // 2. Case Normalization
            if (options.ConvertToLowerCase)
            {
                normalizedText = normalizedText.ToLowerInvariant();
            }

            // 3. URL Handling
            if (options.NormalizeUrls)
            {
                normalizedText = UrlPattern.Replace(normalizedText, " URL ");
            }

            // 4. Email Handling
            if (options.NormalizeEmails)
            {
                normalizedText = EmailPattern.Replace(normalizedText, " EMAIL ");
            }

            // 5. Whitespace Normalization
            normalizedText = normalizedText
                .Replace("\r\n", " ")    // Convert Windows line endings
                .Replace("\n", " ")      // Convert Unix line endings
                .Replace("\t", " ")      // Convert tabs
                .Replace("\v", " ")      // Convert vertical tabs
                .Replace("\f", " ");     // Convert form feeds

            normalizedText = MultipleWhitespacePattern.Replace(normalizedText, " ").Trim();

            // 6. Special Character Handling
            if (options.NormalizeSpecialCharacters)
            {
                // Convert common special characters
                normalizedText = normalizedText
                    .Replace("&", " and ")
                    .Replace("%", " percent ")
                    .Replace("$", " USD ")
                    .Replace("€", " EUR ")
                    .Replace("£", " GBP ")
                    .Replace("@", " at ")
                    .Replace("#", " number ");
            }

            // 7. Number Normalization
            if (options.NormalizeNumbers)
            {
                // Convert number formats (e.g., 1,000,000 to 1000000)
                normalizedText = Regex.Replace(normalizedText, @"(\d),(\d)", "$1$2");
            }

            // 8. Punctuation Handling
            if (options.RemovePunctuation)
            {
                normalizedText = NonAlphanumericPattern.Replace(normalizedText, " ");
                normalizedText = MultipleWhitespacePattern.Replace(normalizedText, " ").Trim();
            }

            // 9. Diacritics Removal (if needed)
            if (options.RemoveDiacritics)
            {
                normalizedText = RemoveDiacritics(normalizedText);
            }

            // 10. Quote Normalization
            if (options.NormalizeQuotes)
            {
                normalizedText = normalizedText
               .Replace('\u201C', '"')  // Left double quote
               .Replace('\u201D', '"')  // Right double quote
               .Replace('\u2018', '\'') // Left single quote
               .Replace('\u2019', '\'') // Right single quote
               .Replace('\u0060', '\'') // Backtick
               .Replace('\u00B4', '\'') // Acute accent
               .Replace('\u2032', '\'') // Prime
               .Replace('\u2033', '"'); // Double prime
            }

            return normalizedText.Trim();
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    public class NormalizationOptions
    {
        public bool ConvertToLowerCase { get; set; } = true;
        public bool NormalizeUrls { get; set; } = true;
        public bool NormalizeEmails { get; set; } = true;
        public bool NormalizeSpecialCharacters { get; set; } = true;
        public bool NormalizeNumbers { get; set; } = true;
        public bool RemovePunctuation { get; set; } = true;
        public bool RemoveDiacritics { get; set; } = true;
        public bool NormalizeQuotes { get; set; } = true;
    }
}
