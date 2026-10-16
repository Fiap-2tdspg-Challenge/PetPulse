using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using PetPulse.Domain.Exceptions;
using PetPulse.Middlewares;

namespace PetPulse.ExceptionHandling;

/// <summary>
/// Último nível de tratamento de exceções da API: captura qualquer exceção não tratada que
/// escape dos controllers e devolve ao cliente uma resposta ProblemDetails (RFC 7807)
/// consistente, mapeando cada tipo de exceção para o status HTTP correspondente (em vez de
/// tratar tudo como um 500 genérico). Registrado via AddExceptionHandler + UseExceptionHandler
/// no Program.cs, complementando o AddProblemDetails já existente.
///
/// Expõe dois identificadores de rastreio na resposta:
/// - "traceId": id de trace do ASP.NET Core (Activity/TraceIdentifier), incluído apenas em
///   Development, útil para localizar a requisição nos logs durante o desenvolvimento.
/// - "correlationId": id de correlação de ponta a ponta gerado/propagado pelo
///   CorrelationIdMiddleware, incluído sempre que presente (também em produção), pois é o
///   identificador que o suporte usa para localizar a requisição nos logs estruturados (Serilog).
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(exception, "Exceção não tratada: {Message} : TraceId {TraceId}", exception.Message, traceId);

        var (statusCode, title, detail) = MapException(exception, environment);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (environment.IsDevelopment())
        {
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        var correlationId = httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            problem.Extensions["correlationId"] = correlationId;
        }

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string? Detail) MapException(
        Exception exception,
        IHostEnvironment environment)
    {
        return exception switch
        {
            ArgumentNullException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
            ArgumentException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
            DomainException e => (StatusCodes.Status400BadRequest, "Não foi possível concluir a operação", e.Message),
            InvalidOperationException e => (StatusCodes.Status400BadRequest, "Não foi possível concluir a operação", e.Message),
            KeyNotFoundException e => (StatusCodes.Status404NotFound, "Recurso não encontrado", e.Message),
            UnauthorizedAccessException e => (StatusCodes.Status401Unauthorized, "Não autorizado", e.Message),
            OracleException e => (StatusCodes.Status502BadGateway, "Banco indisponível", e.Message),
            _ => MapUnhandled(environment, exception)
        };
    }

    private static (int StatusCode, string Title, string? Detail) MapUnhandled(
        IHostEnvironment environment,
        Exception exception)
    {
        return (
            StatusCodes.Status500InternalServerError,
            "Erro interno do servidor",
            "Ocorreu um erro inesperado. Tente novamente mais tarde.");
    }
}
