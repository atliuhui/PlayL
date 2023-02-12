namespace LyricsIndex
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using CsvHelper;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Cn.Smart;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Search;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using LyricsModel;
    using Directory = System.IO.Directory;
    using Document = Lucene.Net.Documents.Document;

    public class LyricsIndicesService
    {
        const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        const string indices_dirname = "indices";
        const string albums_filename = "albums.csv";
        const string artists_filename = "artists.csv";

        readonly DirectoryInfo root;
        readonly DirectoryInfo indicesDir;
        readonly List<AlbumInfo> albums;
        readonly List<ArtistInfo> artists;

        static void CreateIfNotExists(DirectoryInfo root)
        {
            if (!root.Exists)
            {
                root.Create();
            }

            var albums = Path.Combine(root.FullName, albums_filename);
            if (!File.Exists(albums))
            {
                using (var writer = new StreamWriter(albums, false))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<AlbumInfo>();
                    csv.NextRecord();
                    csv.WriteRecord(new AlbumInfo());
                }
            }

            var artists = Path.Combine(root.FullName, artists_filename);
            if (!File.Exists(artists))
            {
                using (var writer = new StreamWriter(artists, false))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<ArtistInfo>();
                    csv.NextRecord();
                    csv.WriteRecord(new ArtistInfo());
                }
            }

            var indices = Path.Combine(root.FullName, indices_dirname);
            if (!Directory.Exists(indices))
            {
                Directory.CreateDirectory(indices);
            }
        }

        public LyricsIndicesService(DirectoryInfo root)
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

            this.indicesDir = new DirectoryInfo(Path.Combine(this.root.FullName, indices_dirname));
        }

        public void IndexAlbum(string album)
        {
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
        }
        public void IndexArtist(string artist)
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
        public void IndexArtist(IEnumerable<string> artists)
        {
            foreach (var artist in artists)
            {
                this.IndexArtist(artist);
            }
        }

        void WriteWrap(Action<IndexWriter, Analyzer> action)
        {
            using (var analyzer = new SmartChineseAnalyzer(AppLuceneVersion))
            {
                var config = new IndexWriterConfig(AppLuceneVersion, analyzer);

                using (var directory = FSDirectory.Open(this.indicesDir))
                using (var writer = new IndexWriter(directory, config))
                {
                    action(writer, analyzer);
                    writer.Commit();
                }
            }
        }
        void ReadWrap(Action<IndexSearcher> action)
        {
            using (var directory = FSDirectory.Open(this.indicesDir))
            using (var reader = DirectoryReader.Open(directory))
            {
                var searcher = new IndexSearcher(reader);
                action(searcher);
            }
        }

        public void Append(Document document)
        {
            this.WriteWrap((writer, analyzer) =>
            {
                writer.AddDocument(document, analyzer);
            });
        }
        public void Update(Term term, Document document)
        {
            this.WriteWrap((writer, analyzer) =>
            {
                writer.UpdateDocument(term, document, analyzer);
            });
        }
        public void Delete(Term term)
        {
            this.WriteWrap((writer, analyzer) =>
            {
                writer.DeleteDocuments(term);
            });
        }
        public void Merge(Term term, Document document)
        {
            this.WriteWrap((writer, analyzer) =>
            {
                writer.UpdateDocuments(term, new Document[] { document }, analyzer);
            });
        }
        public void AppendLyrics(LyricsInfo info)
        {
            var document = Convert(info);

            this.IndexAlbum(info.Metadata.AlbumName);
            this.IndexArtist(info.Metadata.AuthorNames);
            this.IndexArtist(info.Metadata.ArtistNames);

            this.Append(document);
        }
        public void UpdateLyrics(LyricsInfo info)
        {
            var document = Convert(info);

            this.Update(new Term(nameof(info.Content.Code), info.Content.Code), document);
        }
        public void MergeLyrics(LyricsInfo info)
        {
            var document = Convert(info);

            this.IndexAlbum(info.Metadata.AlbumName);
            this.IndexArtist(info.Metadata.AuthorNames);
            this.IndexArtist(info.Metadata.ArtistNames);

            this.Merge(new Term(nameof(info.Content.Code), info.Content.Code), document);
        }

        public LyricsSearchResult Search(Query query, int take = 20)
        {
            LyricsSearchResult result = new LyricsSearchResult();

            this.ReadWrap((searcher) =>
            {
                var tops = searcher.Search(query, take);
                result.MaxScore = tops.MaxScore;
                result.TotalHits = tops.TotalHits;
                result.ScoreDocs = tops.ScoreDocs.Select(item =>
                {
                    var doc = searcher.Doc(item.Doc);
                    return new LyricsSearchResultItem
                    {
                        Score = item.Score,
                        Code = doc.Get(nameof(LyricsSearchResultItem.Code)),
                        Text = doc.Get(nameof(LyricsSearchResultItem.Text)),
                    };
                }).ToArray();
            });

            return result;
        }
        public LyricsSearchResult SearchLyrics(Dictionary<string, string> patterns)
        {
            var terms = new MultiPhraseQuery();
            foreach (var item in patterns)
            {
                terms.Add(new Term(item.Key, item.Value));
            }
            return this.Search(terms);
        }
        public LyricsSearchResult SearchLyrics(string keyword)
        {
            using (var analyzer = new SmartChineseAnalyzer(AppLuceneVersion))
            {
                var parser = new QueryParser(AppLuceneVersion, nameof(LyricsSearch.Text), analyzer);

                return this.Search(parser.Parse(keyword));
            }
        }

        static Document Convert(LyricsInfo info)
        {
            var builder = LyricsLabels.Clear(info.Content.Text);

            var document = new Document();
            document.Add(new TextField(nameof(info.Metadata.AlbumName), info.Metadata.AlbumName, Field.Store.NO));
            document.Add(new TextField(nameof(info.Metadata.TrackName), info.Metadata.TrackName, Field.Store.NO));
            document.Add(new TextField(nameof(info.Metadata.AuthorNames), info.Metadata.AuthorNames.FromEnumerable(), Field.Store.NO));
            document.Add(new TextField(nameof(info.Metadata.ArtistNames), info.Metadata.ArtistNames.FromEnumerable(), Field.Store.NO));
            document.Add(new TextField(nameof(info.Content.Code), info.Content.Code, Field.Store.YES));
            document.Add(new TextField(nameof(info.Content.Text), builder, Field.Store.YES));
            return document;
        }
    }
}
