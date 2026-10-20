using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PetPulse.Application.Services;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Extesions;

/// <summary>
/// Registra o MongoDB (histórico de leituras da coleira IoT) na injeção de dependências.
/// Mesmo modelo do repositório de referência da disciplina (Recommenda:
/// MongoServiceCollectionExtensions).
/// </summary>
public static class MongoServiceCollectionExtensions
{
    public const string DefaultDatabaseName = "petpulse";

    private static int _convencoesRegistradas;

    public static IServiceCollection AddPetPulseMongo(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PetPulseMongo");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'PetPulseMongo' não encontrada. Configure no User Secrets (local) " +
                "ou na variável de ambiente ConnectionStrings__PetPulseMongo.");

        var databaseName = GetDatabaseName(configuration);

        RegistrarConvencoes();

        // Cliente e banco são thread-safe e devem ser únicos na aplicação (recomendação do driver).
        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

        services.AddScoped<ILeituraIotRepository, LeituraIotRepository>();

        return services;
    }

    public static string GetDatabaseName(IConfiguration configuration) =>
        configuration.GetSection("MongoDatabase:Name").Value ?? DefaultDatabaseName;

    private static void RegistrarConvencoes()
    {
        if (Interlocked.Exchange(ref _convencoesRegistradas, 1) == 1)
            return;

        // Guid gravado como UUID padrão — sem isso o driver 3.x recusa serializar Guid.
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        // Documentos em camelCase (mesmo formato do JSON da coleira) e tolerantes a campos que
        // ainda não existem na classe — se o firmware passar a enviar um sensor novo, as leituras
        // antigas e novas continuam sendo lidas normalmente.
        ConventionRegistry.Register(
            "PetPulseMongoConventions",
            new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            },
            type => type.Namespace?.StartsWith("PetPulse", StringComparison.Ordinal) == true);
    }
}
