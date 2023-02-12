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
            if (LyricsLabels.TryParse(content, new Dictionary<string, string>
            {
                [nameof(LyricsLabels.label_version_pattern)] = LyricsLabels.label_version_pattern,
                [nameof(LyricsLabels.label_album_pattern)] = LyricsLabels.label_album_pattern,
                [nameof(LyricsLabels.label_track_pattern)] = LyricsLabels.label_track_pattern,
                [nameof(LyricsLabels.label_authors_pattern)] = LyricsLabels.label_authors_pattern,
                [nameof(LyricsLabels.label_artists_pattern)] = LyricsLabels.label_artists_pattern,
                [nameof(LyricsLabels.label_duration_pattern)] = LyricsLabels.label_duration_pattern,
            }, out var labels))
            {
                metadata = new LyricsMetadataV1
                {
                    //Version = labels[nameof(LyricsLabels.version_pattern)],
                    AlbumName = labels[nameof(LyricsLabels.label_album_pattern)],
                    TrackName = labels[nameof(LyricsLabels.label_track_pattern)],
                    AuthorNames = labels[nameof(LyricsLabels.label_authors_pattern)].ToEnumerable(),
                    ArtistNames = labels[nameof(LyricsLabels.label_artists_pattern)].ToEnumerable(),
                    Duration = labels[nameof(LyricsLabels.label_duration_pattern)].ToTimeSpan(),
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

            builder.AppendFormat(LyricsLabels.label_version_format, this.Version);
            builder.AppendFormat(LyricsLabels.label_album_format, this.AlbumName);
            builder.AppendFormat(LyricsLabels.label_track_format, this.TrackName);
            builder.AppendFormat(LyricsLabels.label_authors_format, this.AuthorNames.FromEnumerable());
            builder.AppendFormat(LyricsLabels.label_artists_format, this.ArtistNames.FromEnumerable());
            builder.AppendFormat(LyricsLabels.label_duration_format, this.Duration.FromTimeSpan());

            return builder.ToString();
        }

        public string UpdateMetadata(string content)
        {
            return LyricsLabels.Replace(content, new Dictionary<string, string>
            {
                [LyricsLabels.label_authors_pattern] = string.Format(LyricsLabels.label_authors_format, this.AuthorNames.FromEnumerable()),
                [LyricsLabels.label_artists_pattern] = string.Format(LyricsLabels.label_artists_format, this.AuthorNames.FromEnumerable()),
                [LyricsLabels.label_duration_pattern] = string.Format(LyricsLabels.label_duration_format, this.AuthorNames.FromEnumerable()),
            });
        }
    }
}
