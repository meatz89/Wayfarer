using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(new PrettyJsonFormatter())
    .WriteTo.Sink(new JsonArrayFileSink("C:\\Logs\\NarrativeAI.json", new PrettyJsonFormatter()))
    .Filter.ByIncludingOnly(evt =>
        evt.Properties.ContainsKey("SourceContext") &&
        evt.Properties["SourceContext"].ToString().Contains("BlazorRPG.Game.EncounterManager.NarrativeAi"))
    .CreateLogger();

// Ensure proper disposal on exit:
AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();


// Register your game services
builder.Services.AddGameServices();
builder.Host.UseSerilog();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
