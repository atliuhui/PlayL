namespace LyricsAdmin.Controllers
{
    using System.Globalization;
    using System.Text;
    using CsvHelper;
    using LyricsAdmin.Arguments;
    using LyricsIndex;
    using LyricsModel;
    using Microsoft.AspNetCore.Mvc;

    [Route("Tracks")]
    [ApiController]
    public class TracksController : ControllerBase
    {
        readonly LyricsAssetsService assets;
        readonly LyricsIndicesService indices;

        public TracksController(LyricsAssetsService assets, LyricsIndicesService indices)
        {
            this.assets = assets;
            this.indices = indices;
        }

        [HttpPost("check")]
        [ProducesResponseType(typeof(LyricsMetadataV1), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Check([FromBody] string text)
        {
            if (LyricsMetadataV1.TryParse(text, out var metadata))
            {
                return this.Ok(metadata);
            }
            else
            {
                return this.BadRequest(new DefaultResult { Message = "error" });
            }
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Append([FromBody] string text)
        {
            if (!LyricsMetadataV1.TryParse(text, out var metadata))
            {
                return this.BadRequest(new DefaultResult { Message = "parse error" });
            }

            try
            {
                var content = new LyricsContentV1 { Code = metadata.Code, Text = text };
                await this.assets.Append(metadata.Code, metadata.AlbumName, metadata.ArtistNames, content.Text);
                this.indices.AppendLyrics(new LyricsInfo { Metadata = metadata, Content = content });

                return this.Ok(new DefaultResult());
            }
            catch (Exception ex)
            {
                return this.BadRequest(new DefaultResult { Message = ex.GetBaseException().Message });
            }
        }

        [HttpPut("")]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] string text)
        {
            if (!LyricsMetadataV1.TryParse(text, out var metadata))
            {
                return this.BadRequest(new DefaultResult { Message = "parse error" });
            }

            try
            {
                var content = new LyricsContentV1 { Code = metadata.Code, Text = text };
                await this.assets.Update(metadata.Code, text);
                this.indices.UpdateLyrics(new LyricsInfo { Metadata = metadata, Content = content });

                return this.Ok(new DefaultResult());
            }
            catch (Exception ex)
            {
                return this.BadRequest(new DefaultResult { Message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("index")]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var view = await this.assets.Index();

                byte[] contents;
                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(view);
                    }
                    contents = stream.ToArray();
                }

                return this.File(contents, "text/csv");
            }
            catch (Exception ex)
            {
                return this.BadRequest(new DefaultResult { Message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("{code}")]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Download([FromRoute] string code)
        {
            try
            {
                var text = await this.assets.Download(code);
                var contents = Encoding.UTF8.GetBytes(text);

                return this.File(contents, "text/plain");
            }
            catch (Exception ex)
            {
                return this.BadRequest(new DefaultResult { Message = ex.GetBaseException().Message });
            }
        }
    }
}
