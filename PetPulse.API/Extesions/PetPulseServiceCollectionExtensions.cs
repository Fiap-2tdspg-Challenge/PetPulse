using Microsoft.EntityFrameworkCore;
using PetPulse.Application.Services;
using PetPulse.Infrastructure.Persistence;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Extesions;

/// <summary>
/// Extensões para registrar persistência e repositórios da solução PetPulse na injeção de dependências.
/// </summary>
public static class PetPulseServiceCollectionExtensions
{
    /// <summary>
    /// Registra o <see cref="PetPulseContext"/> com Oracle Database.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <param name="connectionStringName">Chave em ConnectionStrings para Oracle.</param>
    /// <returns>A mesma instância de <see cref="IServiceCollection"/> para encadeamento.</returns>
    /// <exception cref="InvalidOperationException">Quando a connection string não for encontrada.</exception>
    public static IServiceCollection AddPetPulseDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "PetPulseOracle")
    {
        var oracleConnectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' não encontrada. Configure em appsettings.json ou no ambiente.");

        services.AddDbContext<PetPulseContext>(options =>
            options.UseOracle(oracleConnectionString));

        return services;
    }

    /// <summary>
    /// Registra todas as implementações de repositório como Scoped, um por requisição HTTP.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <returns>A mesma instância de <see cref="IServiceCollection"/> para encadeamento.</returns>
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