using Markdig;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System.Web;

namespace WebSpark.Core.Extensions
{
    public static partial class StringExtensions
    {
        private static readonly Regex DashRemover = MyRegex();
        private static readonly Regex LeadingSlashRemover = MyRegex1();
        private static readonly Regex RegexStripHtml = MyRegex2();
        private static readonly Regex WhiteSpaceRemover = MyRegex3();

        static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
            {
                sb.Append(c);
            }

            return sb.ToString();
        }

        static string RemoveExtraHyphen(string text)
        {
            while (text.Contains("--"))
            {
                text = text.Replace("--", "-");
            }

            return text;
        }

        static string RemoveIllegalCharacters(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string[] chars = { ":", "/", "?", "!", "#", "[", "]", "{", "}", "@", "*", ".", ",", "\"", "&", "'", "~", "$" };

            foreach (var ch in chars)
            {
                text = text.Replace(ch, string.Empty);
            }

            text = text.Replace("–", "-").Replace(" ", "-");
            text = RemoveUnicodePunctuation(text);
            text = RemoveDiacritics(text);
            text = RemoveExtraHyphen(text);

            return HttpUtility.HtmlEncode(text).Replace("%", string.Empty);
        }

        static string RemoveUnicodePunctuation(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.InitialQuotePunctuation &&
                                                    CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.FinalQuotePunctuation))
            {
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return char.ToUpper(str[0]) + str[1..];
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string ExtractTitle(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;
            var separator = str.Contains('\\') ? '\\' : str.Contains('/') ? '/' : '\0';
            return separator == '\0' ? str : str[(str.LastIndexOf(separator) + 1)..];
        }

        public static string Hash(this string source, string salt)
        {
            var bytes = KeyDerivation.Pbkdf2(
                password: source,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return Convert.ToBase64String(bytes);
        }

        public static bool IsMatch(this string str1, string str2)
        {
            str1 = SanitizeForComparison(str1);
            str2 = SanitizeForComparison(str2);
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        public static string MdToHtml(this string str)
        {
            var mpl = new MarkdownPipelineBuilder()
                .UsePipeTables()
                .UseAdvancedExtensions()
                .Build();

            return Markdown.ToHtml(str, mpl);
        }

        public static string RemoveScriptTags(this string str)
        {
            return string.IsNullOrEmpty(str) ? str : MyRegex4().Replace(str, string.Empty);
        }

        public static string ReplaceIgnoreCase(this string str, string search, string replacement)
        {
            return Regex.Replace(
                str,
                Regex.Escape(search),
                replacement.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );
        }

        public static string SanitizeFileName(this string str)
        {
            return str.SanitizePath();
        }

        public static string SanitizePath(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;

            str = str.Replace("%2E", ".").Replace("%2F", "/");

            if (str.Contains("..") || str.Contains("//"))
                throw new ApplicationException("Invalid directory path");

            return str;
        }

        public static string StripHtml(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? string.Empty : RegexStripHtml.Replace(str, string.Empty).Trim();
        }

        public static string ToSlug(this string title)
        {
            if (string.IsNullOrEmpty(title)) return string.Empty;

            var str = title.ToLowerInvariant().Trim('-', '_');
            var bytes = Encoding.GetEncoding("utf-8").GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);

            str = Regex.Replace(str, @"\s", "-", RegexOptions.Compiled);
            str = Regex.Replace(str, @"([-_]){2,}", "$1", RegexOptions.Compiled);
            str = RemoveIllegalCharacters(str);

            return str;
        }

        public static string ToThumb(this string img)
        {
            if (img.IndexOf('/') < 1) return img;

            var first = img[..img.LastIndexOf('/')];
            var second = img[img.LastIndexOf('/')..];

            return $"{first}/thumbs{second}";
        }

        public static string ToHtmlFromMarkdown(this string markdown)
        {
            return Markdown.ToHtml(markdown);
        }

        public static string ToLowerInvariant(this string str)
        {
            return str?.ToLowerInvariant();
        }

        public static string ToUpperInvariant(this string str)
        {
            return str?.ToUpperInvariant();
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string ToBase64(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        public static string FromBase64(this string base64)
        {
            if (string.IsNullOrEmpty(base64)) return base64;
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }

        public static string Truncate(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength) return str;
            return str[..maxLength];
        }

        public static bool ContainsAny(this string str, params string[] values)
        {
            if (string.IsNullOrEmpty(str) || values == null || values.Length == 0) return false;
            foreach (var value in values)
            {
                if (str.Contains(value, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static string SanitizeForComparison(string str)
        {
            return LeadingSlashRemover.Replace(WhiteSpaceRemover.Replace(DashRemover.Replace(str, string.Empty), string.Empty), string.Empty);
        }

        [GeneratedRegex("-", RegexOptions.Compiled)]
        private static partial Regex MyRegex();
        [GeneratedRegex("^/", RegexOptions.Compiled)]
        private static partial Regex MyRegex1();
        [GeneratedRegex("<[^>]*>", RegexOptions.Compiled)]
        private static partial Regex MyRegex2();
        [GeneratedRegex(" ", RegexOptions.Compiled)]
        private static partial Regex MyRegex3();
        [GeneratedRegex(@"<script[^>]*>[\s\S]*?</script>")]
        private static partial Regex MyRegex4();
    }
}
