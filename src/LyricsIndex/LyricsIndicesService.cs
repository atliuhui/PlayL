namespace LyricsIndex
{
    using System.Text;
    using System.Text.RegularExpressions;
    using Lucene.Net.Analysis.Cn.Smart;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Search;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using LyricsModel;

    public class LyricsIndicesService
    {
        const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        readonly DirectoryInfo root;

        public LyricsIndicesService(DirectoryInfo root)
        {
            this.root = root;
        }

        public void Append(Document document)
        {
            using (var analyzer = new SmartChineseAnalyzer(AppLuceneVersion))
            {
                var config = new IndexWriterConfig(AppLuceneVersion, analyzer);

                using (var directory = FSDirectory.Open(this.root))
                using (var writer = new IndexWriter(directory, config))
                {
                    writer.AddDocument(document);
                    writer.Commit();
                }
            }
        }
        public void Update(Term term, Document document)
        {
            using (var analyzer = new SmartChineseAnalyzer(AppLuceneVersion))
            {
                var config = new IndexWriterConfig(AppLuceneVersion, analyzer);

                using (var dir = FSDirectory.Open(this.root))
                using (var writer = new IndexWriter(dir, config))
                {
                    writer.UpdateDocument(term, document);
                    writer.Commit();
                }
            }
        }
        public void Append(Term term)
        {
            using (var analyzer = new SmartChineseAnalyzer(AppLuceneVersion))
            {
                var config = new IndexWriterConfig(AppLuceneVersion, analyzer);

                using (var dir = FSDirectory.Open(this.root))
                using (var writer = new IndexWriter(dir, config))
                {
                    writer.DeleteDocuments(term);
                    writer.Flush(triggerMerge: false, applyAllDeletes: true);
                }
            }
        }
        public void AppendLyrics(LyricsInfo info)
        {
            var document = Convert(info);
            this.Append(document);
        }
        public void UpdateLyrics(LyricsInfo info)
        {
            var document = Convert(info);
            this.Update(new Term(nameof(info.Content.Code), info.Content.Code), document);
        }

        public LyricsSearchResult Search(Query query, int take = 20)
        {
            using (var directory = FSDirectory.Open(this.root))
            using (var reader = DirectoryReader.Open(directory))
            {
                var searcher = new IndexSearcher(reader);
                var tops = searcher.Search(query, take);
                var result = new LyricsSearchResult
                {
                    MaxScore = tops.MaxScore,
                    TotalHits = tops.TotalHits,
                    ScoreDocs = tops.ScoreDocs.Select(item =>
                    {
                        var doc = searcher.Doc(item.Doc);
                        return new LyricsSearchResultItem
                        {
                            Score = item.Score,
                            Code = doc.Get(nameof(LyricsSearchResultItem.Code)),
                            Text = doc.Get(nameof(LyricsSearchResultItem.Text)),
                        };
                    }).ToArray(),
                };

                return result;
            }
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

        private static Document Convert(LyricsInfo info)
        {
            var builder = new StringBuilder();
            using (var reader = new StringReader(info.Content.Text))
            {
                while (true)
                {
                    var line = reader.ReadLine();

                    if (line == null) break;

                    var match = Regex.Match($"{line}\n", LyricsTags.line_pattern);
                    if (match.Success)
                    {
                        builder.AppendLine(match.Groups[2].Value);
                    }
                }
            }

            var document = new Document();
            document.Add(new TextField(nameof(info.Metadata.AlbumName), info.Metadata.AlbumName, Field.Store.NO));
            document.Add(new TextField(nameof(info.Metadata.TrackName), info.Metadata.TrackName, Field.Store.NO));
            document.Add(new TextField(nameof(info.Metadata.AuthorNames), info.Metadata.AuthorNames.FromEnumerable(), Field.Store.NO));
            document.Add(new TextField(nameof(info.Metadata.ArtistNames), info.Metadata.ArtistNames.FromEnumerable(), Field.Store.NO));
            document.Add(new TextField(nameof(info.Content.Code), info.Content.Code, Field.Store.YES));
            document.Add(new TextField(nameof(info.Content.Text), builder.ToString(), Field.Store.YES));
            return document;
        }
    }
}
