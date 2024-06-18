using CatalogApplication;
using CatalogApplication.Database;
using CatalogApplication.Middleware;
using CatalogApplication.Repositories;
using CatalogApplication.Seeding;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
EndpointLogger.Logger = builder.Services
    .BuildServiceProvider()
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger<EndpointLogger>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors( static options => {
    options.AddDefaultPolicy( static cors => cors
        .WithOrigins( "https://localhost:7221", "https://localhost:7212" )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials() );
} );
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IDapperContext, DapperContext>();
builder.Services.AddSingleton<BrandRepository>();
builder.Services.AddSingleton<CategoryRepository>();
builder.Services.AddSingleton<InventoryRepository>();
builder.Services.AddSingleton<ProductDetailsRepository>();
builder.Services.AddSingleton<ProductSearchRepository>();
builder.Services.AddSingleton<SeedingService>();

WebApplication app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.MapEndpoints();
app.UseHttpsRedirection();

var seeder = app.Services.GetRequiredService<SeedingService>();
await seeder.SeedDatabase();

app.Run();