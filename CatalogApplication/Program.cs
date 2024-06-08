using CatalogApplication.Database;
using CatalogApplication.Repositories;
using CatalogApplication.Seeding;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IDapperContext, DapperContext>();
builder.Services.AddSingleton<BrandRepository>();
builder.Services.AddSingleton<CategoryRepository>();
builder.Services.AddSingleton<InventoryRepository>();
builder.Services.AddSingleton<ProductDetailsRepository>();
builder.Services.AddSingleton<ProductSearchRepository>();
builder.Services.AddScoped<SeedingService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var seeder = app.Services.GetRequiredService<SeedingService>();
await seeder.SeedDatabase();

app.Run();