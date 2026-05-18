using Microsoft.EntityFrameworkCore;
using PetPulse.Extesions;
using PetPulse.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPetPulseDbContext(builder.Configuration);

builder.Services.AddPetPulseRepositories();

builder.Services.addPetPulseSwagger(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();

var app = builder.Build();

// Aplica migrations automaticamente ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PetPulseContext>();
    db.Database.Migrate();
}

app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PetPulse API v1");
    });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();