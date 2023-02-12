using LyricsAgent;
using LyricsIndex;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DefaultOption>(builder.Configuration.GetSection("Options"));
builder.Services.AddSingleton<LyricsAssetsService>(provider =>
{
    var option = provider.GetRequiredService<IOptions<DefaultOption>>().Value;
    return new LyricsAssetsService(option.AssetsRoot());
});
builder.Services.AddSingleton<LyricsIndicesService>(provider =>
{
    var option = provider.GetRequiredService<IOptions<DefaultOption>>().Value;
    return new LyricsIndicesService(option.AssetsRoot());
});
builder.Services.AddControllers(options =>
{
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseCors();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
