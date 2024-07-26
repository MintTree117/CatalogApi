using CatalogApplication;
using CatalogApplication.Database;
using CatalogApplication.Middleware;
using CatalogApplication.Repositories.Features;
using CatalogApplication.Seeding;
using CatalogApplication.Utilities;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.ConfigureCors();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IDapperContext, DapperContext>();
builder.Services.AddSingleton<BrandRepository>();
builder.Services.AddSingleton<CategoryRepository>();
builder.Services.AddSingleton<InventoryRepository>();
builder.Services.AddSingleton<ProductDetailsRepository>();
builder.Services.AddSingleton<ProductSearchRepository>();
builder.Services.AddSingleton<ProductSpecialsRepository>();
builder.Services.AddSingleton<SeedingService>();

WebApplication app = builder.Build();

EndpointLogger.InitializeLogger( app.Services.GetRequiredService<ILoggerFactory>() );

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment()) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapEndpoints();
app.UseHttpsRedirection();

string[] origins = builder.Configuration.GetSection( "AllowedOrigins" ).Get<string[]>()
    ?? throw new Exception( "Failed to get AllowedOrigins from configuration during startup." );

foreach ( var s in origins )
{
    EndpointLogger.LogError( s );
}

var seeder = app.Services.GetRequiredService<SeedingService>();
await seeder.SeedDatabase();

app.Run();