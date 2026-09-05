#!/bin/bash
# =============================================================
# azure-cli.sh – Deploy do PetPulse via ACR + ACI + Key Vault
# Challenge FIAP 2026 – PetPulse

# Pré-requisitos:
#   - Azure CLI instalado e logado (az login)
#   - Docker Desktop ABERTO (necessário para login/pull/tag/push no ACR)
#   - Duas variáveis de ambiente exportadas ANTES de rodar este script
#     (nunca deixe essas senhas escritas no script ou no código-fonte —
#     elas são gravadas no Key Vault e apagadas da memória do shell
#     assim que o script termina):
#       export ORACLE_PASSWORD='senha' - senha do usuário SYS do Oracle XE
#       export ORACLE_APP_PASSWORD='senha' - senha do usuário petpulse no Oracle XE
#
# Uso:
#   chmod +x azure-cli.sh
#   ./azure-cli.sh
# =============================================================

set -e  # Interrompe em caso de erro

# Git Bash (MINGW64) reescreve argumentos que começam com "/" como caminhos
# do Windows antes de repassar ao `az`. Mantido por segurança.
export MSYS_NO_PATHCONV=1

# ---------------------------------------------------------------
# Variáveis – ajuste conforme necessário
# ---------------------------------------------------------------
RESOURCE_GROUP="rg-challenge-clyvo-vet"   # reaproveitando o grupo que já existe
LOCATION="southafricanorth"

# --- ACR / imagem da API ---
ACR_NAME="petpulse"               # precisa ser único global, só alfanumérico minúsculo
DOCKERHUB_IMAGE="pietrowilhelm/challenge-clyvo-vet:latest"
ACR_REPO="petpulse-api"
ACR_TAG="v1"

# --- ACI da API ---
API_CONTAINER_NAME="petpulse-api"
API_DNS_LABEL="petpulse"          # FQDN: petpulse.<regiao>.azurecontainer.io

# --- ACI do Oracle (banco containerizado, sem volume/persistência) ---
ORACLE_IMAGE="gvenzl/oracle-xe:21-slim"    # mesma imagem do docker-compose.yml local
ORACLE_CONTAINER_NAME="petpulse-oracle-db"
ORACLE_DNS_LABEL="petpulse-oracle"  # FQDN: petpulse-oracle.<regiao>.azurecontainer.io
ORACLE_APP_USER="petpulse"
ORACLE_SERVICE_NAME="XEPDB1"

# --- Key Vault (guarda todas as senhas/credenciais) ---
KEY_VAULT_NAME="petpulse-kv"      # precisa ser único global, 3-24 caracteres

if [ -z "$ORACLE_PASSWORD" ] || [ -z "$ORACLE_APP_PASSWORD" ]; then
  echo "ERRO: variáveis de ambiente ORACLE_PASSWORD e/ou ORACLE_APP_PASSWORD não definidas."
  echo "Rode antes:"
  echo "  export ORACLE_PASSWORD='senha_do_usuario_sys_do_oracle'"
  echo "  export ORACLE_APP_PASSWORD='senha_do_usuario_petpulse_no_oracle'"
  exit 1
fi

echo "=============================================="
echo " Deploy PetPulse – ACR + ACI + Key Vault"
echo "=============================================="

# ---------------------------------------------------------------
# 1. Confirma que o Resource Group já existe (reaproveitando)
# ---------------------------------------------------------------
echo "[1/8] Verificando Resource Group: $RESOURCE_GROUP..."
if ! az group show --name "$RESOURCE_GROUP" &>/dev/null; then
  echo "  Grupo não encontrado, criando..."
  az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
else
  echo "  Grupo já existe, reaproveitando."
fi

# ---------------------------------------------------------------
# 2. Registrar os providers necessários (idempotente, só precisa 1x por assinatura)
# ---------------------------------------------------------------
echo "[2/8] Registrando providers necessários..."
az provider register --namespace Microsoft.ContainerRegistry
az provider register --namespace Microsoft.ContainerInstance
az provider register --namespace Microsoft.KeyVault

echo "  Aguardando registro de Microsoft.ContainerInstance..."
while [ "$(az provider show --namespace Microsoft.ContainerInstance --query registrationState --output tsv)" != "Registered" ]; do
  sleep 5
  echo "  ainda registrando..."
done
echo "  Providers registrados."

