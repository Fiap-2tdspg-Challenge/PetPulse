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
                Title = configuration.GetSection("Swagger:Title").Value ?? "PetPulse API",
                Version = configuration.GetSection("Swagger:Version").Value ?? "v1",
                Description = configuration.GetSection("Swagger:Description").Value
                              ?? "API para acompanhamento preventivo da saúde de pets."
            });

            options.OrderActionsBy(apiDesc =>
            {
                var controller = apiDesc.ActionDescriptor.RouteValues["controller"];

                var controllerOrder = controller switch
                {
                    "Usuario" => "01",
                    "Pet" => "02",
                    "HistoricoClinico" => "03",
                    "DispositivoIot" => "04",
                    "AlertaInteligente" => "05",
                    _ => "99"
                };

                var methodOrder = apiDesc.HttpMethod switch
                {
                    "GET" => "01",
                    "POST" => "02",
                    "PUT" => "03",
                    "DELETE" => "04",
                    _ => "99"
                };

                return $"{controllerOrder}_{methodOrder}_{apiDesc.RelativePath}";
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