using TechMove.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HTTP Context Accessor (for accessing session and user context)
builder.Services.AddHttpContextAccessor();

// Add distributed memory cache for session storage
builder.Services.AddDistributedMemoryCache();

// Add session support with enhanced security
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Name = ".TechMove.Session";
});

// Get API base URL from configuration or environment variable
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ??
                 Environment.GetEnvironmentVariable("API_BASE_URL") ??
                 "https://localhost:7001/";

// Ensure base URL ends with slash
if (!apiBaseUrl.EndsWith("/"))
{
    apiBaseUrl += "/";
}

// Register API Service (instead of repositories)
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "TechMove-MVC-Client");
});

// Add memory cache for caching data
builder.Services.AddMemoryCache();

// Add response caching for improved performance
builder.Services.AddResponseCaching();

// Add health checks
builder.Services.AddHealthChecks();

// Add logging with configuration
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventSourceLogger();
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseResponseCaching(); // Add response caching middleware
app.UseSession(); // Add session middleware
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Log application startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("========================================");
logger.LogInformation("TechMove MVC Client Started Successfully");
logger.LogInformation("========================================");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("API Base URL: {ApiBaseUrl}", apiBaseUrl);
logger.LogInformation("Session Timeout: 30 minutes");
logger.LogInformation("Health Check: /health");
logger.LogInformation("========================================");

// REMOVED: Database migrations and direct database access
// The MVC client no longer connects to the database directly
// All data operations go through the Web API

app.Run();