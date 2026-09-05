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
- [Testes automatizados](#testes-automatizados)
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

A connection string deve ser configurada no arquivo `appsettings.json` ou `appsettings.Development.json`.

Exemplo:

```json
{
  "ConnectionStrings": {
    "PetPulseOracle": "Data Source=oracle.fiap.com.br:1521/orcl;User ID=SEU_USUARIO;Password=SUA_SENHA;"
  },
  "Swagger": {
    "Title": "PetPulse API",
    "Version": "v1",
    "Description": "API para acompanhamento preventivo da saúde de pets.",
    "OpenApiDocumentName": "v1",
    "SwaggerUiDocumentTitle": "PetPulse API v1"
  }
}
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

Verifica a saúde da API, a conectividade com o banco Oracle e a disponibilidade de um serviço externo. Utiliza `Microsoft.Extensions.Diagnostics.HealthChecks`.

Checks configurados:

| Check | O que verifica |
|---|---|
| `PetPulse API` | Self-check simples, confirma que a API está no ar |
| `Oracle` | Conectividade com o banco via `PetPulseContext` (EF Core) |
| `FIAP` | Disponibilidade de um serviço HTTP externo |

Exemplo de resposta:

```json
{
  "status": "Healthy",
  "duration": "00:00:00.1234567",
  "checks": [
    { "name": "PetPulse API", "status": "Healthy", "description": "API está no ar", "duration": "00:00:00.0000010", "error": null },
    { "name": "Oracle", "status": "Healthy", "description": null, "duration": "00:00:00.0987654", "error": null },
    { "name": "FIAP", "status": "Healthy", "description": null, "duration": "00:00:00.0456789", "error": null }
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

---

## Testes automatizados

A solução conta com testes automatizados em **xUnit**, organizados por camada e espelhando a própria Clean Architecture do projeto. Cada projeto de produção tem seu par de testes:

| Projeto de teste | Camada testada | O que cobre |
|---|---|---|
| `PetPulse.Domain.Tests` | Domínio | Regras de negócio das entidades `Usuario`, `Pet`, `HistoricoClinico`, `DispositivoIot` e `AlertaInteligente` — construção, validações de guarda (ex.: nome vazio, peso negativo, data futura) e os métodos `AtualizarDados` de cada entidade |
| `PetPulse.API.Tests` | API / Controllers | Os 5 controllers (`UsuarioController`, `PetController`, `HistoricoClinicoController`, `DispositivoIotController`, `AlertaInteligenteController`), com os repositórios isolados via **Moq**. Cobre cenários de sucesso (`200`/`201`/`204`), regras de negócio (ex.: e-mail/CPF duplicado, dispositivo já vinculado a um pet) e casos de erro (`404 Not Found`, `400 Bad Request`) |
| `PetPulse.Infrastructure.Tests` | Infraestrutura | As implementações EF Core dos 5 repositórios (`PetRepository`, `UsuarioRepository`, `HistoricoClinicoRepository`, `DispositivoIotRepository`, `AlertaInteligenteRepository`), usando `Microsoft.EntityFrameworkCore.InMemory`. Cobre persistência (`Add`/`Update`/`Delete`), buscas por relacionamento (ex.: pets de um usuário, histórico de um pet) e regras como a checagem case-insensitive de e-mail |

### Executando os testes

Rodar toda a suíte:

```powershell
dotnet test
```

Rodar um projeto de teste específico:

```powershell
dotnet test PetPulse.Domain.Tests\PetPulse.Domain.Tests.csproj
dotnet test PetPulse.API.Tests\PetPulse.API.Tests.csproj
dotnet test PetPulse.Infrastructure.Tests\PetPulse.Infrastructure.Tests.csproj
```

> Convenção de nomenclatura dos testes: `MetodoTestado_Cenario_ResultadoEsperado` (ex.: `Create_ComEmailJaCadastrado_DeveRetornarBadRequestENaoPersistir`), seguindo o padrão AAA (Arrange/Act/Assert).

---

## Como executar

### Opção 1 — Localmente com .NET (requer Oracle externo)

Use esta opção se você já tem uma instância Oracle disponível (ex.: Oracle FIAP).

**1. Clone o repositório**

```bash
git clone https://github.com/PietroWilhelm/PetPulse.git
cd PetPulse
```

**2. Configure a connection string**

Edite `PetPulse.API/appsettings.Development.json` com os dados do seu banco:

```json
{
  "ConnectionStrings": {
    "PetPulseOracle": "Data Source=oracle.fiap.com.br:1521/orcl;User ID=SEU_USUARIO;Password=SUA_SENHA;"
  }
}
```

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

**2. Suba os containers**

```bash
docker compose up -d
```

Isso irá:
- Baixar a imagem `gvenzl/oracle-xe:21-slim` do Docker Hub
- Baixar a imagem `pietrowilhelm/challenge-clyvo-vet:latest` do Docker Hub
- Criar o volume `oracle_data` para persistência dos dados
- Inicializar o Oracle XE (aguarde ~2 minutos para o healthcheck passar)
- Subir a API na porta `8080`

**3. Verifique se os containers estão saudáveis**

```bash
docker ps
```

Aguarde o `oracle-db` aparecer com status `(healthy)` antes de usar a API.

**4. Acesse o Swagger**

```
http://localhost:8080/swagger
```

**5. Parar os containers**

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

A API aceita as seguintes variáveis de ambiente, configuráveis no `docker-compose.yml` ou no `appsettings.json`:

| Variável | Valor padrão | Descrição |
|---|---|---|
| `ConnectionStrings__PetPulseOracle` | _(vazio)_ | Connection string do Oracle |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Ambiente de execução |
| `ASPNETCORE_URLS` | `http://+:8080` | URL de escuta da API |


---

## Endpoints disponíveis

### Usuário

| Método | Endpoint            | Descrição               |
| ------ | ------------------- | ----------------------- |
| GET    | `/api/Usuario`      | Lista todos os usuários |
| GET    | `/api/Usuario/{id}` | Busca usuário por ID    |
| POST   | `/api/Usuario`      | Cria um usuário         |
| PUT    | `/api/Usuario/{id}` | Atualiza um usuário     |
| DELETE | `/api/Usuario/{id}` | Remove um usuário       |

### Pet

| Método | Endpoint                       | Descrição                |
| ------ | ------------------------------ | ------------------------ |
| GET    | `/api/Pet`                     | Lista todos os pets      |
| GET    | `/api/Pet/{id}`                | Busca pet por ID         |
| GET    | `/api/Pet/usuario/{usuarioId}` | Lista pets de um usuário |
| POST   | `/api/Pet`                     | Cria um pet              |
| PUT    | `/api/Pet/{id}`                | Atualiza um pet          |
| DELETE | `/api/Pet/{id}`                | Remove um pet            |

### Histórico Clínico

| Método | Endpoint                            | Descrição                  |
| ------ | ----------------------------------- | -------------------------- |
| GET    | `/api/HistoricoClinico`             | Lista todos os históricos  |
| GET    | `/api/HistoricoClinico/{id}`        | Busca histórico por ID     |
| GET    | `/api/HistoricoClinico/pet/{petId}` | Lista históricos de um pet |
| POST   | `/api/HistoricoClinico`             | Cria histórico clínico     |
| PUT    | `/api/HistoricoClinico/{id}`        | Atualiza histórico clínico |
| DELETE | `/api/HistoricoClinico/{id}`        | Remove histórico clínico   |

### Dispositivo IoT

| Método | Endpoint                          | Descrição                   |
| ------ | --------------------------------- | ---------------------------- |
| GET    | `/api/DispositivoIot`             | Lista todos os dispositivos |
| GET    | `/api/DispositivoIot/{id}`        | Busca dispositivo por ID    |
| GET    | `/api/DispositivoIot/pet/{petId}` | Busca dispositivo de um pet |
| POST   | `/api/DispositivoIot`             | Cria dispositivo IoT        |
| PUT    | `/api/DispositivoIot/{id}`        | Atualiza dispositivo IoT    |
| DELETE | `/api/DispositivoIot/{id}`        | Remove dispositivo IoT      |

### Alerta Inteligente

| Método | Endpoint                                 | Descrição                     |
| ------ | ----------------------------------------- | ------------------------------ |
| GET    | `/api/AlertaInteligente`                 | Lista todos os alertas        |
| GET    | `/api/AlertaInteligente/{id}`            | Busca alerta por ID           |
| GET    | `/api/AlertaInteligente/pet/{petId}`     | Lista alertas de um pet       |
| GET    | `/api/AlertaInteligente/status/{status}` | Lista alertas por status      |
| POST   | `/api/AlertaInteligente`                 | Cria alerta inteligente       |
| PUT    | `/api/AlertaInteligente/{id}`            | Atualiza alerta inteligente   |
| PUT    | `/api/AlertaInteligente/{id}/visualizar` | Marca alerta como visualizado |
| PUT    | `/api/AlertaInteligente/{id}/resolver`   | Marca alerta como resolvido   |
| DELETE | `/api/AlertaInteligente/{id}`            | Remove alerta inteligente     |
---

## Ordem recomendada para testar

Como existem relacionamentos entre as entidades, recomenda-se testar na seguinte ordem:

```text
1. Criar usuário
2. Criar pet usando o ID do usuário
3. Criar histórico clínico usando o ID do pet
4. Criar dispositivo IoT usando o ID do pet
5. Criar alerta inteligente usando o ID do pet
```

> Importante: ao testar pelo Swagger, sempre copie os IDs retornados pela própria API. No Oracle, os IDs do tipo GUID podem aparecer como `RAW(16)`, em formato diferente do usado no JSON.

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
| 200    | OK                    | Consulta ou atualização realizada com sucesso |
| 201    | Created               | Registro criado com sucesso                   |
| 204    | No Content            | Registro removido com sucesso                 |
| 400    | Bad Request           | Dados inválidos ou regra de validação violada |
| 404    | Not Found             | Registro não encontrado                       |
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