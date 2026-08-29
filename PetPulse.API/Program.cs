using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PetPulse.Extesions;
using PetPulse.Health;
using PetPulse.Infrastructure.Persistence;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/petpulse-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

builder.Services.AddPetPulseDbContext(builder.Configuration);

builder.Services.AddPetPulseRepositories();

builder.Services.addPetPulseSwagger(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();

// HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck("PetPulse API", () => HealthCheckResult.Healthy("API está no ar"))
    .AddDbContextCheck<PetPulseContext>("Oracle")
    .AddUrlGroup(new Uri("https://fiap.com.br"), name: "FIAP"); // ou outro serviço externo real, mas atualmente não consumimos nenhum

builder.Services.AddProblemDetails();

// Telemetria
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("PetPulse.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Routing",
            "Microsoft.AspNetCore.Server.Kestrel",
            "System.Net.Http",
            "System.Runtime")
        .AddPrometheusExporter());

var app = builder.Build();

app.UseSerilogRequestLogging();

// Aplica migrations automaticamente ao iniciar - Inciar apenas caso for a primeira vez (não contendo o banco)
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<PetPulseContext>();
//    db.Database.Migrate();
//}

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
});

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PetPulse API v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();