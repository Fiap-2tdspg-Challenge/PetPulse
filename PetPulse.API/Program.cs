using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetPulse.Extesions;
using PetPulse.Health;
using PetPulse.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPetPulseDbContext(builder.Configuration);

builder.Services.AddPetPulseRepositories();

builder.Services.addPetPulseSwagger(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddCheck("PetPulse API", () => HealthCheckResult.Healthy("API está no ar"))
    .AddDbContextCheck<PetPulseContext>("Oracle");

var app = builder.Build();

// Aplica migrations automaticamente ao iniciar
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<PetPulseContext>();
//    db.Database.Migrate();
//}

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