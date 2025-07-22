using Serilog;

// Suppress security warnings
Environment.SetEnvironmentVariable("ASPNETCORE_SUPPRESSSTATUSMESSAGES", "true");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to only use HTTP
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5011); // HTTP only on different port
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

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
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
