namespace LyricsModel
{
    using System;
    using System.Collections.Generic;

    public interface ILyricsMetadata : ILyricsCode
    {
        string Version { get; set; }
        string AlbumName { get; set; }
        string TrackName { get; set; }
        IEnumerable<string> AuthorNames { get; set; }
        IEnumerable<string> ArtistNames { get; set; }
        TimeSpan Duration { get; set; }
    }
}
