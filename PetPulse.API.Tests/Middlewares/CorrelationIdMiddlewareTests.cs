using Microsoft.AspNetCore.Http;
using PetPulse.Middlewares;

namespace PetPulse.API.Tests.Middlewares;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SemHeaderNaRequisicao_DeveGerarNovoCorrelationIdEDevolverNoHeaderDeResposta()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task InvokeAsync_ComHeaderNaRequisicao_DeveReutilizarOMesmoCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "correlation-existente-123";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(
            "correlation-existente-123",
            context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_DeveChamarProximoMiddlewareDaPipeline()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var proximoChamado = false;
        var middleware = new CorrelationIdMiddleware(_ =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(proximoChamado);
    }
}
