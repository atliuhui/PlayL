namespace LyricsAgent.Controllers
{
    using System.Globalization;
    using System.IO;
    using System.Text;
    using CsvHelper;
    using LyricsAgent.Arguments;
    using LyricsIndex;
    using Microsoft.AspNetCore.Mvc;

    [Route("Tracks")]
    [ApiController]
    public class TracksController : ControllerBase
    {
        readonly LyricsAssetsService assets;
        readonly LyricsIndicesService service;

        public TracksController(LyricsAssetsService assets, LyricsIndicesService service)
        {
            this.assets = assets;
            this.service = service;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery(Name = "q")] string pattern)
        {
            try
            {
                var result = this.service.SearchLyrics(pattern);
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                return this.BadRequest(new DefaultResult { Message = ex.GetBaseException().Message });
            }
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromBody] Dictionary<string, string> pattern)
        {
            try
            {
                var result = this.service.SearchLyrics(pattern);
                return this.Ok(result);
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
