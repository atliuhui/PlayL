namespace LyricsIndex
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CsvHelper;
    using LyricsModel;

    public class LyricsAssetsService
    {
        const string albums_filename = "albums.csv";
        const string artists_filename = "artists.csv";
        const string lyrics_dirname = "lyrics";

        readonly DirectoryInfo root;
        readonly List<AlbumInfo> albums;
        readonly List<ArtistInfo> artists;
        readonly DirectoryInfo lyricsDir;

        static void CreateIfNotExists(DirectoryInfo root)
        {
            if (!root.Exists)
            {
                root.Create();
            }

            var albums = Path.Combine(root.FullName, albums_filename);
            if (!File.Exists(albums))
            {
                using (var writer = new StreamWriter(albums))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(new AlbumInfo[] { new AlbumInfo() });
                }
            }

            var artists = Path.Combine(root.FullName, artists_filename);
            if (!File.Exists(artists))
            {
                using (var writer = new StreamWriter(artists))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(new ArtistInfo[] { new ArtistInfo() });
                }
            }

            var lyrics = Path.Combine(root.FullName, lyrics_dirname);
            if (!Directory.Exists(lyrics))
            {
                Directory.CreateDirectory(lyrics);
            }
        }
        static string GetLyricsName(string code)
        {
            return $"{code}.lrc";
        }

        public LyricsAssetsService(DirectoryInfo root)
        {
            this.root = root;

            CreateIfNotExists(this.root);

            using (var reader = new StreamReader(Path.Combine(this.root.FullName, albums_filename)))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                this.albums = csv.GetRecords<AlbumInfo>().ToList();
            }

            using (var reader = new StreamReader(Path.Combine(this.root.FullName, artists_filename)))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                this.artists = csv.GetRecords<ArtistInfo>().ToList();
            }

            this.lyricsDir = new DirectoryInfo(Path.Combine(this.root.FullName, lyrics_dirname));
        }

        public bool Exists(string code)
        {
            return File.Exists(Path.Combine(this.lyricsDir.FullName, GetLyricsName(code)));
        }
        public async Task<IQueryable<LyricsFilterResult?>> Index()
        {
            return this.lyricsDir.GetFiles().Select(item =>
            {
                if (LyricsMetadataV1.TryParse(File.ReadAllText(item.FullName, Encoding.UTF8), out var metadata))
                {
                    return new LyricsFilterResult(metadata);
                }
                else
                {
                    return default(LyricsFilterResult);
                }
            }).Where(item => item != null).AsQueryable();
        }
        public async Task Append(string code, string album, IEnumerable<string> artists, string content)
        {
            if (this.Exists(code))
            {
                throw new Exception($"{code} exists");
            }

            if (!this.albums.Any(item => item.Name.Equals(album, StringComparison.OrdinalIgnoreCase)))
            {
                var record = new AlbumInfo { Name = album };
                this.albums.Add(record);

                using (var writer = new StreamWriter(Path.Combine(this.root.FullName, albums_filename), true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
            }

            foreach (var artist in artists)
            {
                if (!this.artists.Any(item => item.Name.Equals(artist, StringComparison.OrdinalIgnoreCase)))
                {
                    var record = new ArtistInfo { Name = artist };
                    this.artists.Add(record);

                    using (var writer = new StreamWriter(Path.Combine(this.root.FullName, artists_filename), true))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecord(record);
                        csv.NextRecord();
                    }
                }
            }

            await File.WriteAllTextAsync(Path.Combine(this.lyricsDir.FullName, GetLyricsName(code)), content, Encoding.UTF8);
        }
        public async Task Update(string code, string content)
        {
            if (!this.Exists(code))
            {
                throw new Exception($"{code} not exists");
            }

            await File.WriteAllTextAsync(Path.Combine(this.lyricsDir.FullName, GetLyricsName(code)), content, Encoding.UTF8);
        }
        public async Task<string> Download(string code)
        {
            if (!this.Exists(code))
            {
                throw new Exception($"{code} not exists");
            }

            return await File.ReadAllTextAsync(Path.Combine(this.lyricsDir.FullName, GetLyricsName(code)));
        }
    }

    public class AlbumInfo
    {
        public string Name { get; set; }
    }
    public class ArtistInfo
    {
        public string Name { get; set; }
    }
}
