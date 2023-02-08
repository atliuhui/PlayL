namespace LyricsIndex
{
    using LyricsModel;

    public class LyricsSearch
    {
        //public int Skip { get; set; }
        public int Take { get; set; } = 20;
        public string? AlbumName { get; set; }
        public string? TrackName { get; set; }
        public string? AuthorName { get; set; }
        public string? ArtistName { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class LyricsSearchResult
    {
        public virtual float MaxScore { get; set; }
        public int TotalHits { get; set; }
        public IEnumerable<LyricsSearchResultItem> ScoreDocs { get; set; }
    }
    public class LyricsSearchResultItem
    {
        public float Score { get; set; }
        public string Code { get; set; }
        public string Text { get; set; }
    }

    public class LyricsFilter
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 20;
    }

    public class LyricsFilterResult
    {
        readonly ILyricsMetadata metadata;

        public string Code { get => this.metadata.Code; }
        public string AlbumName { get => this.metadata.AlbumName; }
        public string TrackName { get => this.metadata.TrackName; }
        public string AuthorNames { get => this.metadata.AuthorNames.FromEnumerable(); }
        public string ArtistNames { get => this.metadata.ArtistNames.FromEnumerable(); }
        public string Duration { get => this.metadata.Duration.FromTimeSpan(); }

        public LyricsFilterResult(ILyricsMetadata metadata)
        {
            this.metadata = metadata;
        }
    }
}
