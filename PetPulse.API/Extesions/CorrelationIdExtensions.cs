using PetPulse.Middlewares;

namespace PetPulse.Extesions;

public static class CorrelationIdExtensions
{
    /// <summary>
    /// Registra o <see cref="CorrelationIdMiddleware"/> na pipeline. Deve ser chamado antes de
    /// UseSerilogRequestLogging para que o log de resumo da requisição também carregue o
    /// CorrelationId.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
