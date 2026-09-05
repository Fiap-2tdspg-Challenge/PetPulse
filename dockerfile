# =============================================================
# PetPulse – Challenge FIAP 2026
# Multi-stage build: SDK .NET 10 → Runtime ASP.NET 10
# =============================================================

# ---------------------------------------------------------------
# Stage 1: Build
# ---------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia apenas os arquivos de projeto de PRODUÇÃO para cache de layers do restore
# (os projetos de teste ficam de fora do build de imagem — não são necessários em runtime
# e não estão presentes neste estágio, então não podem constar do restore)
COPY PetPulse.API/PetPulse.API.csproj             PetPulse.API/
COPY PetPulse.Application/PetPulse.Application.csproj   PetPulse.Application/
COPY PetPulse.Domain/PetPulse.Domain.csproj         PetPulse.Domain/
COPY PetPulse.Infrastructure/PetPulse.Infrastructure.csproj PetPulse.Infrastructure/

RUN dotnet restore PetPulse.API/PetPulse.API.csproj

# Copia o restante do código-fonte (o .dockerignore já exclui testes, bin/obj, .git etc.)
COPY . .

# Publica a API em modo Release
RUN dotnet publish PetPulse.API/PetPulse.API.csproj \
    -c Release \
    -o /app/publish

# ---------------------------------------------------------------
# Stage 2: Runtime
# ---------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Cria usuário não-root para segurança
RUN groupadd --system appgroup \
 && useradd --system --gid appgroup --no-create-home appuser

# Copia os artefatos publicados
COPY --from=build /app/publish .

# Define o usuário não-root
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "PetPulse.API.dll"]
