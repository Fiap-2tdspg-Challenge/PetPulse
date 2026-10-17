#!/usr/bin/env bash
# =============================================================
# infra-webapp.sh – Infraestrutura do PetPulse para o CI/CD (Sprint 4)
# Challenge FIAP 2026 – PetPulse
#
# Executado pela pipeline de CI no Azure DevOps, no job "Criar Infra Inicial"
# (tarefa Azure CLI), antes do job "Build and Test" — mesmo modelo do lab
# "Azure Pipelines API .NET com Oracle".
#
# É idempotente: pode rodar a cada build sem efeito colateral.
#   1. Banco Oracle (FIAP): cria as tabelas do script_bd.sql SOMENTE se elas
#      ainda não existirem. Nunca apaga nem altera dados existentes.
#   2. Azure: cria (ou reaproveita) o Grupo de Recursos, o Plano de Serviço e o
#      Serviço de Aplicativo (Web App) onde o CD publica a API.
#
# Parâmetros POSICIONAIS (campo "Script Arguments" da tarefa Azure CLI):
#   $1 ORACLE_HOST   $2 ORACLE_PORT   $3 ORACLE_SID   $4 ORACLE_USER   $5 ORACLE_PASS
#   A senha chega como argumento porque variáveis SECRETAS da pipeline não são
#   expostas como variável de ambiente para scripts.
#
# Variáveis de ambiente (variáveis comuns da pipeline, expostas automaticamente):
#   NOME_WEBAPP – nome do Web App, único em todo o Azure (ex.: petpulse-rm561378)
#   LOCATION    – região do Azure (ex.: brazilsouth)
#   RUNTIME     – opcional, runtime do Web App (padrão: dotnet:10)
# =============================================================
set -euo pipefail

DbHost="${1:?ORACLE_HOST ausente}"
DbPort="${2:?ORACLE_PORT ausente}"
DbSid="${3:?ORACLE_SID ausente}"
DbUser="${4:?ORACLE_USER ausente}"
DbPass="${5:?ORACLE_PASS ausente}"

app="${NOME_WEBAPP:?Variável NOME_WEBAPP não definida na pipeline}"
location="${LOCATION:?Variável LOCATION não definida na pipeline}"
runtime="${RUNTIME:-dotnet:10}"
rg="rg-petpulse-webapp"
plan="plan-petpulse"
sku="F1"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DDL_FILE="$SCRIPT_DIR/script_bd.sql"

# ===================================
# 1. Banco de Dados Oracle
# ===================================

# SQLcl portátil (cliente de linha de comando do Oracle)
if [ ! -x "$HOME/sqlcl/bin/sql" ]; then
  echo "Baixando SQLcl..."
  curl -sSL -o "$HOME/sqlcl.zip" "https://download.oracle.com/otn_software/java/sqldeveloper/sqlcl-latest.zip"
  echo "Extraindo SQLcl..."
  unzip -q -o "$HOME/sqlcl.zip" -d "$HOME"
fi
SQLBIN="$HOME/sqlcl/bin/sql"
CONN="${DbUser}/${DbPass}@${DbHost}:${DbPort}/${DbSid}"

echo "Verificando se as tabelas do PetPulse já existem no Oracle..."
existe=$("$SQLBIN" -S "$CONN" <<'SQL' | tr -d '[:space:]'
SET HEADING OFF
SET FEEDBACK OFF
SET PAGESIZE 0
SELECT COUNT(*) FROM user_tables WHERE table_name = 'PP_Usuarios';
EXIT
SQL
) || { echo "ERRO: não foi possível consultar o Oracle (verifique host, usuário e senha)."; exit 1; }

if ! [[ "$existe" =~ ^[0-9]+$ ]]; then
  echo "ERRO: resposta inesperada do Oracle ao verificar as tabelas:"
  echo "$existe"
  exit 1
fi

if [ "$existe" = "0" ]; then
  echo "Tabelas não encontradas: criando a partir do script_bd.sql..."
  {
    echo "WHENEVER SQLERROR EXIT SQL.SQLCODE"
    tr -d '\r' < "$DDL_FILE"   # o script_bd.sql é salvo com quebras de linha do Windows
    echo
    echo "EXIT"
  } > cria_objetos.sql
  "$SQLBIN" -S "$CONN" @cria_objetos.sql
  echo "[OK] Tabelas do PetPulse criadas"
else
  echo "[OK] Tabelas já existem — nada a fazer no banco (dados preservados)"
fi

# ===================================
# 2. Recursos da Azure
# ===================================

echo "Runtimes .NET disponíveis para Web App Windows nesta assinatura:"
az webapp list-runtimes --os windows -o tsv | grep -i dotnet || true

echo "Criando (ou reaproveitando) o Grupo de Recursos $rg..."
az group create --name "$rg" --location "$location" 1>/dev/null

echo "Criando (ou reaproveitando) o Plano de Serviço $plan ($sku)..."
az appservice plan create --name "$plan" --resource-group "$rg" --location "$location" --sku "$sku" 1>/dev/null

if az webapp show --resource-group "$rg" --name "$app" &>/dev/null; then
  echo "Serviço de Aplicativo $app já existe — reaproveitando."
else
  echo "Criando o Serviço de Aplicativo $app (runtime $runtime)..."
  az webapp create --resource-group "$rg" --plan "$plan" --runtime "$runtime" --name "$app" 1>/dev/null
fi

# Logs do Serviço de Aplicativo (aplica só se ainda não estiverem como desejado)
app_logging="$(az webapp log show -g "$rg" -n "$app" --query 'applicationLogs.fileSystem.level' -o tsv 2>/dev/null || true)"
ws_logging="$(az webapp log show -g "$rg" -n "$app" --query 'httpLogs.fileSystem.enabled' -o tsv 2>/dev/null || true)"
det_errors="$(az webapp log show -g "$rg" -n "$app" --query 'detailedErrorMessages.enabled' -o tsv 2>/dev/null || true)"
failed_req="$(az webapp log show -g "$rg" -n "$app" --query 'failedRequestsTracing.enabled' -o tsv 2>/dev/null || true)"

if [ "$app_logging" != "Information" ] || [ "$ws_logging" != "true" ] || [ "$det_errors" != "true" ] || [ "$failed_req" != "true" ]; then
  echo "Habilitando logs do Serviço de Aplicativo..."
  az webapp log config \
    --resource-group "$rg" \
    --name "$app" \
    --application-logging filesystem \
    --web-server-logging filesystem \
    --level information \
    --detailed-error-messages true \
    --failed-request-tracing true 1>/dev/null
else
  echo "Logs já configurados."
fi

echo "[OK] Recursos da Azure prontos"
echo "API (após o primeiro deploy do CD): https://${app}.azurewebsites.net/swagger"
