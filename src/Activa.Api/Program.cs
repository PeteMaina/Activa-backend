using Activa.Api.Middleware;
using Activa.Core.Application.Interfaces;
using Activa.Core.Application.Services;
using Activa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register Core Domain & Infrastructure Services
builder.Services.AddSingleton<IAssetRepository, InMemoryAssetRepository>();
builder.Services.AddScoped<AssetService>();

// Register Enterprise Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Enable Global RFC 7807 Exception Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/healthz/live");
app.MapHealthChecks("/healthz/ready");

app.Run();

public partial class Program { }
