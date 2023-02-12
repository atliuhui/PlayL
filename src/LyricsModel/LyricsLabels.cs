namespace LyricsModel
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class LyricsLabels
    {
        public const string separator = "/";

        public const string label_version_pattern = @"\[ve:([^\n]+)][\r]?\n";
        public const string label_album_pattern = @"\[al:([^\n]+)][\r]?\n";
        public const string label_track_pattern = @"\[ti:([^\n]+)][\r]?\n";
        public const string label_authors_pattern = @"\[au:([^\n]*)][\r]?\n";
        public const string label_artists_pattern = @"\[ar:([^\n]*)][\r]?\n";
        public const string label_duration_pattern = @"\[length:([^\n]+)][\r]?\n";
        public const string line_pattern = @"\[([0-9:\.]+)]([^\n]*)[\r]?\n";

        public const string label_version_format = "[ve:{0}]\n";
        public const string label_album_format = "[al:{0}]\n";
        public const string label_track_format = "[ti:{0}]\n";
        public const string label_authors_format = "[au:{0}]\n";
        public const string label_artists_format = "[ar:{0}]\n";
        public const string label_duration_format = "[length:{0}]\n";
        public const string line_format = "[{0}]{1}\n";

        const string time1_format = @"m\:ss";
        const string time2_format = @"h\:mm\:ss";

        public static bool TryParse(string content, Dictionary<string, string> patterns, out Dictionary<string, string> labels)
        {
            labels = new Dictionary<string, string>();
            var pattern = string.Join(string.Empty, patterns.Values);
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);

            if (!match.Success) { return false; }
            if ((match.Groups.Count - patterns.Count()) < 1) { return false; }

            var index = 1;
            foreach (var item in patterns.Keys)
            {
                labels.Add(item, match.Groups[index++].Value);
            }

            return true;
        }
        public static string Replace(string content, Dictionary<string, string> replacements)
        {
            foreach (var item in replacements)
            {
                content = Regex.Replace(content, item.Key, item.Value);
            }

            return content;
        }
        public static string Insert(string content, IEnumerable<string> lines)
        {
            var builder = new StringBuilder();

            foreach (var item in lines)
            {
                builder.Append(item);
            }

            builder.Append(content);

            return builder.ToString();
        }
        public static string Remove(string content, IEnumerable<string> patterns)
        {
            var replacements = patterns.Select(item => new KeyValuePair<string, string>(item, string.Empty));
            return Replace(content, new Dictionary<string, string>(replacements));
        }
        public static string Init(string content, Dictionary<string, string> replacements)
        {
            content = Remove(content, replacements.Keys);
            content = Insert(content, replacements.Values);

            return content;
        }
        public static string Clear(string content)
        {
            var builder = new StringBuilder();
            using (var reader = new StringReader(content))
            {
                while (true)
                {
                    var line = reader.ReadLine();

                    if (line == null) break;

                    var match = Regex.Match($"{line}\n", LyricsLabels.line_pattern);
                    if (match.Success)
                    {
                        builder.AppendLine(match.Groups[2].Value);
                    }
                }
            }
            return builder.ToString();
        }

        public static IEnumerable<string> ToEnumerable(this string text)
        {
            return text.Split(separator.ToCharArray()).Select(item => item.Trim());
        }
        public static string FromEnumerable(this IEnumerable<string> array)
        {
            return string.Join(separator, array);
        }
        public static TimeSpan ToTimeSpan(this string text)
        {
            // https://learn.microsoft.com/zh-cn/dotnet/api/system.timespan.parseexact
            return -TimeSpan.ParseExact(text, new string[] { time1_format, time2_format }, CultureInfo.CurrentCulture, TimeSpanStyles.AssumeNegative);
        }
        public static string FromTimeSpan(this TimeSpan span)
        {
            // https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/custom-timespan-format-strings
            return span.ToString(time2_format).TrimStart('0', ':');
        }
    }
}
