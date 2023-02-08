namespace LyricsAdmin.Extensions
{
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;
    using System.Text;

    // https://learn.microsoft.com/zh-cn/aspnet/core/web-api/advanced/custom-formatters
    public class TextPlainFormatter : TextInputFormatter
    {
        const string TextPlain = "text/plain";
        public TextPlainFormatter()
        {
            this.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(TextPlain));
            this.SupportedEncodings.Add(Encoding.UTF8);
        }
        protected override bool CanReadType(Type type) => type == typeof(string);
        public override bool CanRead(InputFormatterContext context) => TextPlain.Equals(context.HttpContext.Request.ContentType, StringComparison.OrdinalIgnoreCase);
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var request = context.HttpContext.Request;

            using (var reader = new StreamReader(request.Body, encoding))
            {
                var content = await reader.ReadToEndAsync();

                return await InputFormatterResult.SuccessAsync(content);
            }
        }
    }
}
