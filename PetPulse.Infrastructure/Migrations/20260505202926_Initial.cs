using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PP_Usuarios",
                columns: table => new
                {
                    ID_USUARIO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    CPF = table.Column<string>(type: "NVARCHAR2(14)", maxLength: 14, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    SENHA = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    ENDERECO = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: true),
                    ATIVO = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PP_Usuarios", x => x.ID_USUARIO);
                });

            migrationBuilder.CreateTable(
                name: "PP_Pets",
                columns: table => new
                {
                    ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ESPECIE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    RACA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DT_NASCIMENTO = table.Column<string>(type: "NVARCHAR2(10)", nullable: true),
                    PESO = table.Column<decimal>(type: "DECIMAL(6,2)", precision: 6, scale: 2, nullable: true),
                    SEXO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CASTRADO = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    PORTE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_USUARIO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ATIVO = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PP_Pets", x => x.ID_PET);
                    table.ForeignKey(
                        name: "FK_PP_Pets_PP_Usuarios_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalTable: "PP_Usuarios",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PP_AlertasInteligentes",
                columns: table => new
                {
                    ID_ALERTA = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TIPO_ALERTA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NIVEL_RISCO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ORIGEM_ALERTA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MENSAGEM = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    RECOMENDACAO = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DT_GERACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ATIVO = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PP_AlertasInteligentes", x => x.ID_ALERTA);
                    table.ForeignKey(
                        name: "FK_PP_AlertasInteligentes_PP_Pets_ID_PET",
                        column: x => x.ID_PET,
                        principalTable: "PP_Pets",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PP_DispositivoIots",
                columns: table => new
                {
                    ID_DISPOSITIVO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DT_VINCULACAO = table.Column<string>(type: "NVARCHAR2(10)", nullable: false),
                    INTERVALO_COLETA_MINUTOS = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FREQUENCIA_CARDIACA = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    NIVEL_ATIVIDADE = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: true),
                    PRESSAO = table.Column<decimal>(type: "DECIMAL(6,2)", precision: 6, scale: 2, nullable: true),
                    DT_ULTIMA_LEITURA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ATIVO = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PP_DispositivoIots", x => x.ID_DISPOSITIVO);
                    table.ForeignKey(
                        name: "FK_PP_DispositivoIots_PP_Pets_ID_PET",
                        column: x => x.ID_PET,
                        principalTable: "PP_Pets",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PP_HistoricoClinicos",
                columns: table => new
                {
                    ID_HISTORICO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TIPO_REGISTRO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    DT_REGISTRO = table.Column<string>(type: "NVARCHAR2(10)", nullable: false),
                    DT_RETORNO = table.Column<string>(type: "NVARCHAR2(10)", nullable: true),
                    PROFISSIONAL_CLINICA = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    OBSERVACOES = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ATIVO = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PP_HistoricoClinicos", x => x.ID_HISTORICO);
                    table.ForeignKey(
                        name: "FK_PP_HistoricoClinicos_PP_Pets_ID_PET",
                        column: x => x.ID_PET,
                        principalTable: "PP_Pets",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PP_AlertasInteligentes_ID_PET",
                table: "PP_AlertasInteligentes",
                column: "ID_PET");

            migrationBuilder.CreateIndex(
                name: "IX_PP_DispositivoIots_ID_PET",
                table: "PP_DispositivoIots",
                column: "ID_PET",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PP_HistoricoClinicos_ID_PET",
                table: "PP_HistoricoClinicos",
                column: "ID_PET");

            migrationBuilder.CreateIndex(
                name: "IX_PP_Pets_ID_USUARIO",
                table: "PP_Pets",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_PP_Usuarios_CPF",
                table: "PP_Usuarios",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PP_Usuarios_EMAIL",
                table: "PP_Usuarios",
                column: "EMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PP_AlertasInteligentes");

            migrationBuilder.DropTable(
                name: "PP_DispositivoIots");

            migrationBuilder.DropTable(
                name: "PP_HistoricoClinicos");

            migrationBuilder.DropTable(
                name: "PP_Pets");

            migrationBuilder.DropTable(
                name: "PP_Usuarios");
        }
    }
}
