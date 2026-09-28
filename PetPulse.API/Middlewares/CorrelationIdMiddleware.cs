using Serilog.Context;

namespace PetPulse.Middlewares;

/// <summary>
/// Garante um identificador de correlação por requisição: reaproveita o valor recebido no
/// header <see cref="HeaderName"/> (útil quando o chamador/gateway já propaga um id) ou gera
/// um novo Guid quando nenhum é informado. O valor é devolvido no header de resposta e
/// empurrado para o Serilog LogContext, de forma que toda linha de log emitida durante o
/// processamento da requisição (incluindo o log de resumo do UseSerilogRequestLogging)
/// carregue a propriedade "CorrelationId" — complementando a correlação por trace/span que
/// já existia (Enrich.WithSpan) com um id de requisição de ponta a ponta.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
