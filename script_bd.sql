-- =============================================================
-- script_bd.sql
-- PetPulse - Challenge FIAP 2026
-- DDL das tabelas CORE da aplicação (Oracle Database)
--
-- Reflete EXATAMENTE o schema gerado pelo EF Core (Oracle.EntityFrameworkCore),
-- conforme o PetPulseContextModelSnapshot.cs do projeto (nomes de tabela e
-- de coluna copiados diretamente de lá). Este script serve como documentação
-- explícita da estrutura do banco, exigida na entrega da Sprint (DevOps
-- Tools and Cloud Computing).
-- =============================================================

-- ============================================================
-- Tabela: PP_Usuarios
-- Tutores/responsáveis pelos pets cadastrados na plataforma
-- ============================================================
CREATE TABLE "PP_Usuarios" (
    "ID_USUARIO"  RAW(16)         NOT NULL,
    "NOME"        NVARCHAR2(150)  NOT NULL,
    "CPF"         NVARCHAR2(14)   NOT NULL,
    "EMAIL"       NVARCHAR2(150)  NOT NULL,
    "SENHA"       NVARCHAR2(255)  NOT NULL,
    "TELEFONE"    NVARCHAR2(20),
    "ENDERECO"    NVARCHAR2(255),
    "ATIVO"       NUMBER(1)       DEFAULT 1 NOT NULL,
    "DT_CADASTRO" TIMESTAMP(7)    DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT "PK_PP_Usuarios" PRIMARY KEY ("ID_USUARIO"),
    CONSTRAINT "UQ_PP_Usuarios_Email" UNIQUE ("EMAIL"),
    CONSTRAINT "UQ_PP_Usuarios_Cpf" UNIQUE ("CPF")
);

COMMENT ON TABLE "PP_Usuarios" IS 'Tutores/responsáveis pelos pets cadastrados na plataforma.';
COMMENT ON COLUMN "PP_Usuarios"."ID_USUARIO" IS 'Identificador único do usuário (GUID).';
COMMENT ON COLUMN "PP_Usuarios"."NOME" IS 'Nome completo do tutor.';
COMMENT ON COLUMN "PP_Usuarios"."CPF" IS 'CPF do tutor (único).';
COMMENT ON COLUMN "PP_Usuarios"."EMAIL" IS 'E-mail do tutor (único, usado para regras de cadastro).';
COMMENT ON COLUMN "PP_Usuarios"."SENHA" IS 'Senha do tutor (hash).';
COMMENT ON COLUMN "PP_Usuarios"."TELEFONE" IS 'Telefone de contato (opcional).';
COMMENT ON COLUMN "PP_Usuarios"."ENDERECO" IS 'Endereço do tutor (opcional).';
COMMENT ON COLUMN "PP_Usuarios"."ATIVO" IS 'Indica se o registro está ativo (1) ou inativo/excluído logicamente (0).';
COMMENT ON COLUMN "PP_Usuarios"."DT_CADASTRO" IS 'Data/hora de criação do registro.';

-- ============================================================
-- Tabela: PP_Pets
-- Animais cadastrados, vinculados a um tutor (Usuario)
-- ============================================================
CREATE TABLE "PP_Pets" (
    "ID_PET"        RAW(16)         NOT NULL,
    "ID_USUARIO"    RAW(16)         NOT NULL,
    "NOME"          NVARCHAR2(100)  NOT NULL,
    "ESPECIE"       NVARCHAR2(50)   NOT NULL,
    "RACA"          NVARCHAR2(100),
    "DT_NASCIMENTO" NVARCHAR2(10),
    "PESO"          DECIMAL(6,2),
    "SEXO"          NUMBER(10)      NOT NULL,
    "CASTRADO"      NUMBER(1)       NOT NULL,
    "PORTE"         NUMBER(10)      NOT NULL,
    "ATIVO"         NUMBER(1)       DEFAULT 1 NOT NULL,
    "DT_CADASTRO"   TIMESTAMP(7)    DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT "PK_PP_Pets" PRIMARY KEY ("ID_PET"),
    CONSTRAINT "FK_PP_Pets_Usuarios" FOREIGN KEY ("ID_USUARIO") REFERENCES "PP_Usuarios" ("ID_USUARIO") ON DELETE CASCADE
);

