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

            // A partir do Swashbuckle 10 / Microsoft.OpenApi 2.x (o par usado neste projeto,
            // alinhado ao .NET 10), o modelo de segurança mudou: OpenApiSecurityScheme não tem
            // mais a propriedade "Reference", e AddSecurityRequirement passou a receber um
            // Func<OpenApiDocument, OpenApiSecurityRequirement> — o requirement é montado a
            // partir do próprio documento, referenciando o esquema por id via
            // OpenApiSecuritySchemeReference (em vez do antigo OpenApiReference/ReferenceType).
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Token JWT obtido em POST /api/auth/login. Informe apenas o token " +
                              "(o prefixo \"Bearer \" é adicionado automaticamente pelo Swagger)."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });

            options.OrderActionsBy(apiDesc =>
            {
                var controller = apiDesc.ActionDescriptor.RouteValues["controller"];

                var controllerOrder = controller switch
                {
                    "Auth" => "00",
                    "Usuario" => "01",
                    "Pet" => "02",
                    "HistoricoClinico" => "03",
                    "DispositivoIot" => "04",
                    "LeituraIot" => "05",
                    "AlertaInteligente" => "06",
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