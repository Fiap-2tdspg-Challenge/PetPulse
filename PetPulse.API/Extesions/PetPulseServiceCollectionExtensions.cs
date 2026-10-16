using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oracle.EntityFrameworkCore;
using PetPulse.Application.Auth;
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

    /// <summary>
    /// Registra o serviço de autenticação (<see cref="ILoginService"/>) e configura a
    /// validação de token JWT Bearer (<c>AddAuthentication</c> + <c>AddJwtBearer</c>).
    /// </summary>
    /// <remarks>
    /// A chave de assinatura vem de "Jwt:Key" (appsettings.json, ou da variável de
    /// ambiente <c>Jwt__Key</c> em produção — ver azure-cli.sh). Precisa ser a mesma
    /// usada para emitir o token em <see cref="JwtTokenIssuer"/>.
    /// </remarks>
    public static IServiceCollection AddPetPulseAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ILoginService, LoginService>();

        var jwtSection = configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key não configurada. Configure em appsettings.json ou na variável de ambiente Jwt__Key.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"] ?? "PetPulse",
                    ValidAudience = jwtSection["Audience"] ?? "PetPulse.API",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }
}