# ---------------------------------------------------------------
# 3. Criar o Azure Container Registry e recuperar credenciais
# ---------------------------------------------------------------
echo "[3/8] Criando ACR: $ACR_NAME (ou reaproveitando se já existir)..."
if ! az acr show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
  az acr create \
    --resource-group "$RESOURCE_GROUP" \
    --name "$ACR_NAME" \
    --sku Basic \
    --location "$LOCATION" \
    --admin-enabled true
else
  echo "  ACR já existe, reaproveitando."
fi

LOGIN_SERVER=$(az acr show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --query loginServer --output tsv)
ACR_ADMIN_USERNAME=$(az acr credential show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --query username --output tsv)
ACR_ADMIN_PASSWORD=$(az acr credential show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --query "passwords[0].value" --output tsv)
echo "  Login Server: $LOGIN_SERVER"

# ---------------------------------------------------------------
# 4. Logar no ACR, puxar a imagem da API do Docker Hub, re-taguear e subir
# ---------------------------------------------------------------
echo "[4/8] Login no ACR, pull/tag/push da imagem da API..."
az acr login --name "$ACR_NAME"
docker pull "$DOCKERHUB_IMAGE"
docker tag "$DOCKERHUB_IMAGE" "$LOGIN_SERVER/$ACR_REPO:$ACR_TAG"
docker push "$LOGIN_SERVER/$ACR_REPO:$ACR_TAG"
az acr repository list --name "$ACR_NAME" --output table

# ---------------------------------------------------------------
# 5. Criar o Key Vault e gravar todas as senhas/credenciais nele
# ---------------------------------------------------------------
echo "[5/8] Criando Key Vault: $KEY_VAULT_NAME (ou reaproveitando se já existir)..."
if ! az keyvault show --name "$KEY_VAULT_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
  az keyvault create --name "$KEY_VAULT_NAME" --resource-group "$RESOURCE_GROUP" --location "$LOCATION"
else
  echo "  Key Vault já existe, reaproveitando."
fi

echo "  Concedendo à sua conta o papel 'Key Vault Administrator'..."
az role assignment create \
  --assignee "$(az account show --query user.name -o tsv)" \
  --role "Key Vault Administrator" \
  --scope "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.KeyVault/vaults/$KEY_VAULT_NAME" \
  2>/dev/null || echo "  (papel já concedido anteriormente, seguindo em frente)"

# A permissão do RBAC recém-concedida pode levar alguns segundos para propagar.
echo "  Gravando segredos no Key Vault..."
SECRETS_OK=false
for i in $(seq 1 6); do
  if az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name oracle-password --value "$ORACLE_PASSWORD" &>/dev/null; then
    SECRETS_OK=true
    break
  fi
  echo "  aguardando a permissão do Key Vault propagar... (tentativa $i/6)"
  sleep 15
done

if [ "$SECRETS_OK" = false ]; then
  echo "ERRO: não foi possível gravar segredos no Key Vault (permissão RBAC pode não ter propagado)."
  echo "Rode o script de novo em alguns minutos, ou grave manualmente com:"
  echo "  az keyvault secret set --vault-name $KEY_VAULT_NAME --name oracle-password --value '<senha>'"
  exit 1
fi

az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name oracle-app-password --value "$ORACLE_APP_PASSWORD" >/dev/null
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name acr-username --value "$ACR_ADMIN_USERNAME" >/dev/null
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name acr-password --value "$ACR_ADMIN_PASSWORD" >/dev/null
echo "  Segredos gravados: oracle-password, oracle-app-password, acr-username, acr-password."

# ---------------------------------------------------------------
# 6. Criar a ACI do Oracle (banco containerizado, sem volume)
# ---------------------------------------------------------------
echo "[6/8] Recriando a ACI do Oracle: $ORACLE_CONTAINER_NAME..."
az container delete --resource-group "$RESOURCE_GROUP" --name "$ORACLE_CONTAINER_NAME" --yes 2>/dev/null || true

az container create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$ORACLE_CONTAINER_NAME" \
  --image "$ORACLE_IMAGE" \
  --os-type Linux \
  --cpu 2 \
  --memory 4 \
  --ports 1521 \
  --dns-name-label "$ORACLE_DNS_LABEL" \
  --environment-variables APP_USER="$ORACLE_APP_USER" \
  --secure-environment-variables \
    ORACLE_PASSWORD="$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name oracle-password --query value -o tsv)" \
    APP_USER_PASSWORD="$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name oracle-app-password --query value -o tsv)" \
  --restart-policy Never

