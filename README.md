# PetPulse API - Challenge FIAP 2026

## lembre de rodar a aplicação

link do swagger: http://localhost:5292/swagger/index.html

## Visão geral da solução

O **PetPulse** é uma API RESTful desenvolvida em **ASP.NET Core** para apoiar o projeto de **saúde preditiva pet**. A solução permite cadastrar tutores, pets, histórico clínico, dispositivos IoT e alertas inteligentes.

A proposta do sistema é centralizar informações importantes sobre o animal e permitir o acompanhamento preventivo de sua saúde. No escopo inicial, o sistema permite registrar dados do pet, seu histórico clínico, vincular uma coleira/dispositivo IoT e gerar alertas inteligentes com base em informações de comportamento, saúde e monitoramento.

A API foi construída seguindo uma arquitetura em camadas inspirada em **Clean Architecture**, separando responsabilidades entre domínio, aplicação, infraestrutura e camada de apresentação.

---

## Tecnologias utilizadas

* C#
* ASP.NET Core Web API
* Entity Framework Core
* Oracle Database
* EF Core Migrations
* Swagger / OpenAPI
* Rider / Visual Studio

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

## Executando a API

Na raiz da solução, execute:

```powershell
dotnet run --project PetPulse.API\PetPulse.API.csproj
```

Depois acesse o Swagger no navegador:

```text
http://localhost:5292/swagger
```

A porta pode variar conforme o ambiente.

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

# JSONs para teste

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
1 = Agua
2 = Alimentacao
3 = Atividade
4 = Vacina
5 = Medicamento
6 = CheckUp
7 = Comportamento
8 = FrequenciaCardiaca
9 = Pressao

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

# Testes de consulta

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

# Testes de atualização

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

# Testes de erro

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

# Consultas para conferir no Oracle

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

# Códigos HTTP utilizados

| Código | Significado           | Uso na API                                    |
| ------ | --------------------- | --------------------------------------------- |
| 200    | OK                    | Consulta ou atualização realizada com sucesso |
| 201    | Created               | Registro criado com sucesso                   |
| 204    | No Content            | Registro removido com sucesso                 |
| 400    | Bad Request           | Dados inválidos ou regra de validação violada |
| 404    | Not Found             | Registro não encontrado                       |
| 500    | Internal Server Error | Erro inesperado na aplicação                  |

---

# Observações importantes

## GUID e Oracle RAW(16)

Os IDs das entidades são do tipo `Guid` no C# e são armazenados no Oracle como `RAW(16)`. Por isso, o valor exibido diretamente no banco pode aparecer em formato hexadecimal diferente do formato textual usado pela API.

Para testar endpoints que dependem de ID, utilize sempre o `id` retornado pelos endpoints `GET` ou `POST` da própria API, e não o valor copiado diretamente do banco Oracle.

## Enums

A API utiliza enums para padronizar campos com valores fixos, como sexo do pet, porte, tipo de registro clínico, status do dispositivo, tipo do alerta, nível de risco e status do alerta.

Isso evita valores inválidos e facilita o uso da API pelo Swagger.

---

# Conclusão

A API PetPulse fornece uma base funcional para o sistema de saúde preditiva pet, permitindo o cadastro de tutores, pets, histórico clínico, dispositivos IoT e alertas inteligentes. A solução utiliza ASP.NET Core, Entity Framework Core, Oracle Database e Swagger, atendendo ao escopo inicial do Challenge e permitindo evolução futura para regras mais avançadas de IA, análise preditiva e integração com dispositivos reais.
