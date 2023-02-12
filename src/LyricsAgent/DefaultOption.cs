namespace LyricsAgent
{
    public class DefaultOption
    {
        public string Assets { get; set; } = "assets";

        public DirectoryInfo AssetsRoot()
        {
            var dir = new DirectoryInfo(this.Assets);

            return dir;
        }
    }
}
