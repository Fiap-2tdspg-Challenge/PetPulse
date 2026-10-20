using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetPulse.Infrastructure.Persistence;

namespace PetPulse.API.Tests.Integration;

/// <summary>
/// Sobe a API real em memória (TestServer) para os testes de integração: mesmo Program.cs,
/// mesma pipeline de middlewares (exceções, autenticação JWT, autorização, HATEOAS, health checks).
/// </summary>
/// <remarks>
/// Só o que depende de infraestrutura externa é trocado, para os testes rodarem em qualquer
/// máquina e na pipeline do Azure DevOps sem banco:
/// - Oracle → EF Core InMemory (um banco isolado por instância da factory);
/// - health checks externos (MongoDB e URL da FIAP) são removidos do /health.
/// As configurações obrigatórias (connection strings e chave JWT) são fornecidas com valores de
/// teste via <c>UseSetting</c>, que já ficam disponíveis enquanto o Program.cs registra os serviços.
/// </remarks>
public sealed class PetPulseApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"PetPulseIntegracao-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:PetPulseOracle", "Data Source=localhost:1521/XEPDB1;User ID=teste;Password=teste;");
        builder.UseSetting("ConnectionStrings:PetPulseMongo", "mongodb://localhost:27017");
        builder.UseSetting("Jwt:Key", "chave-de-teste-de-integracao-petpulse-com-mais-de-32-caracteres");

        builder.ConfigureTestServices(services =>
        {
            // Troca o provedor Oracle pelo InMemory (remove as duas registrações do AddDbContext).
            services.RemoveAll<DbContextOptions<PetPulseContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<PetPulseContext>>();
            services.AddDbContext<PetPulseContext>(options => options.UseInMemoryDatabase(_databaseName));

            // Mantém no /health só as checagens que não dependem de rede (API e banco relacional).
            services.Configure<HealthCheckServiceOptions>(options =>
            {
                var externas = options.Registrations
                    .Where(registro => registro.Name is "MongoDB" or "FIAP")
                    .ToList();

                foreach (var registro in externas)
                    options.Registrations.Remove(registro);
            });
        });
    }
}
