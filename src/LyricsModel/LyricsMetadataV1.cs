namespace LyricsModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class LyricsMetadataV1 : ILyricsMetadata
    {
        public string Code { get => Extensions.CreateHash($"{this.AlbumName}+{this.TrackName}"); set { } }
        public string Version { get; set; } = "1";
        public string AlbumName { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public IEnumerable<string> AuthorNames { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> ArtistNames { get; set; } = Enumerable.Empty<string>();
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        public static bool TryParse(string content, out LyricsMetadataV1? metadata)
        {
            if (LyricsTags.TryParse(content, new Dictionary<string, string>
            {
                [nameof(LyricsTags.version_pattern)] = LyricsTags.version_pattern,
                [nameof(LyricsTags.album_pattern)] = LyricsTags.album_pattern,
                [nameof(LyricsTags.track_pattern)] = LyricsTags.track_pattern,
                [nameof(LyricsTags.authors_pattern)] = LyricsTags.authors_pattern,
                [nameof(LyricsTags.artists_pattern)] = LyricsTags.artists_pattern,
                [nameof(LyricsTags.duration_pattern)] = LyricsTags.duration_pattern,
            }, out var tags))
            {
                metadata = new LyricsMetadataV1
                {
                    //Version = tags[nameof(LyricsTags.version_pattern)],
                    AlbumName = tags[nameof(LyricsTags.album_pattern)],
                    TrackName = tags[nameof(LyricsTags.track_pattern)],
                    AuthorNames = tags[nameof(LyricsTags.authors_pattern)].ToEnumerable(),
                    ArtistNames = tags[nameof(LyricsTags.artists_pattern)].ToEnumerable(),
                    Duration = tags[nameof(LyricsTags.duration_pattern)].ToTimeSpan(),
                };
                return true;
            }
            else
            {
                metadata = default(LyricsMetadataV1);
                return false;
            }
        }
        public string CoverMetadata(string content)
        {
            var builder = new StringBuilder();

            builder.AppendFormat(LyricsTags.version_format, this.Version);
            builder.AppendFormat(LyricsTags.album_format, this.AlbumName);
            builder.AppendFormat(LyricsTags.track_format, this.TrackName);
            builder.AppendFormat(LyricsTags.authors_format, this.AuthorNames.FromEnumerable());
            builder.AppendFormat(LyricsTags.artists_format, this.ArtistNames.FromEnumerable());
            builder.AppendFormat(LyricsTags.duration_format, this.Duration.FromTimeSpan());

            return builder.ToString();
        }

        public string UpdateMetadata(string content)
        {
            return LyricsTags.Replace(content, new Dictionary<string, string>
            {
                [LyricsTags.authors_pattern] = string.Format(LyricsTags.authors_format, this.AuthorNames.FromEnumerable()),
                [LyricsTags.artists_pattern] = string.Format(LyricsTags.artists_format, this.AuthorNames.FromEnumerable()),
                [LyricsTags.duration_pattern] = string.Format(LyricsTags.duration_format, this.AuthorNames.FromEnumerable()),
            });
        }
    }
}