ORACLE_FQDN=$(az container show --resource-group "$RESOURCE_GROUP" --name "$ORACLE_CONTAINER_NAME" --query ipAddress.fqdn --output tsv)
echo "  Oracle FQDN: $ORACLE_FQDN"

# Em vez de um "sleep" fixo, fica conferindo os logs até aparecer a linha
# que o próprio Oracle XE imprime quando termina de fato a inicialização.
echo "[7/8] Aguardando o Oracle XE inicializar totalmente (pode levar de 3 a 10 minutos na primeira vez)..."
ORACLE_READY=false
for i in $(seq 1 60); do
  if az container logs --resource-group "$RESOURCE_GROUP" --name "$ORACLE_CONTAINER_NAME" 2>/dev/null | grep -q "DATABASE IS READY TO USE"; then
    ORACLE_READY=true
    break
  fi
  echo "  ainda inicializando... (tentativa $i/60, ~$((i*15))s decorridos)"
  sleep 15
done

if [ "$ORACLE_READY" = false ]; then
  echo "ERRO: o Oracle não sinalizou pronto dentro do tempo esperado. Verifique os logs:"
  echo "  az container logs --resource-group $RESOURCE_GROUP --name $ORACLE_CONTAINER_NAME"
  exit 1
fi
echo "  Oracle pronto!"

# ---------------------------------------------------------------
# 7. Criar (ou recriar) a ACI da API, apontando para o Oracle containerizado
# ---------------------------------------------------------------
ORACLE_APP_PASSWORD_FROM_VAULT=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name oracle-app-password --query value -o tsv)
ORACLE_CONNECTION="Data Source=${ORACLE_FQDN}:1521/${ORACLE_SERVICE_NAME};User ID=${ORACLE_APP_USER};Password=${ORACLE_APP_PASSWORD_FROM_VAULT};"

echo "[8/8] Recriando ACI da API (se já existir, remove antes para atualizar a connection string)..."
az container delete --resource-group "$RESOURCE_GROUP" --name "$API_CONTAINER_NAME" --yes 2>/dev/null || true

az container create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$API_CONTAINER_NAME" \
  --image "$LOGIN_SERVER/$ACR_REPO:$ACR_TAG" \
  --registry-login-server "$LOGIN_SERVER" \
  --registry-username "$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name acr-username --query value -o tsv)" \
  --registry-password "$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name acr-password --query value -o tsv)" \
  --os-type Linux \
  --cpu 1 \
  --memory 1.5 \
  --ports 8080 \
  --dns-name-label "$API_DNS_LABEL" \
  --environment-variables ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://+:8080 \
  --secure-environment-variables ConnectionStrings__PetPulseOracle="$ORACLE_CONNECTION" \
  --restart-policy OnFailure

# ---------------------------------------------------------------
# 8. Exibir FQDNs e como testar
# ---------------------------------------------------------------
API_FQDN=$(az container show --resource-group "$RESOURCE_GROUP" --name "$API_CONTAINER_NAME" --query ipAddress.fqdn --output tsv)

echo ""
echo "=============================================="
echo " Deploy concluído! (App e Banco, ambos containerizados, segredos no Key Vault)"
echo " Key Vault              : $KEY_VAULT_NAME"
echo " Oracle FQDN (interno)  : $ORACLE_FQDN:1521/$ORACLE_SERVICE_NAME"
echo " API FQDN               : $API_FQDN"
echo " Swagger (API)          : http://$API_FQDN:8080/swagger"
echo " Health Check           : http://$API_FQDN:8080/health"
echo " Métricas               : http://$API_FQDN:8080/metrics"
echo "=============================================="
echo ""
echo "Teste com:"
echo "  curl http://$API_FQDN:8080/health"
echo ""
echo "Para ver logs:"
echo "  az container logs --resource-group $RESOURCE_GROUP --name $API_CONTAINER_NAME"
echo "  az container logs --resource-group $RESOURCE_GROUP --name $ORACLE_CONTAINER_NAME"
echo ""
echo "Para ver os segredos gravados (sem exibir o valor):"
echo "  az keyvault secret list --vault-name $KEY_VAULT_NAME --output table"
echo ""
echo "Para remover os containers (sem apagar ACR/Key Vault):"
echo "  az container delete --resource-group $RESOURCE_GROUP --name $API_CONTAINER_NAME --yes"
echo "  az container delete --resource-group $RESOURCE_GROUP --name $ORACLE_CONTAINER_NAME --yes"