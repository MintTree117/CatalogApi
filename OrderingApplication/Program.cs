using OrderingApplication.Extentions;
using OrderingInfrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

builder.ConfigureLogging();
builder.ConfigureInfrastructure();
builder.ConfigureFeatures();
builder.ConfigureSwagger();
builder.ConfigureCors();

WebApplication app = builder.Build();

app.HandleSwagger();
app.MapEndpoints();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.AddCustomMiddleware();
app.Run();