using System.Reflection;
using Microsoft.OpenApi;

namespace PetPulse.Extesions;

/// <summary>
/// Configuração do Swagger/OpenAPI com interface Swagger UI via Swashbuckle.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona geração de documento OpenAPI e metadados para o Swagger UI.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <returns>A mesma instância para encadeamento.</returns>
    public static IServiceCollection addPetPulseSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = configuration.GetSection("Swagger:Title").Value,
                Version = "v1",
                Description = "API para acompanhamento preventivo da saúde de pets, cadastro de tutores, histórico clínico, dispositivos IoT e alertas inteligentes."
            });

            var xml = Path.Combine(
                AppContext.BaseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
            );

            if (File.Exists(xml))
                options.IncludeXmlComments(xml, includeControllerXmlComments: true);
        });

        return services;
    }
}