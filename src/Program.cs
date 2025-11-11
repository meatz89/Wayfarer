using Serilog;

// Suppress security warnings
Environment.SetEnvironmentVariable("ASPNETCORE_SUPPRESSSTATUSMESSAGES", "true");

WebApplicationOptions options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Path.GetFullPath(Directory.GetCurrentDirectory()),
    WebRootPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
};

WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers(); // Add controller support

IConfigurationRoot configuration = new ConfigurationBuilder()
.AddJsonFile("appsettings.json")
.AddJsonFile("appsettings.Development.json", optional: true)
.Build();

// Register IConfiguration for dependency injection
builder.Services.AddSingleton<IConfiguration>(configuration);

// Initialize GameWorld via static factory - no DI dependencies
GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();
builder.Services.AddSingleton(gameWorld);

builder.Services.ConfigureServices();

Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.WriteTo.Console()
.CreateLogger();

// Ensure proper disposal on exit
AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;

// Register your game services
builder.Host.UseSerilog();

WebApplication app = builder.Build();

// Test Ollama connection on startup
try
{
    OllamaConfiguration? ollamaConfig = app.Services.GetService<OllamaConfiguration>();
    if (ollamaConfig != null)
    {
        OllamaClient? ollamaClient = app.Services.GetService<OllamaClient>();
        if (ollamaClient != null)
        {
            bool isAvailable = await ollamaClient.CheckHealthAsync();
        }
    }
}
catch (Exception)
{
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // Removed HSTS
}

// Removed HTTPS redirection

// Add request logging middleware
app.Use(async (context, next) =>
{
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

static void OnApplicationExit(object sender, EventArgs e)
{
    Log.CloseAndFlush();
}
