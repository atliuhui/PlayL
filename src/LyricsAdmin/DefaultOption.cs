namespace LyricsAdmin
{
    public class DefaultOption
    {
        public string Assets { get; set; } = "assets";
        public string Indices { get; set; } = "indices";

        public DirectoryInfo AssetsRoot()
        {
            var dir = new DirectoryInfo(this.Assets);

            return dir;
        }
        public DirectoryInfo IndicesRoot()
        {
            var dir = new DirectoryInfo(this.Indices);

            return dir;
        }
    }
}
