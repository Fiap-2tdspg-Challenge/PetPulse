using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using PetPulse.Domain.Exceptions;
using PetPulse.ExceptionHandling;
using PetPulse.Middlewares;

namespace PetPulse.API.Tests.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_QuandoDomainException_DeveRetornar400()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Production);
        var exception = new DomainException("Nome do pet não pode ser vazio.");

        // Act
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var problem = await LerProblema(context);
        Assert.Equal("Não foi possível concluir a operação", problem.Title);
        Assert.Equal(exception.Message, problem.Detail);
        Assert.False(problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task TryHandleAsync_QuandoInvalidOperationException_DeveRetornar400()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Production);
        var exception = new InvalidOperationException("Usuário não encontrado.");

        // Act
        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var problem = await LerProblema(context);
        Assert.Equal("Não foi possível concluir a operação", problem.Title);
        Assert.Equal(exception.Message, problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_QuandoArgumentException_DeveRetornar400()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(context, new ArgumentException("campo inválido"), CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var problem = await LerProblema(context);
        Assert.Equal("Requisição inválida", problem.Title);
    }

    [Fact]
    public async Task TryHandleAsync_QuandoKeyNotFoundException_DeveRetornar404()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(context, new KeyNotFoundException("não encontrado"), CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        var problem = await LerProblema(context);
        Assert.Equal("Recurso não encontrado", problem.Title);
    }

    [Fact]
    public async Task TryHandleAsync_QuandoUnauthorizedAccessException_DeveRetornar401()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(context, new UnauthorizedAccessException(), CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        var problem = await LerProblema(context);
        Assert.Equal("Não autorizado", problem.Title);
    }

    [Fact]
    public async Task TryHandleAsync_QuandoExcecaoNaoMapeadaEmProducao_DeveRetornar500SemExporDetalheInterno()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(context, new Exception("segredo interno"), CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var problem = await LerProblema(context);
        Assert.Equal("Erro interno do servidor", problem.Title);
        Assert.Equal("Ocorreu um erro inesperado. Tente novamente mais tarde.", problem.Detail);
        Assert.DoesNotContain("segredo interno", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_EmAmbienteDeDesenvolvimento_DeveIncluirTraceId()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Development);
        context.TraceIdentifier = "trace-aula";

        // Act
        await handler.TryHandleAsync(context, new DomainException("falhou"), CancellationToken.None);

        // Assert
        var problem = await LerProblema(context);
        Assert.True(problem.Extensions.TryGetValue("traceId", out var traceId));
        Assert.Equal("trace-aula", traceId?.ToString());
    }

    [Fact]
    public async Task TryHandleAsync_ComCorrelationIdJaDefinidoNaResposta_DeveIncluiloMesmoEmProducao()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Production);
        context.Response.Headers[CorrelationIdMiddleware.HeaderName] = "correlation-abc-123";

        // Act
        await handler.TryHandleAsync(context, new Exception("falha"), CancellationToken.None);

        // Assert
        var problem = await LerProblema(context);
        Assert.True(problem.Extensions.TryGetValue("correlationId", out var correlationId));
        Assert.Equal("correlation-abc-123", correlationId?.ToString());
    }

    [Fact]
    public async Task TryHandleAsync_SemCorrelationIdNaResposta_NaoDeveIncluirCorrelationIdNoCorpo()
    {
        // Arrange
        var (handler, context) = CriarHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(context, new Exception("falha"), CancellationToken.None);

        // Assert
        var problem = await LerProblema(context);
        Assert.False(problem.Extensions.ContainsKey("correlationId"));
    }

    private static (GlobalExceptionHandler Handler, DefaultHttpContext Context) CriarHandler(string environmentName)
    {
        var logger = new Mock<ILogger<GlobalExceptionHandler>>();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(environmentName);

        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() }
        };
        context.Request.Path = "/api/pets";

        return (new GlobalExceptionHandler(logger.Object, environment.Object), context);
    }

    private static async Task<ProblemDetails> LerProblema(HttpContext context)
    {
        context.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problem);
        return problem!;
    }
}