CREATE INDEX "IX_PP_Pets_ID_USUARIO" ON "PP_Pets" ("ID_USUARIO");

COMMENT ON TABLE "PP_Pets" IS 'Animais cadastrados, vinculados a um tutor (Usuario). Relação Usuario 1:N Pet.';
COMMENT ON COLUMN "PP_Pets"."ID_PET" IS 'Identificador único do pet (GUID).';
COMMENT ON COLUMN "PP_Pets"."ID_USUARIO" IS 'FK para PP_Usuarios — tutor responsável pelo pet.';
COMMENT ON COLUMN "PP_Pets"."NOME" IS 'Nome do pet.';
COMMENT ON COLUMN "PP_Pets"."ESPECIE" IS 'Espécie do pet (ex: Cachorro, Gato).';
COMMENT ON COLUMN "PP_Pets"."RACA" IS 'Raça do pet (opcional).';
COMMENT ON COLUMN "PP_Pets"."DT_NASCIMENTO" IS 'Data de nascimento do pet, formato ISO (yyyy-MM-dd), opcional.';
COMMENT ON COLUMN "PP_Pets"."PESO" IS 'Peso do pet em kg (opcional, deve ser maior que zero quando informado).';
COMMENT ON COLUMN "PP_Pets"."SEXO" IS 'Sexo do pet: 1=Macho, 2=Fêmea, 3=NaoInformado.';
COMMENT ON COLUMN "PP_Pets"."CASTRADO" IS 'Indica se o pet é castrado (1) ou não (0).';
COMMENT ON COLUMN "PP_Pets"."PORTE" IS 'Porte do pet: 1=Pequeno, 2=Medio, 3=Grande, 4=NaoInformado.';
COMMENT ON COLUMN "PP_Pets"."ATIVO" IS 'Indica se o registro está ativo (1) ou inativo/excluído logicamente (0).';
COMMENT ON COLUMN "PP_Pets"."DT_CADASTRO" IS 'Data/hora de criação do registro.';

-- ============================================================
-- Tabela: PP_HistoricoClinicos
-- Registros clínicos do pet: vacinas, consultas, exames etc.
-- ============================================================
CREATE TABLE "PP_HistoricoClinicos" (
    "ID_HISTORICO"         RAW(16)         NOT NULL,
    "ID_PET"               RAW(16)         NOT NULL,
    "TIPO_REGISTRO"        NUMBER(10)      NOT NULL,
    "DESCRICAO"            NVARCHAR2(500)  NOT NULL,
    "DT_REGISTRO"          NVARCHAR2(10)   NOT NULL,
    "DT_RETORNO"           NVARCHAR2(10),
    "PROFISSIONAL_CLINICA" NVARCHAR2(150),
    "OBSERVACOES"          NVARCHAR2(1000),
    "ATIVO"                NUMBER(1)       DEFAULT 1 NOT NULL,
    "DT_CADASTRO"          TIMESTAMP(7)    DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT "PK_PP_HistoricoClinicos" PRIMARY KEY ("ID_HISTORICO"),
    CONSTRAINT "FK_PP_HistClin_Pets" FOREIGN KEY ("ID_PET") REFERENCES "PP_Pets" ("ID_PET") ON DELETE CASCADE
);

CREATE INDEX "IX_PP_HistClin_ID_PET" ON "PP_HistoricoClinicos" ("ID_PET");

