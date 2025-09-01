using Serilog;

// Suppress security warnings
Environment.SetEnvironmentVariable("ASPNETCORE_SUPPRESSSTATUSMESSAGES", "true");

WebApplicationOptions options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
};

WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

// Add services to the container.
Console.WriteLine("[STARTUP] Adding Razor Pages and Blazor services...");
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers(); // Add controller support
Console.WriteLine("[STARTUP] Razor Pages and Blazor services added");

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

// Register IConfiguration for dependency injection
builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.ConfigureServices();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

// Ensure proper disposal on exit:
AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    Log.CloseAndFlush();
};

// Register your game services
builder.Host.UseSerilog();

WebApplication app = builder.Build();

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
    Console.WriteLine($"[REQUEST] {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[RESPONSE] {context.Response.StatusCode} for {context.Request.Path}");
});

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

Console.WriteLine("[STARTUP] Starting application...");
app.Run();
