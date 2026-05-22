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
- [Como executar](#como-executar)
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
* Rider / Visual Studio
* Docker / Docker Compose
* Azure CLI / Microsoft Azure

---

## Arquitetura do projeto

A solução foi organizada em quatro projetos principais:

```text
PetPulse
├── PetPulse.API
├── PetPulse.Application
├── PetPulse.Domain
└── PetPulse.Infrastructure
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

---

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

**4. Acesse o Swagger**

```
http://localhost:5292/swagger
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

## Opção 3 — Deploy na nuvem com Azure VM

Use esta opção para provisionar toda a infraestrutura automaticamente no Microsoft Azure.

**Pré-requisitos:**
- Conta Azure ativa com permissão para criar recursos
- Azure CLI instalado e autenticado

**1. Autentique no Azure**

```bash
az login
```

**2. Clone o repositório**

```bash
git clone https://github.com/PietroWilhelm/PetPulse.git
cd PetPulse
```

**3. Revise as variáveis do script (opcional)**

Abra `azure-cli.sh` e ajuste conforme necessário:

```bash
RESOURCE_GROUP="rg-challenge-clyvo-vet"   # Nome do Resource Group
LOCATION="southafricanorth"               # Região Azure
VM_NAME="vm-petpulse"                     # Nome da VM
VM_SIZE="Standard_B4ls_v2"               # Tamanho da VM (4 vCPU, 8 GB RAM)
ADMIN_USER="petpulseadmin"               # Usuário SSH da VM
ADMIN_PASSWORD="Fiap@20262026"           # Senha SSH da VM
DOCKERHUB_USER="pietrowilhelm"           # Usuário do Docker Hub
IMAGE_TAG="latest"                        # Tag da imagem da API
```

**4. Execute o script de provisionamento**

```bash
chmod +x azure-cli.sh
./azure-cli.sh
```

O script executa automaticamente os seguintes passos:

| Passo | O que faz |
|---|---|
| 1/6 | Cria o Resource Group na região configurada |
| 2/6 | Provisiona a VM Ubuntu 22.04 com IP público |
| 3/6 | Abre as portas 22 (SSH), 8080 (API) e 1521 (Oracle) |
| 4/6 | Instala Docker, Git e Nano na VM |
| 5/6 | Cria o `docker-compose.yml` na VM e sobe os containers |
| 6/6 | Exibe o IP público, URL do Swagger e dados de SSH |

> O script leva entre 10 e 15 minutos para concluir.

**5. Aguarde os containers iniciarem**

Após o script terminar, aguarde aproximadamente **4 minutos** para o Oracle XE inicializar completamente.

**6. Acesse a API pelo IP público**

```
http://IP_PUBLICO:8080/swagger
```

O IP público é exibido ao final da execução do script.

**7. Verificar containers na VM via SSH**

```bash
ssh petpulseadmin@IP_PUBLICO
# Senha: Fiap@20262026

docker ps                          # Listar containers em execução
docker exec petpulse-api whoami    # Confirmar usuário não-root (appuser)
docker volume ls                   # Confirmar volume nomeado (oracle_data)
exit
```

**8. Deletar a infraestrutura ao final**

> ⚠️ Importante: delete os recursos ao terminar para evitar cobranças.

```bash
az group delete --name "rg-challenge-clyvo-vet" --yes --no-wait
```

Confirme a deleção após alguns minutos:

```bash
az group show --name "rg-challenge-clyvo-vet" --query "properties.provisioningState" --output tsv
# Se retornar erro "not found", a deleção foi confirmada
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
| ------ | --------------------------------- | --------------------------- |
| GET    | `/api/DispositivoIot`             | Lista todos os dispositivos |
| GET    | `/api/DispositivoIot/{id}`        | Busca dispositivo por ID    |
| GET    | `/api/DispositivoIot/pet/{petId}` | Busca dispositivo de um pet |
| POST   | `/api/DispositivoIot`             | Cria dispositivo IoT        |
| PUT    | `/api/DispositivoIot/{id}`        | Atualiza dispositivo IoT    |
| DELETE | `/api/DispositivoIot/{id}`        | Remove dispositivo IoT      |

### Alerta Inteligente

| Método | Endpoint                                 | Descrição                     |
| ------ | ---------------------------------------- | ----------------------------- |
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
SELECT * FROM "PP_DispositivosIot";
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
| ------ | --------------------- | --------------------------------------------- |
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

A API PetPulse fornece uma base funcional para o sistema de saúde preditiva pet, permitindo o cadastro de tutores, pets, histórico clínico, dispositivos IoT e alertas inteligentes. A solução utiliza ASP.NET Core, Entity Framework Core, Oracle Database e Swagger, atendendo ao escopo inicial do Challenge e permitindo evolução futura para regras mais avançadas de IA, análise preditiva e integração com dispositivos reais.