COMMENT ON TABLE "PP_HistoricoClinicos" IS 'Registros clínicos do pet: vacinas, consultas, doenças, medicamentos, observações e exames. Relação Pet 1:N HistoricoClinico.';
COMMENT ON COLUMN "PP_HistoricoClinicos"."ID_HISTORICO" IS 'Identificador único do registro clínico (GUID).';
COMMENT ON COLUMN "PP_HistoricoClinicos"."ID_PET" IS 'FK para PP_Pets — pet ao qual o registro pertence.';
COMMENT ON COLUMN "PP_HistoricoClinicos"."TIPO_REGISTRO" IS 'Tipo: 1=Vacina, 2=Consulta, 3=Doenca, 4=Medicamento, 5=Observacao, 6=Exame.';
COMMENT ON COLUMN "PP_HistoricoClinicos"."DESCRICAO" IS 'Descrição do registro clínico.';
COMMENT ON COLUMN "PP_HistoricoClinicos"."DT_REGISTRO" IS 'Data em que o registro foi realizado, formato ISO (yyyy-MM-dd).';
COMMENT ON COLUMN "PP_HistoricoClinicos"."DT_RETORNO" IS 'Data de retorno/próxima dose, formato ISO (yyyy-MM-dd), opcional.';
COMMENT ON COLUMN "PP_HistoricoClinicos"."PROFISSIONAL_CLINICA" IS 'Profissional ou clínica responsável (opcional).';
COMMENT ON COLUMN "PP_HistoricoClinicos"."OBSERVACOES" IS 'Observações adicionais (opcional).';
COMMENT ON COLUMN "PP_HistoricoClinicos"."ATIVO" IS 'Indica se o registro está ativo (1) ou inativo/excluído logicamente (0).';
COMMENT ON COLUMN "PP_HistoricoClinicos"."DT_CADASTRO" IS 'Data/hora de criação do registro.';

-- ============================================================
-- Tabela: PP_DispositivoIots
-- Dispositivo IoT (coleira) vinculado a um pet (relação 1:1)
-- ============================================================
CREATE TABLE "PP_DispositivoIots" (
    "ID_DISPOSITIVO"           RAW(16)       NOT NULL,
    "ID_PET"                   RAW(16)       NOT NULL,
    "DT_VINCULACAO"            NVARCHAR2(10) NOT NULL,
    "INTERVALO_COLETA_MINUTOS" NUMBER(10),
    "FREQUENCIA_CARDIACA"      NUMBER(10),
    "NIVEL_ATIVIDADE"          DECIMAL(5,2),
    "PRESSAO"                  DECIMAL(6,2),
    "DT_ULTIMA_LEITURA"        TIMESTAMP(7),
    "STATUS"                   NUMBER(10)    NOT NULL,
    "ATIVO"                    NUMBER(1)     DEFAULT 1 NOT NULL,
    "DT_CADASTRO"              TIMESTAMP(7)  DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT "PK_PP_DispositivoIots" PRIMARY KEY ("ID_DISPOSITIVO"),
    CONSTRAINT "UQ_PP_DispositivoIots_ID_PET" UNIQUE ("ID_PET"),
    CONSTRAINT "FK_PP_DispIot_Pets" FOREIGN KEY ("ID_PET") REFERENCES "PP_Pets" ("ID_PET") ON DELETE CASCADE
);

COMMENT ON TABLE "PP_DispositivoIots" IS 'Dispositivo IoT (coleira) vinculado a um pet. Relação Pet 1:1 DispositivoIot (UNIQUE em ID_PET).';
COMMENT ON COLUMN "PP_DispositivoIots"."ID_DISPOSITIVO" IS 'Identificador único do dispositivo (GUID).';
COMMENT ON COLUMN "PP_DispositivoIots"."ID_PET" IS 'FK única para PP_Pets — cada pet tem no máximo um dispositivo vinculado.';
COMMENT ON COLUMN "PP_DispositivoIots"."DT_VINCULACAO" IS 'Data em que o dispositivo foi vinculado ao pet, formato ISO (yyyy-MM-dd).';
COMMENT ON COLUMN "PP_DispositivoIots"."INTERVALO_COLETA_MINUTOS" IS 'Intervalo entre coletas de dados, em minutos (opcional, > 0 quando informado).';
COMMENT ON COLUMN "PP_DispositivoIots"."FREQUENCIA_CARDIACA" IS 'Última leitura de frequência cardíaca, em bpm (opcional, > 0 quando informado).';
COMMENT ON COLUMN "PP_DispositivoIots"."NIVEL_ATIVIDADE" IS 'Último nível de atividade registrado (opcional, >= 0).';
COMMENT ON COLUMN "PP_DispositivoIots"."PRESSAO" IS 'Última leitura de pressão (opcional, > 0 quando informado).';
COMMENT ON COLUMN "PP_DispositivoIots"."DT_ULTIMA_LEITURA" IS 'Data/hora da última leitura recebida do dispositivo (opcional).';
COMMENT ON COLUMN "PP_DispositivoIots"."STATUS" IS 'Status do dispositivo: 1=Ativo, 2=Inativo, 3=Manutencao.';
COMMENT ON COLUMN "PP_DispositivoIots"."ATIVO" IS 'Indica se o registro está ativo (1) ou inativo/excluído logicamente (0).';
COMMENT ON COLUMN "PP_DispositivoIots"."DT_CADASTRO" IS 'Data/hora de criação do registro.';

