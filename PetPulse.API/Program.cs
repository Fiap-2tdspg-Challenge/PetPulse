using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PetPulse.ExceptionHandling;
using PetPulse.Extesions;
using PetPulse.Health;
using PetPulse.Infrastructure.Persistence;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Segredos (connection string do Oracle e chave JWT) não ficam no appsettings.json versionado.
// Localmente eles vêm do User Secrets do .NET ("dotnet user-secrets set ..."), que fica fora do
// repositório. O ASP.NET Core só carrega User Secrets sozinho quando o ambiente é "Development",
// e este projeto roda localmente como "Local" (ver o comentário do UseExceptionHandler abaixo),
// por isso o carregamento é explícito aqui. Na nuvem, os mesmos valores chegam por variável de
// ambiente (ConnectionStrings__PetPulseOracle e Jwt__Key), definidas como secretas na pipeline.
if (builder.Environment.IsEnvironment("Local"))
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

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

builder.Services.AddPetPulseAuth(builder.Configuration);

// MongoDB: histórico de leituras da coleira IoT (o Oracle guarda só a última leitura).
builder.Services.AddPetPulseMongo(builder.Configuration);

builder.Services.addPetPulseSwagger(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();

// HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck("PetPulse API", () => HealthCheckResult.Healthy("API está no ar"))
    .AddDbContextCheck<PetPulseContext>("Oracle")
    .AddUrlGroup(new Uri("https://fiap.com.br"), name: "FIAP") // ou outro serviço externo real, mas atualmente não consumimos nenhum
    .AddMongoDb(
        databaseNameFactory: _ => MongoServiceCollectionExtensions.GetDatabaseName(builder.Configuration),
        name: "MongoDB",
        timeout: TimeSpan.FromSeconds(10));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

// Tratamento global de exceções: captura qualquer exceção não tratada em qualquer middleware/
// controller abaixo e devolve uma resposta ProblemDetails consistente (ver GlobalExceptionHandler).
// Fica como o primeiro middleware da pipeline para cobrir tudo que vem depois dele.
//
// Observação: a partir do .NET 8, o ASP.NET Core registra automaticamente a "Developer
// Exception Page" como o middleware mais externo da pipeline sempre que o ambiente é
// literalmente "Development" — e ela intercepta a exceção antes mesmo de chegar aqui,
// devolvendo a página/JSON de debug "crus" (stack trace, headers) em vez da resposta
// mapeada pelo GlobalExceptionHandler. Por isso o perfil local (launchSettings.json) usa
// ASPNETCORE_ENVIRONMENT=Local em vez de "Development": assim o GlobalExceptionHandler é
// sempre quem responde, mesmo rodando localmente com "dotnet run".
app.UseExceptionHandler();

// Garante um id de correlação por requisição (reaproveitado do header X-Correlation-Id
// quando presente, ou gerado como novo Guid) e o injeta no Serilog LogContext antes do
// log de resumo da requisição, para além da correlação por trace/span já existente.
app.UseCorrelationId();

app.UseSerilogRequestLogging();

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

// UseAuthentication precisa vir antes de UseAuthorization: é ela que lê o header
// "Authorization: Bearer {token}", valida o JWT e popula o HttpContext.User — sem
// isso, [Authorize] sempre rejeitaria a requisição mesmo com um token válido.
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();