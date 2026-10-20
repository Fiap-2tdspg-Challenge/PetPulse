# PetPulse API — Challenge FIAP 2026

> API RESTful de saúde preditiva para pets, desenvolvida em ASP.NET Core com Oracle Database.

---

## Sumário

- [Descrição do Projeto](#descrição-do-projeto)
- [Benefícios para o Negócio](#benefícios-para-o-negócio)
- [Tecnologias utilizadas](#tecnologias-utilizadas)
- [Arquitetura do projeto](#arquitetura-do-projeto)
- [Entidades principais](#entidades-principais)
- [Relacionamentos](#relacionamentos)
- [Configuração do banco Oracle](#configuração-do-banco-oracle)
- [Migrations](#migrations)
- [Observabilidade e Monitoramento](#observabilidade-e-monitoramento)
- [Autenticação (JWT)](#autenticação-jwt)
- [MongoDB — histórico de leituras IoT](#mongodb--histórico-de-leituras-iot)
- [Paginação, ordenação, filtros e HATEOAS](#paginação-ordenação-filtros-e-hateoas)
- [Testes automatizados](#testes-automatizados)
- [Arquitetura de Deploy (ACR + ACI + Key Vault)](#arquitetura-de-deploy-acr--aci--key-vault)
- [Como executar](#como-executar)
- [Deploy em Nuvem (ACR + ACI + Key Vault)](#deploy-em-nuvem-acr--aci--key-vault)
- [Portas e serviços](#portas-e-serviços)
- [Variáveis de ambiente](#variáveis-de-ambiente)
- [Endpoints disponíveis](#endpoints-disponíveis)
- [Ordem recomendada para testar](#ordem-recomendada-para-testar)
- [JSONs para teste](#jsons-para-teste)
- [Testes de consulta](#testes-de-consulta)
- [Testes de atualização](#testes-de-atualização)
- [Testes de erro](#testes-de-erro)
- [Consultas para conferir no Oracle](#consultas-para-conferir-no-oracle)
- [Códigos HTTP utilizados](#códigos-http-utilizados)
- [Observações importantes](#observações-importantes)

---

## Descrição do Projeto

O **PetPulse** é uma plataforma de **saúde preditiva para pets** que combina monitoramento via dispositivos IoT com inteligência de dados clínicos para oferecer acompanhamento preventivo contínuo da saúde animal.

A solução foi desenvolvida como uma **API RESTful em ASP.NET Core**, permitindo que tutores cadastrem seus pets, registrem histórico clínico completo (vacinas, consultas, exames e medicamentos), vinculem coleiras ou dispositivos inteligentes de monitoramento e recebam **alertas automáticos** gerados com base em dados comportamentais, fisiológicos e clínicos do animal.

O sistema é capaz de coletar métricas em tempo real — como frequência cardíaca, nível de atividade e pressão — diretamente do dispositivo IoT vinculado ao pet, cruzar essas informações com o histórico clínico e gerar **alertas inteligentes classificados por nível de risco**, indicando recomendações precisas ao tutor.

A arquitetura segue princípios de **Clean Architecture**, garantindo alta manutenibilidade, escalabilidade e separação clara de responsabilidades entre as camadas de domínio, aplicação, infraestrutura e apresentação. O banco de dados utilizado é o **Oracle**, com gerenciamento de schema via **Entity Framework Core Migrations**.

---

## Benefícios para o Negócio

| Benefício | Descrição |
|---|---|
| **Prevenção e redução de custos** | Ao identificar riscos de saúde de forma precoce, a plataforma reduz internações de emergência e tratamentos tardios, que costumam ser significativamente mais caros. |
| **Fidelização de clientes** | Clínicas veterinárias e pet shops que adotam o PetPulse oferecem um diferencial competitivo, criando um vínculo contínuo com o tutor além da consulta presencial. |
| **Geração de receita recorrente** | O modelo baseado em dispositivos IoT e planos de monitoramento abre oportunidade para receita por assinatura (SaaS/hardware-as-a-service). |
| **Dados clínicos centralizados** | O histórico completo do pet em um único sistema elimina retrabalho, perda de informações e melhora a qualidade dos atendimentos veterinários. |
| **Escalabilidade da solução** | A arquitetura em camadas e o uso de Oracle + EF Core permitem crescimento horizontal da base de usuários e integração com sistemas veterinários existentes. |
| **Inteligência preditiva** | Os alertas gerados cruzam dados de IoT com histórico clínico, gerando valor real ao tutor e abrindo espaço para evolução com modelos de machine learning. |
| **Diferenciação no mercado pet** | O mercado pet brasileiro movimenta mais de R$ 60 bilhões por ano. Soluções de tecnologia para saúde animal ainda são escassas, posicionando o PetPulse em um segmento de alto crescimento. |

---

## Tecnologias utilizadas

* C#
* ASP.NET Core Web API
* Entity Framework Core
* Oracle Database
* MongoDB (MongoDB.Driver) — histórico de leituras da coleira IoT
* EF Core Migrations
* Swagger / OpenAPI
* Serilog (logging estruturado)
* OpenTelemetry (tracing e métricas)
* Microsoft.Extensions.Diagnostics.HealthChecks
* xUnit / Moq (testes automatizados)
* Microsoft.EntityFrameworkCore.InMemory (testes de repositório)
* Rider / Visual Studio
* Docker / Docker Compose
* Azure CLI / Microsoft Azure
* Azure Container Registry (ACR) / Azure Container Instances (ACI)

---

## Arquitetura do projeto

A solução foi organizada em quatro projetos principais (mais os projetos de teste correspondentes):

```text
PetPulse
├── PetPulse.API
├── PetPulse.Application
├── PetPulse.Domain
├── PetPulse.Infrastructure
├── PetPulse.API.Tests
├── PetPulse.Application.Tests
├── PetPulse.Domain.Tests
└── PetPulse.Infrastructure.Tests
```

### PetPulse.API

Camada responsável pela exposição dos endpoints REST da aplicação.

Contém:

* Controllers
* Program.cs
* Configurações do Swagger
* Injeção de dependência
* appsettings.json

### PetPulse.Application

Camada responsável pelos contratos e objetos de transferência de dados.

Contém:

* DTOs de request e response
* Interfaces de repositórios/serviços

### PetPulse.Domain

Camada de domínio da aplicação.

Contém:

* Entidades principais
* Enums
* BaseEntity
* Regras básicas de validação das entidades

### PetPulse.Infrastructure

Camada responsável pela persistência de dados.

Contém:

* DbContext
* Configurations do Entity Framework
* Repositories
* Migrations

---

## Entidades principais

A API trabalha com as seguintes entidades:

### Usuario

Representa o tutor ou responsável pelo pet.

Principais campos:

* Id
* Nome
* CPF
* Email
* Senha
* Telefone
* Endereço
* Data de cadastro

### Pet

Representa o animal cadastrado no sistema.

Principais campos:

* Id
* UsuarioId
* Nome
* Espécie
* Raça
* Data de nascimento
* Peso
* Sexo
* Castrado
* Porte

### HistoricoClinico

Representa registros clínicos do pet, como vacinas, consultas, medicamentos, exames e observações.

Principais campos:

* Id
* PetId
* Tipo de registro
* Descrição
* Data do registro
* Data de retorno
* Profissional ou clínica
* Observações

### DispositivoIot

Representa uma coleira ou dispositivo IoT vinculado ao pet.

Principais campos:

* Id
* PetId
* Data de vinculação
* Intervalo de coleta
* Frequência cardíaca
* Nível de atividade
* Pressão
* Data da última leitura
* Status

### AlertaInteligente

Representa alertas gerados pelo sistema.

Principais campos:

* Id
* PetId
* Tipo de alerta
* Nível de risco
* Origem do alerta
* Mensagem
* Recomendação
* Data de geração
* Status

---

## Relacionamentos

```text
Usuario 1:N Pet
Pet 1:N HistoricoClinico
Pet 1:1 DispositivoIot
Pet 1:N AlertaInteligente
```

Ou seja:

* Um usuário pode ter vários pets.
* Um pet pertence a um usuário.
* Um pet pode ter vários registros clínicos.
* Um pet pode ter um dispositivo IoT vinculado.
* Um pet pode ter vários alertas inteligentes.

---

## Configuração do banco Oracle

A connection string **não fica versionada** no `appsettings.json` — senhas e chaves nunca vão para o GitHub. A API lê a chave `ConnectionStrings:PetPulseOracle`, que vem de um destes lugares conforme a forma de execução:

| Onde a API roda | De onde vem a connection string |
|---|---|
| Local, com `dotnet run` | **User Secrets** do .NET — arquivo fora do repositório, só na sua máquina (ver Opção 1 em "Como executar") |
| Local, com Docker Compose | Arquivo `.env` (no `.gitignore`), lido pelo `docker-compose.yml` (ver Opção 2) |
| Nuvem (Azure) | Variável de ambiente `ConnectionStrings__PetPulseOracle`, definida como **secreta** (cadeado) na pipeline do Azure DevOps |

Formato da connection string (Oracle FIAP):

```text
Data Source=oracle.fiap.com.br:1521/orcl;User ID=SEU_RM;Password=SUA_SENHA;
```

---

## Migrations

A API utiliza **EF Core Migrations** para criação do banco da parte .NET.


Caso precise criar as migrations e testar no seu banco segue os comandos abaixo

### Criar migration

```powershell
dotnet ef migrations add Initial --project PetPulse.Infrastructure\PetPulse.Infrastructure.csproj --startup-project PetPulse.API\PetPulse.API.csproj --context PetPulse.Infrastructure.Persistence.PetPulseContext --configuration Debug --output-dir Migrations
```

### Aplicar migration no Oracle

```powershell
dotnet ef database update --project PetPulse.Infrastructure\PetPulse.Infrastructure.csproj --startup-project PetPulse.API\PetPulse.API.csproj --context PetPulse.Infrastructure.Persistence.PetPulseContext
```

### Remover última migration, se necessário

```powershell
dotnet ef migrations remove --project PetPulse.Infrastructure\PetPulse.Infrastructure.csproj --startup-project PetPulse.API\PetPulse.API.csproj --context PetPulse.Infrastructure.Persistence.PetPulseContext
```

## Observabilidade e Monitoramento

A API implementa monitoramento e observabilidade seguindo os requisitos da Sprint 3: Health Checks, logging estruturado e tracing/métricas distribuídas.

### Health Check

```http
GET /health
```

Verifica a saúde da API, a conectividade com o banco Oracle e com o MongoDB e a disponibilidade de um serviço externo. Utiliza `Microsoft.Extensions.Diagnostics.HealthChecks`.

Checks configurados:

| Check | O que verifica |
|---|---|
| `PetPulse API` | Self-check simples, confirma que a API está no ar |
| `Oracle` | Conectividade com o banco via `PetPulseContext` (EF Core) |
| `FIAP` | Disponibilidade de um serviço HTTP externo |
| `MongoDB` | Conectividade com o MongoDB (histórico de leituras IoT) |

Exemplo de resposta:

```json
{
  "status": "Healthy",
  "duration": "00:00:00.1234567",
  "checks": [
    { "name": "PetPulse API", "status": "Healthy", "description": "API está no ar", "duration": "00:00:00.0000010", "error": null },
    { "name": "Oracle", "status": "Healthy", "description": null, "duration": "00:00:00.0987654", "error": null },
    { "name": "FIAP", "status": "Healthy", "description": null, "duration": "00:00:00.0456789", "error": null },
    { "name": "MongoDB", "status": "Healthy", "description": null, "duration": "00:00:00.0123456", "error": null }
  ]
}
```

Se algum check falhar, o `status` do item correspondente muda para `Unhealthy` e o campo `error` traz a mensagem da exceção.

### Métricas (Prometheus)

```http
GET /metrics
```

Endpoint no formato de exposição do Prometheus, gerado pelo OpenTelemetry (`AddPrometheusExporter`). Inclui métricas de ASP.NET Core, HttpClient e runtime — como tempo de resposta por rota e taxa de erros por status code.

### Tracing distribuído (OpenTelemetry)

A aplicação instrumenta automaticamente:

- Requisições HTTP recebidas (`AddAspNetCoreInstrumentation`)
- Chamadas HTTP de saída (`AddHttpClientInstrumentation`)
- Consultas ao banco via EF Core (`AddEntityFrameworkCoreInstrumentation`)

Em desenvolvimento, os traces são exportados para o console (`AddConsoleExporter`), permitindo acompanhar uma requisição atravessando as camadas API → Application/Infrastructure → Oracle.

### Logging estruturado (Serilog)

Logs estruturados com Serilog, com dois destinos configurados:

- **Console** — saída formatada em tempo real durante a execução
- **Arquivo** — `logs/petpulse-AAAAMMDD.log`, um arquivo por dia (`RollingInterval.Day`)

Níveis utilizados: `Information` (fluxo normal), `Warning` (situações não críticas) e `Error` (falhas). Cada linha de log de requisição (`UseSerilogRequestLogging`) é correlacionada com o `TraceId`/`SpanId` da requisição via `Serilog.Enrichers.Span`, permitindo cruzar um log específico com o trace distribuído correspondente.

Além do trace/span, toda requisição também recebe um **id de correlação de requisição** (`CorrelationIdMiddleware`): reaproveitado do header `X-Correlation-Id` quando o cliente já envia um, ou gerado como um novo Guid caso contrário. Esse id é devolvido no header `X-Correlation-Id` da resposta e injetado no Serilog `LogContext`, aparecendo em toda linha de log daquela requisição (inclusive no resumo gerado pelo `UseSerilogRequestLogging`) — útil para localizar rapidamente todos os logs de uma requisição específica, sem depender só do trace distribuído.

### Tratamento de exceções

Qualquer exceção não tratada que escape de um controller é capturada globalmente pelo `GlobalExceptionHandler` (`IExceptionHandler`, registrado via `AddExceptionHandler` + `UseExceptionHandler`, como primeiro middleware da pipeline). Em vez de devolver todo erro como um `500` genérico, o handler mapeia o **tipo** da exceção para o status HTTP correspondente:

| Tipo de exceção | Status | Título |
|---|---|---|
| `ArgumentNullException` / `ArgumentException` | 400 | Requisição inválida |
| `DomainException` (regra de negócio violada no Domain) | 400 | Não foi possível concluir a operação |
| `InvalidOperationException` | 400 | Não foi possível concluir a operação |
| `KeyNotFoundException` | 404 | Recurso não encontrado |
| `UnauthorizedAccessException` | 401 | Não autorizado |
| `OracleException` (falha de acesso ao banco) | 502 | Banco indisponível |
| Qualquer outra exceção não mapeada | 500 | Erro interno do servidor |

`DomainException` é uma exceção própria do `PetPulse.Domain` (`PetPulse.Domain.Exceptions.DomainException`), lançada pelas entidades ao violar uma regra de negócio (ex.: `Nome do pet não pode ser vazio.`, `Frequência cardíaca deve ser maior que zero.`). Como o handler global já mapeia esse tipo para `400`, nenhum controller precisa de `try/catch` para isso — uma chamada como `pet.AtualizarPeso(request.Peso)` que viole uma regra é automaticamente convertida em uma resposta `400` estruturada, em vez de um `500` cru.

> Essas respostas mapeadas só aparecem quando `ASPNETCORE_ENVIRONMENT` **não** é literalmente `Development` — veja a observação em [Variáveis de ambiente](#variáveis-de-ambiente) para o motivo.

Além de mapear o status, o handler:

- Registra a exceção no log estruturado (nível `Error`), com o `TraceId` da requisição;
- Devolve ao cliente uma resposta no formato `ProblemDetails` (RFC 7807), com `Content-Type: application/problem+json`;
- Expõe `traceId` (id de trace do ASP.NET Core) no corpo da resposta **apenas em ambiente de desenvolvimento**, para facilitar localizar a requisição nos logs durante o desenvolvimento;
- Expõe `correlationId` no corpo da resposta **sempre que presente** (inclusive em produção) — esse é o id de correlação de ponta a ponta gerado/propagado pelo `CorrelationIdMiddleware`, usado para localizar a requisição nos logs estruturados do Serilog;
- No caso de exceções não mapeadas, nunca expõe a mensagem real da exceção no corpo da resposta (nem em desenvolvimento), apenas uma mensagem genérica — o detalhe técnico fica só no log do servidor.

Exemplo de resposta para uma regra de domínio violada (`DomainException`, ambiente de desenvolvimento):

```json
{
  "type": "about:blank",
  "title": "Não foi possível concluir a operação",
  "status": 400,
  "detail": "Frequência cardíaca deve ser maior que zero.",
  "instance": "/api/dispositivoiot/3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "traceId": "0HN8K2...",
  "correlationId": "b3b3f6d2-4b1e-4e9a-9c3a-7a2f9e1d0c5a"
}
```

Exemplo de resposta para uma exceção não mapeada (produção — sem `traceId`, sem detalhe técnico):

```json
{
  "type": "about:blank",
  "title": "Erro interno do servidor",
  "status": 500,
  "detail": "Ocorreu um erro inesperado. Tente novamente mais tarde.",
  "instance": "/api/pets",
  "correlationId": "b3b3f6d2-4b1e-4e9a-9c3a-7a2f9e1d0c5a"
}
```

---

## Autenticação (JWT)

A API usa autenticação via **JWT Bearer token**, seguindo o mesmo padrão de emissão usado no repositório de referência da disciplina (`2TDSPG-2026/Recommenda`): senha armazenada como hash (`BCrypt`, nunca em texto puro) e um endpoint de login que emite um token assinado.

### Fluxo

1. Cadastre um usuário em `POST /api/Usuario` (único endpoint de escrita público — é como se cria a própria conta).
2. Autentique-se em `POST /api/auth/login`, enviando `email` e `senha`. A resposta traz o `accessToken` (JWT), a expiração (2h) e os dados básicos do usuário.
3. Para qualquer outro endpoint da API, envie o token no header `Authorization: Bearer {accessToken}`. Sem esse header, ou com um token inválido/expirado, a API responde `401 Unauthorized`.

No Swagger UI, clique no botão **Authorize** (cadeado no topo da página), cole o `accessToken` (sem o prefixo `Bearer`, o próprio Swagger adiciona) e todas as chamadas feitas pela interface passam a incluir o header automaticamente.

### Endpoint

| Método | Endpoint           | Descrição                                | Protegido? |
| ------ | ------------------ | ----------------------------------------- | ---------- |
| POST   | `/api/auth/login`  | Autentica um usuário e emite o token JWT  | Não        |

### Exemplo de requisição

```json
POST /api/auth/login
{
  "email": "ana.souza@email.com",
  "senha": "Senha123456"
}
```

### Exemplo de resposta (200)

```json
{
  "accessToken": "eyJhbGciOi...",
  "expiracao": "2026-10-17T02:30:00Z",
  "email": "ana.souza@email.com",
  "nome": "Ana Souza"
}
```

### Configuração

`Jwt:Issuer` e `Jwt:Audience` ficam no `appsettings.json`. A chave de assinatura (`Jwt:Key`) é um segredo e **não** fica versionada: localmente vem do User Secrets (`dotnet run`) ou do arquivo `.env` (Docker Compose); na nuvem chega como variável de ambiente secreta `Jwt__Key`, definida com cadeado na pipeline do Azure DevOps (ou gravada no Key Vault, segredo `jwt-key`, pelo `azure-cli.sh` no deploy via ACI).


---

## MongoDB — histórico de leituras IoT

Além do Oracle (dados relacionais), a API usa o **MongoDB** para guardar o **histórico completo das leituras da coleira IoT** — o mesmo JSON que o firmware do ESP32 ([PetPulse-Iot](https://github.com/Fiap-2tdspg-Challenge/PetPulse-Iot)) gera em `GET /api/dados`.

| Banco | O que guarda | Por quê |
|---|---|---|
| Oracle (`PP_DispositivoIots`) | Só a **última** leitura de cada dispositivo (estado atual) | Dado relacional, ligado a pet e usuário, consultado com frequência |
| MongoDB (coleção `leituras_iot`) | **Todas** as leituras recebidas (série temporal) | Alto volume, só inserções, esquema flexível — se o firmware passar a enviar um sensor novo, nada precisa ser migrado |

Ao registrar uma leitura (`POST /api/DispositivoIot/{dispositivoId}/leituras`), a API grava o documento no MongoDB **e** atualiza a última leitura do dispositivo no Oracle.

Detalhes da implementação (mesmo modelo do repositório de referência da disciplina):

- **Documento:** `PetPulse.Domain/Entites/Mongo/LeituraIotDocument.cs`, com `Id` (Guid como UUID padrão), `dispositivoId`, `petId`, sinais vitais, `estado`, `alerta`, `dataLeitura` e `registradoEm`. Os campos são gravados em camelCase.
- **Repositório:** `LeituraIotRepository` (Infrastructure) implementa `ILeituraIotRepository` (Application) com o `MongoDB.Driver`. As leituras são eventos imutáveis, então o repositório só inclui, consulta e exclui — não existe atualização.
- **Índice:** composto `{ dispositivoId: 1, dataLeitura: -1 }`, no mesmo formato da consulta de histórico, criado automaticamente.
- **Configuração:** `MongoServiceCollectionExtensions.AddPetPulseMongo` registra `IMongoClient` e `IMongoDatabase` como singleton. A connection string vem de `ConnectionStrings:PetPulseMongo` (User Secrets localmente, variável secreta na nuvem) e o nome do banco de `MongoDatabase:Name` (padrão `petpulse`).
- **Health check:** o `/health` inclui o check `MongoDB`.

Exemplo de documento na coleção `leituras_iot`:

```json
{
  "_id": "6f1c0a2e-1b4a-4c3d-9e10-0a1b2c3d4e5f",
  "dispositivoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "petId": "9b2e7c1a-4d3f-4e8b-a1c2-5d6e7f8a9b0c",
  "codigoDispositivo": "COLLAR-001",
  "frequenciaCardiaca": 95,
  "nivelAtividade": 45,
  "descAtividade": "caminhada",
  "aceleracaoTotal": 1.23,
  "pressaoSistolica": 130,
  "pressaoDiastolica": 80,
  "temperatura": 38.5,
  "score": 87,
  "estado": "OK",
  "alerta": "",
  "uptime": 12345,
  "dataLeitura": "2026-10-19T15:00:00Z",
  "registradoEm": "2026-10-19T15:00:01Z"
}
```

Para conferir os dados direto no MongoDB (Compass ou `mongosh`):

```javascript
use petpulse
db.leituras_iot.find({ dispositivoId: UUID("<id-do-dispositivo>") }).sort({ dataLeitura: -1 }).limit(10)
```

---

## Paginação, ordenação, filtros e HATEOAS

As cinco listagens principais (`GET /api/Usuario`, `/api/Pet`, `/api/HistoricoClinico`, `/api/DispositivoIot` e `/api/AlertaInteligente`) são **paginadas**, aceitam **ordenação** e **filtros** pela query string e retornam **links HATEOAS**. A paginação por offset segue o modelo do repositório de referência da disciplina (`PaginationQuery` / `PagedResponse`).

### Parâmetros comuns

| Parâmetro | Padrão | Regra |
|---|---|---|
| `page` | `1` | Número da página, a partir de 1 |
| `pageSize` | `20` | De 1 a 100 itens por página |
| `sortBy` | depende do recurso | Campo de ordenação (lista abaixo, sem diferenciar maiúsculas) |
| `sortDirection` | `asc` | `asc` ou `desc` |

Valores inválidos (ex.: `page=0`, `pageSize=500`, `sortBy=cpf`, `sortDirection=cima`, `dataInicio` depois de `dataFim`) retornam **400 Bad Request** com a mensagem do problema. Filtros de texto buscam por "contém" e ignoram maiúsculas; enums aceitam o nome (`Aberto`) ou o número (`1`). Em empates na ordenação, o desempate é feito pelo `Id`, para que nenhum item se repita ou suma entre as páginas.

### Ordenação e filtros por recurso

| Recurso | `sortBy` aceitos | Ordenação padrão | Filtros |
|---|---|---|---|
| `GET /api/Usuario` | `nome`, `email`, `dataCadastro` | `nome` crescente | `nome`, `email` |
| `GET /api/Pet` | `nome`, `especie`, `peso`, `dataNascimento`, `dataCadastro` | `nome` crescente | `usuarioId`, `nome`, `especie` (exata), `porte`, `sexo`, `castrado` |
| `GET /api/HistoricoClinico` | `dataRegistro`, `tipoRegistro`, `dataCadastro` | `dataRegistro` decrescente | `petId`, `tipoRegistro`, `dataInicio`, `dataFim` (formato `yyyy-MM-dd`) |
| `GET /api/DispositivoIot` | `dataUltimaLeitura`, `dataVinculacao`, `frequenciaCardiaca`, `dataCadastro` | `dataUltimaLeitura` decrescente | `petId`, `status` |
| `GET /api/AlertaInteligente` | `dataGeracao`, `nivelRisco`, `status`, `tipoAlerta` | `dataGeracao` decrescente | `petId`, `status`, `nivelRisco`, `tipoAlerta` |

### Exemplos

```http
GET /api/Pet?page=1&pageSize=2&sortBy=peso&sortDirection=desc&especie=Cachorro
GET /api/AlertaInteligente?status=Aberto&nivelRisco=Alto
GET /api/HistoricoClinico?petId={petId}&dataInicio=2026-01-01&dataFim=2026-06-30
GET /api/Usuario?nome=ana&sortBy=dataCadastro&sortDirection=desc
```

### Formato da resposta paginada

```json
{
  "items": [
    {
      "id": "8f1c2a4e-0d6b-4b8e-9b0a-1f2e3d4c5b6a",
      "usuarioId": "3a7d9e10-55c2-4f0b-8e61-2b9c4d7a1e3f",
      "nome": "Thor",
      "especie": "Cachorro",
      "peso": 30.0,
      "links": [
        { "rel": "self", "href": "/api/Pet/8f1c2a4e-0d6b-4b8e-9b0a-1f2e3d4c5b6a", "method": "GET" },
        { "rel": "update", "href": "/api/Pet/8f1c2a4e-0d6b-4b8e-9b0a-1f2e3d4c5b6a", "method": "PUT" },
        { "rel": "delete", "href": "/api/Pet/8f1c2a4e-0d6b-4b8e-9b0a-1f2e3d4c5b6a", "method": "DELETE" },
        { "rel": "usuario", "href": "/api/Usuario/3a7d9e10-55c2-4f0b-8e61-2b9c4d7a1e3f", "method": "GET" },
        { "rel": "historicos-clinicos", "href": "/api/HistoricoClinico/pet/8f1c2a4e-0d6b-4b8e-9b0a-1f2e3d4c5b6a", "method": "GET" },
        { "rel": "alertas", "href": "/api/AlertaInteligente/pet/8f1c2a4e-0d6b-4b8e-9b0a-1f2e3d4c5b6a", "method": "GET" },
        { "rel": "dispositivo-iot", "href": "/api/DispositivoIot/pet/8f1c2a4e-0d6b-4b8e-9b0a-1f2e3d4c5b6a", "method": "GET" }
      ]
    }
  ],
  "page": 1,
  "pageSize": 2,
  "totalItems": 3,
  "totalPages": 2,
  "hasPrevious": false,
  "hasNext": true,
  "links": [
    { "rel": "self", "href": "/api/Pet?page=1&pageSize=2&sortBy=peso&sortDirection=desc&especie=Cachorro", "method": "GET" },
    { "rel": "first", "href": "/api/Pet?page=1&pageSize=2&sortBy=peso&sortDirection=desc&especie=Cachorro", "method": "GET" },
    { "rel": "next", "href": "/api/Pet?page=2&pageSize=2&sortBy=peso&sortDirection=desc&especie=Cachorro", "method": "GET" },
    { "rel": "last", "href": "/api/Pet?page=2&pageSize=2&sortBy=peso&sortDirection=desc&especie=Cachorro", "method": "GET" },
    { "rel": "create", "href": "/api/Pet", "method": "POST" }
  ]
}
```

> O item acima está resumido (os demais campos do pet foram omitidos).

### Links HATEOAS

- **Na página:** `self`, `first`, `prev` (só se houver página anterior), `next` (só se houver próxima), `last` e `create`. Os links de navegação mantêm os mesmos filtros e a mesma ordenação da requisição.
- **Em cada recurso** (nas listagens e também em `GET /{id}`, `POST` e `PUT`): `self`, `update`, `delete` e os recursos relacionados:

| Recurso | Links relacionados |
|---|---|
| Usuário | `pets` |
| Pet | `usuario`, `historicos-clinicos`, `alertas`, `dispositivo-iot` |
| Histórico clínico | `pet` |
| Dispositivo IoT | `pet`, `leituras` (GET) e `registrar-leitura` (POST) do histórico no MongoDB |
| Alerta inteligente | `pet`, e os links de ação conforme o estado: `visualizar` só enquanto está **Aberto** e `resolver` enquanto ainda não foi **Resolvido** |

Os links são caminhos relativos à API (ex.: `/api/Pet/{id}`) e ficam em `PetPulse.API/Hateoas/HateoasLinks.cs`.

---

## Testes automatizados

A solução conta com testes automatizados em **xUnit**, organizados por camada e espelhando a própria Clean Architecture do projeto. Cada projeto de produção tem seu par de testes:

| Projeto de teste | Camada testada | O que cobre |
|---|---|---|
| `PetPulse.Domain.Tests` | Domínio | Regras de negócio das entidades `Usuario`, `Pet`, `HistoricoClinico`, `DispositivoIot` e `AlertaInteligente` — construção, validações de guarda (ex.: nome vazio, peso negativo, data futura) e os métodos `AtualizarDados` de cada entidade |
| `PetPulse.Application.Tests` | Application (DTOs) | Mapeamento `ToDomain()`/`FromDomain()` e validação (`DataAnnotations`) dos DTOs de request/response das 5 entidades, do login e das leituras IoT (MongoDB) |
| `PetPulse.API.Tests` | API / Controllers / Middlewares / Auth | Os 7 controllers (`UsuarioController`, `PetController`, `HistoricoClinicoController`, `DispositivoIotController`, `AlertaInteligenteController`, `AuthController`, `LeituraIotController`), com os repositórios/serviços isolados via **Moq**. Cobre cenários de sucesso (`200`/`201`/`204`), regras de negócio (ex.: e-mail/CPF duplicado, dispositivo já vinculado a um pet) e casos de erro (`404 Not Found`, `400 Bad Request`, `401 Unauthorized`). Também cobre o `CorrelationIdMiddleware`, o `GlobalExceptionHandler`, o `LoginService` e a emissão de token no `JwtTokenIssuer` |
| `PetPulse.Infrastructure.Tests` | Infraestrutura | As implementações EF Core dos 5 repositórios (`PetRepository`, `UsuarioRepository`, `HistoricoClinicoRepository`, `DispositivoIotRepository`, `AlertaInteligenteRepository`), usando `Microsoft.EntityFrameworkCore.InMemory`. Cobre persistência (`Add`/`Update`/`Delete`), buscas por relacionamento (ex.: pets de um usuário, histórico de um pet) e regras como a checagem case-insensitive de e-mail |

### Executando os testes

Rodar toda a suíte:

```powershell
dotnet test
```

Rodar um projeto de teste específico:

```powershell
dotnet test PetPulse.Domain.Tests\PetPulse.Domain.Tests.csproj
dotnet test PetPulse.Application.Tests\PetPulse.Application.Tests.csproj
dotnet test PetPulse.API.Tests\PetPulse.API.Tests.csproj
dotnet test PetPulse.Infrastructure.Tests\PetPulse.Infrastructure.Tests.csproj
```

> Convenção de nomenclatura dos testes: `MetodoTestado_Cenario_ResultadoEsperado` (ex.: `Create_ComEmailJaCadastrado_DeveRetornarBadRequestENaoPersistir`), seguindo o padrão AAA (Arrange/Act/Assert).

---
## Arquitetura de Deploy (ACR + ACI + Key Vault)
![alt text](<Screenshot 2026-09-05 152950.png>)



## Como executar

### Opção 1 — Localmente com .NET (requer Oracle externo)

Use esta opção se você já tem uma instância Oracle disponível (ex.: Oracle FIAP) e um MongoDB acessível (ex.: instalado localmente na porta padrão `27017`, ou um cluster do MongoDB Atlas).

**1. Clone o repositório**

```bash
git clone https://github.com/PietroWilhelm/PetPulse.git
cd PetPulse
```

**2. Configure os segredos locais (User Secrets)**

A connection string e a chave JWT não ficam no `appsettings.json`. Localmente elas ficam no **User Secrets** do .NET, um arquivo fora do repositório (`%APPDATA%\Microsoft\UserSecrets\` no Windows, `~/.microsoft/usersecrets/` no Linux/macOS):

```powershell
cd PetPulse.API
dotnet user-secrets set "ConnectionStrings:PetPulseOracle" "Data Source=oracle.fiap.com.br:1521/orcl;User ID=SEU_RM;Password=SUA_SENHA;"
dotnet user-secrets set "Jwt:Key" "UMA-CHAVE-ALEATORIA-COM-PELO-MENOS-32-CARACTERES"
dotnet user-secrets set "ConnectionStrings:PetPulseMongo" "mongodb://localhost:27017"
cd ..
```

Para conferir o que está configurado: `dotnet user-secrets list --project PetPulse.API`.

> Os User Secrets são carregados quando a API roda com `ASPNETCORE_ENVIRONMENT=Local`, que é o padrão do `launchSettings.json`.

**3. Execute a API**

```powershell
dotnet run --project PetPulse.API\PetPulse.API.csproj
```

> As migrations são aplicadas automaticamente na inicialização. Não é necessário rodar `dotnet ef database update`.

**4. Acesse o Swagger e o restante da aplicação**

```
http://localhost:5292/swagger
http://localhost:5292/metrics
http://localhost:5292/health
```

---

## Opção 2 — Localmente com Docker (Oracle incluído)

Use esta opção para rodar tudo localmente sem depender de um banco externo. O Docker sobe o Oracle XE e a API automaticamente.

**Pré-requisito:** Docker Desktop em execução.

**1. Clone o repositório**

```bash
git clone https://github.com/PietroWilhelm/PetPulse.git
cd PetPulse
```

**2. Crie o arquivo `.env` com as senhas locais**

As senhas do Oracle local e a chave JWT não ficam no `docker-compose.yml`: ele as lê de um arquivo `.env`, que está no `.gitignore` e nunca vai para o GitHub. Copie o modelo e preencha os valores:

```bash
cp .env.example .env
```

```text
ORACLE_PASSWORD=senha-do-usuario-sys-do-oracle-local
ORACLE_APP_PASSWORD=senha-do-usuario-petpulse
JWT_KEY=uma-chave-aleatoria-com-pelo-menos-32-caracteres
```

**3. Suba os containers**

```bash
docker compose up -d
```

Isso irá:
- Baixar a imagem `gvenzl/oracle-xe:21-slim` do Docker Hub
- Baixar a imagem `pietrowilhelm/challenge-clyvo-vet:latest` do Docker Hub
- Criar o volume `oracle_data` para persistência dos dados
- Inicializar o Oracle XE (aguarde ~2 minutos para o healthcheck passar)
- Subir o MongoDB 7 (`petpulse-mongo`) para o histórico de leituras IoT — exposto na porta `27018` do host, para não conflitar com um MongoDB instalado localmente
- Subir a API na porta `8080`

**4. Verifique se os containers estão saudáveis**

```bash
docker ps
```

Aguarde o `oracle-db` aparecer com status `(healthy)` antes de usar a API.

**5. Acesse o Swagger**

```
http://localhost:8080/swagger
```

**6. Parar os containers**

```bash
docker compose down
```

> Os dados do Oracle ficam salvos no volume `oracle_data`. Para remover os dados também:
> ```bash
> docker compose down -v
> ```
---

## Deploy em Nuvem (ACR + ACI + Key Vault)

Além das opções locais (Opção 1), Docker Compose (Opção 2) e VM no Azure (Opção 3), a API também pode ser publicada de forma **serverless** no Azure — 100% containerizada, tanto a API quanto o banco Oracle, sem misturar com nenhum serviço PaaS (banco gerenciado), conforme exigido pela documentação do Challenge. A arquitetura usa três serviços:

- **Azure Container Registry (ACR)** — guarda a imagem Docker da API.
- **Azure Container Instances (ACI)** — dois containers independentes, cada um com seu próprio FQDN público: um para a API e outro para o banco Oracle (`gvenzl/oracle-xe`, a mesma imagem usada localmente no `docker-compose.yml`).
- **Azure Key Vault** — guarda todas as senhas e credenciais (senha do Oracle, senha do usuário da aplicação, usuário/senha do ACR). Nenhuma credencial fica em texto puro no script, no container ou no repositório.

> O Oracle roda **sem persistência em disco** (sem Azure File Share/volume) — de propósito. O Oracle XE não tolera bem um shutdown "sujo" quando os dados estão em um volume persistido (gera `ORA-01081: cannot start already-running ORACLE`), então trocamos persistência por simplicidade e confiabilidade. Isso está dentro da regra do Challenge, que exige o banco **containerizado**, mas não exige persistência entre reinícios. Se o container do Oracle for recriado, basta rodar o `script_bd.sql` de novo (veja a seção abaixo).

### Fluxo

```text
Docker Hub (imagem já publicada) → docker pull/tag/push → Azure Container Registry (ACR)
                                                                     │
                                                                     ▼
Azure Key Vault (senhas/credenciais) ──────────► Azure Container Instance – API (petpulse-api)
                                       └────────► Azure Container Instance – Oracle (petpulse-oracle-db)
```

A imagem da API usada é a mesma publicada no Docker Hub (`pietrowilhelm/challenge-clyvo-vet:latest`, gerada a partir do `dockerfile` na raiz do projeto). O script `azure-cli.sh` automatiza todo o processo: cria o Resource Group, o ACR, o Key Vault, sobe os dois containers (Oracle e API) e conecta tudo.



### Clone o repositório

```bash
git clone https://github.com/PietroWilhelm/PetPulse.git
cd PetPulse
```

### Pré-requisitos

- Azure CLI instalado e autenticado (`az login`)
- Docker Desktop **aberto** (necessário para `docker pull`/`tag`/`push` no ACR)
- Duas senhas exportadas como variável de ambiente **antes** de rodar o script — nunca deixe senhas escritas no script ou no código-fonte; elas só existem no shell local e são gravadas diretamente no Key Vault:

```bash
export ORACLE_PASSWORD='senha_do_usuario_sys_do_oracle'
export ORACLE_APP_PASSWORD='senha_do_usuario_petpulse_no_oracle'
```

- No Git Bash (Windows), exporte também a variável abaixo antes de rodar o script, para evitar que o Git Bash reescreva argumentos como `/subscriptions/...` como se fossem caminhos do Windows:

```bash
export MSYS_NO_PATHCONV=1
```

### Executando o deploy

```bash
chmod +x azure-cli.sh
./azure-cli.sh
```

O script é idempotente — pode ser executado mais de uma vez sem recriar recursos já existentes — e faz, na ordem:

| Passo | O que faz |
|---|---|
| 1 | Reaproveita o Resource Group `rg-challenge-clyvo-vet` (cria se não existir) |
| 2 | Registra os providers `Microsoft.ContainerRegistry`, `Microsoft.ContainerInstance` e `Microsoft.KeyVault` na assinatura (necessário ao menos uma vez; em assinaturas Azure para Estudantes eles geralmente não vêm habilitados por padrão) |
| 3 | Cria o Azure Container Registry `petpulse` (SKU Basic) e recupera `loginServer` + credenciais |
| 4 | Faz login no ACR, puxa a imagem do Docker Hub, retagueia como `petpulse.azurecr.io/petpulse-api:v1` e sobe (push) |
| 5 | Cria o Key Vault `petpulse-kv`, concede à sua conta o papel **Key Vault Administrator** (com espera automática pela propagação do RBAC) e grava os segredos `oracle-password`, `oracle-app-password`, `acr-username`, `acr-password` |
| 6 | Recria do zero a ACI do Oracle (`petpulse-oracle-db`, sem volume), lendo a senha diretamente do Key Vault como `--secure-environment-variables` |
| 7 | Aguarda o Oracle sinalizar `DATABASE IS READY TO USE` nos logs (poll a cada 15s, até 15 minutos) em vez de um `sleep` fixo |
| 8 | Recria do zero a ACI da API (`petpulse-api`), lendo credenciais do ACR e a connection string do Oracle do Key Vault, também como `--secure-environment-variables` |

Todas as senhas usadas na criação dos containers (`ORACLE_PASSWORD`, `APP_USER_PASSWORD`, `ConnectionStrings__PetPulseOracle`, credenciais do ACR) são passadas como **secure environment variables** (`--secure-environment-variables`), que a Azure mantém criptografadas e não expõe em `az container show` — nunca em texto puro.

### Acessando a API na nuvem

Ao final da execução, o script imprime os FQDNs gerados (API e Oracle) e os endpoints de observabilidade:

```
http://<API_FQDN>:8080/swagger
http://<API_FQDN>:8080/health
http://<API_FQDN>:8080/metrics
```

### Aplicando o script_bd.sql no Oracle da nuvem

Como o container do Oracle não usa volume persistente, sempre que ele for (re)criado é preciso rodar o `script_bd.sql` de novo para criar as tabelas. Há duas formas de fazer isso; ambas usam apenas ferramentas já disponíveis (Azure CLI/Portal e o `sqlplus` embutido na própria imagem do Oracle).

#### Opção A — direto de dentro do container do Oracle (recomendada)

Não depende do Docker local nem de instalar nada extra — só do `az container exec` (ou do Console do Portal). Funciona porque a imagem `gvenzl/oracle-xe` já vem com o `sqlplus` instalado.

**1. Abra uma sessão dentro do container do Oracle:**

```bash
az container exec --resource-group rg-challenge-clyvo-vet --name petpulse-oracle-db --exec-command "/bin/bash"
```

Ou pelo Portal do Azure: recurso `petpulse-oracle-db` → aba **Containers** → sub-aba **Console** → escolha `/bin/bash` → Conectar. Fica ainda melhor para o vídeo de demonstração, já que mostra visualmente a execução dentro do Azure.

**2. Conecte no banco com o SQL\*Plus**:

```bash
sqlplus petpulse@localhost:1521/XEPDB1
```

Quando pedir `Enter password:`, digite a senha do usuário `petpulse` (a mesma gravada como `oracle-app-password` no Key Vault).

**3. Cole o conteúdo inteiro do `script_bd.sql`** no prompt `SQL>` (abra o arquivo no editor, copie tudo e cole). O SQL\*Plus executa cada comando terminado em `;` em sequência, criando as 5 tabelas e aplicando os `COMMENT ON`.

**4. Confirme que as tabelas foram criadas:**

```sql
SELECT table_name FROM user_tables WHERE table_name LIKE 'PP_%' ORDER BY table_name;
```

Deve listar as 5: `PP_AlertasInteligentes`, `PP_DispositivoIots`, `PP_HistoricoClinicos`, `PP_Pets`, `PP_Usuarios`.

**5. Saia:**

```sql
exit;
```

e depois `exit` de novo para sair do shell do container.

> Se colar o arquivo inteiro de uma vez engasgar no console do navegador (paste muito longo em consoles web pode derrubar caracteres), cole em blocos menores — por exemplo, uma `CREATE TABLE` de cada vez — e rode o `SELECT` do passo 4 no final para confirmar que as 5 tabelas foram criadas. Pelo terminal local (`az container exec` fora do Portal) isso raramente acontece.

### Solução de problemas comuns

**`(ConflictError) A vault with the same name already exists in deleted state`** ao rodar o passo 5 (criação do Key Vault): isso acontece porque o Azure Key Vault tem *soft-delete* — se o Resource Group já foi apagado e recriado antes, o `petpulse-kv` antigo continua existindo num estado "excluído temporariamente" por um período de retenção, e bloqueia a criação de outro com o mesmo nome. Como não há nada para recuperar (as senhas serão regravadas do zero mesmo), a solução é purgar o vault antigo para liberar o nome:

```bash
az keyvault purge --name petpulse-kv --location southafricanorth
```

Depois é só rodar `./azure-cli.sh` de novo — ele reaproveita o Resource Group e o ACR (que já existem) e segue direto para criar o Key Vault normalmente.

### Comandos úteis

```bash
# Ver logs dos containers
az container logs --resource-group rg-challenge-clyvo-vet --name petpulse-api
az container logs --resource-group rg-challenge-clyvo-vet --name petpulse-oracle-db

# Logs em tempo real
az container logs --resource-group rg-challenge-clyvo-vet --name petpulse-api --follow

# Executar um comando dentro do container
az container exec --resource-group rg-challenge-clyvo-vet --name petpulse-api --exec-command "/bin/bash"

# Ver os segredos gravados no Key Vault (sem exibir o valor)
az keyvault secret list --vault-name petpulse-kv --output table

# Parar de cobrar pelos containers sem perder a imagem no ACR nem os segredos no Key Vault
az container delete --resource-group rg-challenge-clyvo-vet --name petpulse-api --yes
az container delete --resource-group rg-challenge-clyvo-vet --name petpulse-oracle-db --yes

# Listar as imagens/tags disponíveis no ACR
az acr repository show-tags --name petpulse --repository petpulse-api
```

> Importante: as ACIs permanecem rodando (e gerando consumo do crédito da assinatura) até serem deletadas. Se não estiver testando ativamente, use os comandos `az container delete` acima para parar a cobrança — a imagem continua guardada no ACR e os segredos continuam no Key Vault, então rodar `./azure-cli.sh` de novo recria os dois containers do zero (lembrando que o Oracle volta vazio, sendo necessário rodar o `script_bd.sql` novamente).

### Deletando toda a infraestrutura da nuvem ao final

```bash
az group delete --name "rg-challenge-clyvo-vet" --yes --no-wait
```

---

## Portas e serviços

| Serviço | Porta | URL |
|---|---|---|
| PetPulse API | 8080 | `http://HOST:8080/api/...` |
| Swagger UI | 8080 | `http://HOST:8080/swagger` |
| Oracle XE | 1521 | `HOST:1521/XEPDB1` |

---

## Variáveis de ambiente

A API aceita as seguintes variáveis de ambiente. Os valores sensíveis (connection string e chave JWT) nunca ficam versionados no `appsettings.json`: localmente vêm do User Secrets (`dotnet run`) ou do arquivo `.env` (Docker Compose), e na nuvem de variáveis secretas da pipeline do Azure DevOps (ou do Key Vault):

| Variável | Valor padrão | Descrição |
|---|---|---|
| `ConnectionStrings__PetPulseOracle` | _(nenhum — obrigatória)_ | Connection string do Oracle. Local: User Secrets ou `.env`; nuvem: variável secreta da pipeline |
| `ASPNETCORE_ENVIRONMENT` | `Local` (localmente) / `Production` (nuvem) | Ambiente de execução — propositalmente nunca `Development`, veja observação abaixo |
| `ASPNETCORE_URLS` | `http://+:8080` | URL de escuta da API |
| `Jwt__Key` | _(nenhum — obrigatória)_ | Chave de assinatura dos tokens JWT (mín. 32 caracteres). Local: User Secrets ou `.env`; nuvem: variável secreta da pipeline (ou Key Vault, segredo `jwt-key`) |
| `Jwt__Issuer` | `PetPulse` | Emissor (`iss`) do token JWT |
| `Jwt__Audience` | `PetPulse.API` | Público-alvo (`aud`) do token JWT |
| `ConnectionStrings__PetPulseMongo` | _(nenhum — obrigatória)_ | Connection string do MongoDB (histórico de leituras IoT). Local: User Secrets (ex.: `mongodb://localhost:27017`) ou definida pelo `docker-compose.yml`; nuvem: variável secreta da pipeline |
| `MongoDatabase__Name` | `petpulse` | Nome do banco no MongoDB |

> **Por que não usar `ASPNETCORE_ENVIRONMENT=Development`?** A partir do .NET 8, o ASP.NET Core liga automaticamente a *Developer Exception Page* sempre que o ambiente é literalmente `Development` — e ela intercepta qualquer exceção não tratada **antes** do `GlobalExceptionHandler`, devolvendo um `500` cru (stack trace completo, headers da requisição) em vez da resposta `ProblemDetails` mapeada. Por isso o `launchSettings.json` (execução local com `dotnet run`) usa `ASPNETCORE_ENVIRONMENT=Local`, e a imagem Docker (`dockerfile`, usada pelo Docker Compose e pelo ACI) usa `Production` — um nome qualquer diferente de `Development` já é suficiente para que o `GlobalExceptionHandler` sempre responda, em qualquer forma de executar o projeto. O único efeito colateral é que o campo `traceId` (que só aparece quando `environment.IsDevelopment()` é verdadeiro) não é exibido nesse modo — o `correlationId` continua presente normalmente.


---

## Endpoints disponíveis

> Coluna **Protegido?**: `Sim` exige o header `Authorization: Bearer {accessToken}` (ver [Autenticação (JWT)](#autenticação-jwt)). Os únicos endpoints públicos são o cadastro de usuário e o login.
>
> As listagens `GET` de cada recurso aceitam `page`, `pageSize`, `sortBy`, `sortDirection` e filtros — ver [Paginação, ordenação, filtros e HATEOAS](#paginação-ordenação-filtros-e-hateoas).


### Autenticação

| Método | Endpoint          | Descrição                               | Protegido? |
| ------ | ----------------- | ---------------------------------------- | ---------- |
| POST   | `/api/auth/login` | Autentica um usuário e emite o token JWT | Não        |

### Usuário

| Método | Endpoint            | Descrição               | Protegido? |
| ------ | ------------------- | ----------------------- |------------|
| GET    | `/api/Usuario`      | Lista usuários (paginado, filtros e ordenação) | Não        |
| GET    | `/api/Usuario/{id}` | Busca usuário por ID    | Não        |
| POST   | `/api/Usuario`      | Cria um usuário         | Não        |
| PUT    | `/api/Usuario/{id}` | Atualiza um usuário     | Sim        |
| DELETE | `/api/Usuario/{id}` | Remove um usuário       | Sim        |

### Pet

| Método | Endpoint                       | Descrição                | Protegido? |
| ------ | ------------------------------ | ------------------------ | ---------- |
| GET    | `/api/Pet`                     | Lista pets (paginado, filtros e ordenação) | Sim        |
| GET    | `/api/Pet/{id}`                | Busca pet por ID         | Sim        |
| GET    | `/api/Pet/usuario/{usuarioId}` | Lista pets de um usuário | Sim        |
| POST   | `/api/Pet`                     | Cria um pet              | Sim        |
| PUT    | `/api/Pet/{id}`                | Atualiza um pet          | Sim        |
| DELETE | `/api/Pet/{id}`                | Remove um pet            | Sim        |

### Histórico Clínico

| Método | Endpoint                            | Descrição                  | Protegido? |
| ------ | ----------------------------------- | -------------------------- | ---------- |
| GET    | `/api/HistoricoClinico`             | Lista históricos (paginado, filtros e ordenação) | Sim        |
| GET    | `/api/HistoricoClinico/{id}`        | Busca histórico por ID     | Sim        |
| GET    | `/api/HistoricoClinico/pet/{petId}` | Lista históricos de um pet | Sim        |
| POST   | `/api/HistoricoClinico`             | Cria histórico clínico     | Sim        |
| PUT    | `/api/HistoricoClinico/{id}`        | Atualiza histórico clínico | Sim        |
| DELETE | `/api/HistoricoClinico/{id}`        | Remove histórico clínico   | Sim        |

### Dispositivo IoT

| Método | Endpoint                          | Descrição                    | Protegido? |
| ------ | --------------------------------- | ----------------------------- | ---------- |
| GET    | `/api/DispositivoIot`             | Lista dispositivos (paginado, filtros e ordenação) | Sim        |
| GET    | `/api/DispositivoIot/{id}`        | Busca dispositivo por ID     | Sim        |
| GET    | `/api/DispositivoIot/pet/{petId}` | Busca dispositivo de um pet  | Sim        |
| POST   | `/api/DispositivoIot`             | Cria dispositivo IoT         | Sim        |
| PUT    | `/api/DispositivoIot/{id}`        | Atualiza dispositivo IoT     | Sim        |
| DELETE | `/api/DispositivoIot/{id}`        | Remove dispositivo IoT       | Sim        |

### Leituras IoT (MongoDB)

| Método | Endpoint                                                 | Descrição                                      | Protegido? |
| ------ | -------------------------------------------------------- | ---------------------------------------------- | ---------- |
| GET    | `/api/DispositivoIot/{dispositivoId}/leituras?limite=50` | Histórico de leituras (mais recentes primeiro) | Sim        |
| GET    | `/api/DispositivoIot/{dispositivoId}/leituras/{id}`      | Busca uma leitura                              | Sim        |
| POST   | `/api/DispositivoIot/{dispositivoId}/leituras`           | Registra leitura (MongoDB + última no Oracle)  | Sim        |
| DELETE | `/api/DispositivoIot/{dispositivoId}/leituras/{id}`      | Remove uma leitura do histórico                | Sim        |

### Alerta Inteligente

| Método | Endpoint                                 | Descrição                      | Protegido? |
| ------ | ----------------------------------------- | ------------------------------- | ---------- |
| GET    | `/api/AlertaInteligente`                 | Lista alertas (paginado, filtros e ordenação) | Sim        |
| GET    | `/api/AlertaInteligente/{id}`            | Busca alerta por ID            | Sim        |
| GET    | `/api/AlertaInteligente/pet/{petId}`     | Lista alertas de um pet        | Sim        |
| GET    | `/api/AlertaInteligente/status/{status}` | Lista alertas por status       | Sim        |
| POST   | `/api/AlertaInteligente`                 | Cria alerta inteligente        | Sim        |
| PUT    | `/api/AlertaInteligente/{id}`            | Atualiza alerta inteligente    | Sim        |
| PUT    | `/api/AlertaInteligente/{id}/visualizar` | Marca alerta como visualizado  | Sim        |
| PUT    | `/api/AlertaInteligente/{id}/resolver`   | Marca alerta como resolvido    | Sim        |
| DELETE | `/api/AlertaInteligente/{id}`            | Remove alerta inteligente      | Sim        |
---

## Ordem recomendada para testar

Como existem relacionamentos entre as entidades, recomenda-se testar na seguinte ordem:

```text
1. Criar usuário (POST /api/Usuario — único endpoint de escrita público)
2. Login (POST /api/auth/login) para obter o accessToken
3. Usar o accessToken (Authorization: Bearer {accessToken}) em todas as chamadas a seguir
4. Criar pet usando o ID do usuário
5. Criar histórico clínico usando o ID do pet
6. Criar dispositivo IoT usando o ID do pet
7. Registrar leituras da coleira (histórico no MongoDB) usando o ID do dispositivo
8. Criar alerta inteligente usando o ID do pet
```

> Importante: ao testar pelo Swagger, sempre copie os IDs retornados pela própria API. No Oracle, os IDs do tipo GUID podem aparecer como `RAW(16)`, em formato diferente do usado no JSON. Para o passo 3, clique em **Authorize** no topo do Swagger UI e cole o `accessToken` do passo 2.

---

## JSONs para teste

## 1. Criar usuário

### Endpoint

```http
POST /api/Usuario
```

### Body

```json
{
  "nome": "Ana Souza",
  "cpf": "12345678901",
  "email": "ana.souza@email.com",
  "senha": "Senha123456",
  "telefone": "11999990001",
  "endereco": "Rua das Flores, 100"
}
```

### Resultado esperado

```text
201 Created
```

Copie o campo `id` retornado. Ele será usado como `usuarioId` nos testes de Pet.

---

## Login (obter o token JWT)

### Endpoint

```http
POST /api/auth/login
```

### Body

```json
{
  "email": "ana.souza@email.com",
  "senha": "Senha123456"
}
```

### Resultado esperado

```text
200 OK
```

```json
{
  "accessToken": "eyJhbGciOi...",
  "expiracao": "2026-10-17T02:30:00Z",
  "email": "ana.souza@email.com",
  "nome": "Ana Souza"
}
```

Copie o `accessToken` e informe-o no botão **Authorize** do Swagger UI (ou no header `Authorization: Bearer {accessToken}`, se estiver testando com `curl`/Postman). Ele é exigido em todos os endpoints a partir daqui.

---

## 2. Criar pet

### Endpoint

```http
POST /api/Pet
```

### Body

Substitua `USUARIO_ID` pelo ID retornado pela API.

```json
{
  "usuarioId": "USUARIO_ID",
  "nome": "Thor",
  "especie": "Cachorro",
  "raca": "Golden Retriever",
  "dataNascimento": "2021-04-10",
  "peso": 28.5,
  "sexo": 1,
  "castrado": true,
  "porte": 3
}
```

### Enums

```text
sexo:
1 = Macho
2 = Femea
3 = NaoInformado

porte:
1 = Pequeno
2 = Medio
3 = Grande
4 = NaoInformado
```

### Resultado esperado

```text
201 Created
```

Copie o campo `id` retornado. Ele será usado como `petId` nos próximos testes.

---

## 3. Criar histórico clínico

### Endpoint

```http
POST /api/HistoricoClinico
```

### Body

Substitua `PET_ID` pelo ID retornado pela API.

```json
{
  "petId": "PET_ID",
  "tipoRegistro": 1,
  "descricao": "Vacina V10 aplicada",
  "dataRegistro": "2026-05-05",
  "dataRetorno": "2027-05-05",
  "profissionalClinica": "Clínica Pet Vida",
  "observacoes": "Pet sem reação adversa."
}
```

### Enums

```text
tipoRegistro:
1 = Vacina
2 = Consulta
3 = Doenca
4 = Medicamento
5 = Observacao
6 = Exame
```

### Resultado esperado

```text
201 Created
```

---

## 4. Criar dispositivo IoT

### Endpoint

```http
POST /api/DispositivoIot
```

### Body

Substitua `PET_ID` pelo ID retornado pela API.

```json
{
  "petId": "PET_ID",
  "dataVinculacao": "2026-05-05",
  "intervaloColetaMinutos": 30,
  "frequenciaCardiaca": 95,
  "nivelAtividade": 72.5,
  "pressao": 12.8,
  "dataUltimaLeitura": "2026-05-05T17:30:00",
  "status": 1
}
```

### Enums

```text
status:
1 = Ativo
2 = Inativo
3 = Manutencao
```

### Resultado esperado

```text
201 Created
```

---

## 4.1 Registrar leitura da coleira (MongoDB)

### Endpoint

```http
POST /api/DispositivoIot/{dispositivoId}/leituras
```

Use o `id` retornado na criação do dispositivo IoT. O dispositivo precisa estar com `status` = 1 (Ativo).

### Body

Mesmo JSON que a coleira (ESP32) gera em `GET /api/dados`. Só `frequenciaCardiaca` é obrigatória; campos extras, como `pet`, são ignorados:

```json
{
  "idDispositivo": "COLLAR-001",
  "frequenciaCardiaca": 95,
  "nivelAtividade": 45,
  "descAtividade": "caminhada",
  "aceleracaoTotal": 1.23,
  "pressaoSistolica": 130,
  "pressaoDiastolica": 80,
  "temperatura": 38.5,
  "score": 87,
  "estado": "OK",
  "alerta": "",
  "uptime": 12345
}
```

### Resultado esperado

```text
201 Created
```

A leitura aparece em `GET /api/DispositivoIot/{dispositivoId}/leituras`, e o `GET /api/DispositivoIot/{dispositivoId}` passa a mostrar essa frequência cardíaca como a última leitura (Oracle).

---

## 5. Criar alerta inteligente

### Endpoint

```http
POST /api/AlertaInteligente
```

### Body

Substitua `PET_ID` pelo ID retornado pela API.

```json
{
  "petId": "PET_ID",
  "tipoAlerta": 3,
  "nivelRisco": 2,
  "origemAlerta": 2,
  "mensagem": "O nível de atividade do pet está abaixo do padrão esperado.",
  "recomendacao": "Observar o comportamento nas próximas 24 horas e procurar uma clínica se persistir."
}
```

### Enums

```text
tipoAlerta:
1 = Atividade
2 = Vacina
3 = Medicamento
4 = CheckUp
5 = FrequenciaCardiaca
6 = Pressao

nivelRisco:
1 = Baixo
2 = Medio
3 = Alto

origemAlerta:
1 = HistoricoClinico
2 = DispositivoIot
3 = Sistema
4 = Usuario
```

### Resultado esperado

```text
201 Created
```

---

## Testes de consulta

## Listar usuários

```http
GET /api/Usuario
```

Resultado esperado:

```text
200 OK
```

A resposta vem paginada (`items`, `page`, `pageSize`, `totalItems`, `totalPages`, `hasPrevious`, `hasNext` e `links`).

## Listar pets com paginação, filtro e ordenação

```http
GET /api/Pet?page=1&pageSize=2&sortBy=peso&sortDirection=desc&especie=Cachorro
```

Resultado esperado:

```text
200 OK — até 2 cachorros, do mais pesado para o mais leve, com os links next/last apontando para a página seguinte
```

## Listar com parâmetro inválido

```http
GET /api/Pet?pageSize=500
```

Resultado esperado:

```text
400 Bad Request — "pageSize deve estar entre 1 e 100."
```

---

## Buscar pets de um usuário

```http
GET /api/Pet/usuario/{usuarioId}
```

Resultado esperado:

```text
200 OK
```

---

## Buscar históricos de um pet

```http
GET /api/HistoricoClinico/pet/{petId}
```

Resultado esperado:

```text
200 OK
```

---

## Buscar dispositivo de um pet

```http
GET /api/DispositivoIot/pet/{petId}
```

Resultado esperado:

```text
200 OK
```

---

## Buscar alertas de um pet

```http
GET /api/AlertaInteligente/pet/{petId}
```

Resultado esperado:

```text
200 OK
```

---

## Buscar alertas por status

```http
GET /api/AlertaInteligente/status/1
```

Status:

```text
1 = Aberto
2 = Visualizado
3 = Resolvido
```

---

## Testes de atualização

## Atualizar usuário

### Endpoint

```http
PUT /api/Usuario/{id}
```

### Body

```json
{
  "nome": "Ana Souza Atualizada",
  "cpf": "12345678901",
  "email": "ana.souza.atualizada@email.com",
  "senha": "Senha123456",
  "telefone": "11999990002",
  "endereco": "Rua das Flores, 200"
}
```

Resultado esperado:

```text
200 OK
```

---

## Atualizar pet

### Endpoint

```http
PUT /api/Pet/{id}
```

### Body

```json
{
  "usuarioId": "USUARIO_ID",
  "nome": "Thor Atualizado",
  "especie": "Cachorro",
  "raca": "Golden Retriever",
  "dataNascimento": "2021-04-10",
  "peso": 29.2,
  "sexo": 1,
  "castrado": true,
  "porte": 3
}
```

Resultado esperado:

```text
200 OK
```

---

## Atualizar alerta inteligente

### Endpoint

```http
PUT /api/AlertaInteligente/{id}
```

### Body

```json
{
  "petId": "PET_ID",
  "tipoAlerta": 3,
  "nivelRisco": 3,
  "origemAlerta": 2,
  "mensagem": "Atividade muito abaixo do padrão esperado.",
  "recomendacao": "Recomenda-se avaliação clínica preventiva."
}
```

Resultado esperado:

```text
200 OK
```

---

## Marcar alerta como visualizado

```http
PUT /api/AlertaInteligente/{id}/visualizar
```

Resultado esperado:

```text
200 OK
```

O campo `status` deve retornar como `2`.

---

## Marcar alerta como resolvido

```http
PUT /api/AlertaInteligente/{id}/resolver
```

Resultado esperado:

```text
200 OK
```

O campo `status` deve retornar como `3`.

---

## Testes de erro

## Buscar usuário inexistente

```http
GET /api/Usuario/00000000-0000-0000-0000-000000000000
```

Resultado esperado:

```text
404 Not Found
```

---

## Criar pet com usuário inexistente

```http
POST /api/Pet
```

```json
{
  "usuarioId": "00000000-0000-0000-0000-000000000000",
  "nome": "Pet Teste",
  "especie": "Cachorro",
  "raca": "Vira-lata",
  "dataNascimento": "2020-01-01",
  "peso": 10.5,
  "sexo": 1,
  "castrado": false,
  "porte": 2
}
```

Resultado esperado:

```text
404 Not Found
```

---

## Criar usuário com e-mail repetido

Repita o `POST /api/Usuario` usando o mesmo e-mail já cadastrado.

Resultado esperado:

```text
400 Bad Request
```

---

## Chamar um endpoint protegido sem token

```http
GET /api/Pet
```

(sem o header `Authorization`)

Resultado esperado:

```text
401 Unauthorized
```

---

## Login com credenciais incorretas

```http
POST /api/auth/login
```

```json
{
  "email": "ana.souza@email.com",
  "senha": "senha-errada"
}
```

Resultado esperado:

```text
401 Unauthorized
```

---

## Consultas para conferir no Oracle

Após os testes, é possível conferir os dados diretamente no Oracle:

```sql
SELECT * FROM "PP_Usuarios";
SELECT * FROM "PP_Pets";
SELECT * FROM "PP_HistoricoClinicos";
SELECT * FROM "PP_DispositivoIots";
SELECT * FROM "PP_AlertasInteligentes";
```

Também é possível listar as tabelas criadas:

```sql
SELECT table_name
FROM user_tables
WHERE table_name LIKE 'PP_%'
ORDER BY table_name;
```

---

## Códigos HTTP utilizados

| Código | Significado           | Uso na API                                    |
| ------ | ---------------------- | --------------------------------------------- |
| 200    | OK                    | Consulta, atualização ou login realizados com sucesso |
| 201    | Created               | Registro criado com sucesso                   |
| 204    | No Content            | Registro removido com sucesso                 |
| 400    | Bad Request           | Dados inválidos ou regra de validação violada |
| 401    | Unauthorized          | Token JWT ausente/inválido/expirado, ou login com credenciais incorretas |
| 404    | Not Found             | Registro não encontrado                       |
| 502    | Bad Gateway           | Banco de dados (Oracle) indisponível          |
| 500    | Internal Server Error | Erro inesperado na aplicação                  |

---

## Observações importantes

## GUID e Oracle RAW(16)

Os IDs das entidades são do tipo `Guid` no C# e são armazenados no Oracle como `RAW(16)`. Por isso, o valor exibido diretamente no banco pode aparecer em formato hexadecimal diferente do formato textual usado pela API.

Para testar endpoints que dependem de ID, utilize sempre o `id` retornado pelos endpoints `GET` ou `POST` da própria API, e não o valor copiado diretamente do banco Oracle.

## Enums

A API utiliza enums para padronizar campos com valores fixos, como sexo do pet, porte, tipo de registro clínico, status do dispositivo, tipo do alerta, nível de risco e status do alerta.

Isso evita valores inválidos e facilita o uso da API pelo Swagger.

---

## Conclusão

A API PetPulse fornece uma base funcional para o sistema de saúde preditiva pet, permitindo o cadastro de tutores, pets, histórico clínico, dispositivos IoT e alertas inteligentes. A solução utiliza ASP.NET Core, Entity Framework Core, Oracle Database, Swagger, Serilog e OpenTelemetry (Health Checks, logging estruturado, tracing e métricas), atendendo ao escopo inicial do Challenge e permitindo evolução futura para regras mais avançadas de IA, análise preditiva e integração com dispositivos reais. A cobertura de testes automatizados (xUnit) nas camadas de Domínio, API e Infraestrutura reforça a confiabilidade das regras de negócio e da persistência de dados. Como etapa de DevOps, a imagem também pode ser publicada de forma serverless na nuvem via Azure Container Registry e Azure Container Instances, sem a necessidade de provisionar ou administrar máquinas virtuais.