-- ============================================================
-- Tabela: PP_AlertasInteligentes
-- Alertas gerados a partir do cruzamento de dados clínicos e IoT
-- ============================================================
CREATE TABLE "PP_AlertasInteligentes" (
    "ID_ALERTA"     RAW(16)         NOT NULL,
    "ID_PET"        RAW(16)         NOT NULL,
    "TIPO_ALERTA"   NUMBER(10)      NOT NULL,
    "NIVEL_RISCO"   NUMBER(10)      NOT NULL,
    "ORIGEM_ALERTA" NUMBER(10)      NOT NULL,
    "MENSAGEM"      NVARCHAR2(500)  NOT NULL,
    "RECOMENDACAO"  NVARCHAR2(1000),
    "DT_GERACAO"    TIMESTAMP(7)    NOT NULL,
    "STATUS"        NUMBER(10)      NOT NULL,
    "ATIVO"         NUMBER(1)       DEFAULT 1 NOT NULL,
    "DT_CADASTRO"   TIMESTAMP(7)    DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT "PK_PP_AlertasInteligentes" PRIMARY KEY ("ID_ALERTA"),
    CONSTRAINT "FK_PP_Alertas_Pets" FOREIGN KEY ("ID_PET") REFERENCES "PP_Pets" ("ID_PET") ON DELETE CASCADE
);

CREATE INDEX "IX_PP_Alertas_ID_PET" ON "PP_AlertasInteligentes" ("ID_PET");

COMMENT ON TABLE "PP_AlertasInteligentes" IS 'Alertas inteligentes gerados a partir do cruzamento de dados clínicos e do dispositivo IoT. Relação Pet 1:N AlertaInteligente.';
COMMENT ON COLUMN "PP_AlertasInteligentes"."ID_ALERTA" IS 'Identificador único do alerta (GUID).';
COMMENT ON COLUMN "PP_AlertasInteligentes"."ID_PET" IS 'FK para PP_Pets — pet ao qual o alerta se refere.';
COMMENT ON COLUMN "PP_AlertasInteligentes"."TIPO_ALERTA" IS 'Tipo: 1=Atividade, 2=Vacina, 3=Medicamento, 4=CheckUp, 5=FrequenciaCardiaca, 6=Pressao.';
COMMENT ON COLUMN "PP_AlertasInteligentes"."NIVEL_RISCO" IS 'Nível de risco: 1=Baixo, 2=Medio, 3=Alto.';
COMMENT ON COLUMN "PP_AlertasInteligentes"."ORIGEM_ALERTA" IS 'Origem: 1=HistoricoClinico, 2=DispositivoIot, 3=Sistema, 4=Usuario.';
COMMENT ON COLUMN "PP_AlertasInteligentes"."MENSAGEM" IS 'Mensagem descritiva do alerta.';
COMMENT ON COLUMN "PP_AlertasInteligentes"."RECOMENDACAO" IS 'Recomendação associada ao alerta (opcional).';
COMMENT ON COLUMN "PP_AlertasInteligentes"."DT_GERACAO" IS 'Data/hora em que o alerta foi gerado.';
COMMENT ON COLUMN "PP_AlertasInteligentes"."STATUS" IS 'Status do alerta: 1=Aberto, 2=Visualizado, 3=Resolvido.';
COMMENT ON COLUMN "PP_AlertasInteligentes"."ATIVO" IS 'Indica se o registro está ativo (1) ou inativo/excluído logicamente (0).';
COMMENT ON COLUMN "PP_AlertasInteligentes"."DT_CADASTRO" IS 'Data/hora de criação do registro.';