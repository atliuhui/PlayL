namespace LyricsIndex
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using LyricsModel;

    public class LyricsAssetsService
    {
        const string lyrics_dirname = "lyrics";

        readonly DirectoryInfo root;
        readonly DirectoryInfo lyricsDir;

        static void CreateIfNotExists(DirectoryInfo root)
        {
            if (!root.Exists)
            {
                root.Create();
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
        static string GetLyricsPattern()
        {
            return GetLyricsName("*");
        }

        public LyricsAssetsService(DirectoryInfo root)
        {
            this.root = root;

            CreateIfNotExists(this.root);

            this.lyricsDir = new DirectoryInfo(Path.Combine(this.root.FullName, lyrics_dirname));
        }

        public bool Exists(string code)
        {
            return File.Exists(Path.Combine(this.lyricsDir.FullName, GetLyricsName(code)));
        }
        public async Task<IQueryable<FileInfo>> GetFiles()
        {
            var files = this.lyricsDir.GetFiles(GetLyricsPattern(), SearchOption.AllDirectories).Select(item => item);

            return files.AsQueryable();
        }
        public async Task<IQueryable<LyricsFilterResult>> Index()
        {
            var files = this.lyricsDir.GetFiles(GetLyricsPattern(), SearchOption.AllDirectories).Select(item => item.FullName);
            var results = files.Select(item =>
            {
                var text = File.ReadAllText(item, Encoding.UTF8);
                var result = LyricsMetadataV1.TryParse(text, out var metadata) ? new LyricsFilterResult(metadata) : default(LyricsFilterResult);

                return result;
            });

            return results.Where(item => item != null).AsQueryable();
        }
        public async Task Append(string code, string content)
        {
            if (this.Exists(code))
            {
                throw new Exception($"{code} exists");
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
        public async Task Merge(string code, string content)
        {
            if (this.Exists(code))
            {
                await this.Update(code, content);
            }
            else
            {
                await this.Append(code, content);
            }
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
