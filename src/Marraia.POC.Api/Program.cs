using Marraia.POC.Api.Extensions;
using Marraia.POC.Application;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();

builder.Services.AddApplication();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
