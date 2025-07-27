using Serilog;

// Suppress security warnings
Environment.SetEnvironmentVariable("ASPNETCORE_SUPPRESSSTATUSMESSAGES", "true");

Console.WriteLine("[STARTUP] Creating WebApplicationBuilder...");
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Console.WriteLine("[STARTUP] WebApplicationBuilder created");

// Configure Kestrel to only use HTTP
Console.WriteLine("[STARTUP] Configuring Kestrel...");
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5011); // HTTP only on different port
});
Console.WriteLine("[STARTUP] Kestrel configured");

// Add services to the container.
Console.WriteLine("[STARTUP] Adding Razor Pages and Blazor services...");
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers(); // Add controller support
Console.WriteLine("[STARTUP] Razor Pages and Blazor services added");

Console.WriteLine("[STARTUP] Building configuration...");
IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
Console.WriteLine("[STARTUP] Configuration built");

Console.WriteLine("[STARTUP] Configuring custom services...");
builder.Services.ConfigureServices();
Console.WriteLine("[STARTUP] Custom services configured");

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
Console.WriteLine("[STARTUP] Configuring Serilog...");
builder.Host.UseSerilog();
Console.WriteLine("[STARTUP] Serilog configured");

Console.WriteLine("[STARTUP] Building WebApplication...");
WebApplication app = builder.Build();
Console.WriteLine("[STARTUP] WebApplication built");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // Removed HSTS
}

// Removed HTTPS redirection
Console.WriteLine("[STARTUP] Configuring HTTP pipeline...");

// Add request logging middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"[REQUEST] {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[RESPONSE] {context.Response.StatusCode} for {context.Request.Path}");
});

app.UseStaticFiles();
app.UseRouting();
app.MapControllers(); // Map controller endpoints
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
Console.WriteLine("[STARTUP] HTTP pipeline configured");

Console.WriteLine("[STARTUP] Starting application...");
app.Run();
