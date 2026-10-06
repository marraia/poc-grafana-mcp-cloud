using Marraia.POC.Api.Extensions;
using Marraia.POC.Application;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();

builder.Services.AddApplication(builder.Configuration.GetConnectionString("ProductsDatabase"));
builder.Services.AddControllers();
// Unhandled exceptions are logged and returned as HTTP 500 with a ProblemDetails body.
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
