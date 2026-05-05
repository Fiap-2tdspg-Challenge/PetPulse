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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PetPulse API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();