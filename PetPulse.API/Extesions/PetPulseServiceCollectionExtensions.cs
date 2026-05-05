using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore;
using PetPulse.Application.Services;
using PetPulse.Infrastructure.Persistence;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Extesions;

public static class PetPulseServiceCollectionExtensions
{
    public static IServiceCollection AddPetPulseDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "PetPulseOracle")
    {
        var oracleConnectionString = configuration.GetConnectionString(connectionStringName)
                                     ?? throw new InvalidOperationException(
                                         $"Connection string '{connectionStringName}' não encontrada. Configure em appsettings.json ou no ambiente.");

        services.AddDbContext<PetPulseContext>(options =>
            options.UseOracle(
                oracleConnectionString,
                oracleOptions => oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)
            ));

        return services;
    }

    public static IServiceCollection AddPetPulseRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IHistoricoClinicoRepository, HistoricoClinicoRepository>();
        services.AddScoped<IDispositivoIotRepository, DispositivoIotRepository>();
        services.AddScoped<IAlertaInteligenteRepository, AlertaInteligenteRepository>();

        return services;
    }
}