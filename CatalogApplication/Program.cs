using CatalogApplication;
using CatalogApplication.Database;
using CatalogApplication.Middleware;
using CatalogApplication.Repositories;
using CatalogApplication.Seeding;
using CatalogApplication.Utilities;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddCors();
builder.AddDatabase();
builder.AddRepositories();
builder.AddSeeding();

WebApplication app = builder.Build();

app.UseEndpointLogger();
app.ConfigureMiddleware();

if (app.Environment.IsDevelopment()) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapEndpoints();
app.UseHttpsRedirection();

await app.Seed();

app.Run();