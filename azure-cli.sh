#!/bin/bash
# =============================================================
# azure-cli.sh – Provisionamento da infraestrutura Azure
# Challenge FIAP 2026 – PetPulse (Clyvo Vet)
#
# Pré-requisitos:
#   - Azure CLI instalado e logado (az login)
#   - Permissão para criar recursos na assinatura
#
# Uso:
#   chmod +x azure-cli.sh
#   ./azure-cli.sh
# =============================================================

set -e  # Interrompe em caso de erro

# ---------------------------------------------------------------
# Variáveis – ajuste conforme necessário
# ---------------------------------------------------------------
RESOURCE_GROUP="rg-challenge-clyvo-vet"
LOCATION="southafricanorth"
VM_NAME="vm-petpulse"
VM_SIZE="Standard_B4ls_v2"
VM_IMAGE="Ubuntu2204"
ADMIN_USER="adminlnx"
DOCKERHUB_USER="pietrowilhelm"
IMAGE_TAG="latest"

echo "=============================================="
echo " Provisionando infraestrutura PetPulse – Azure"
echo "=============================================="

# ---------------------------------------------------------------
# 1. Resource Group
# ---------------------------------------------------------------
echo "[1/6] Criando Resource Group: $RESOURCE_GROUP ($LOCATION)..."
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION"

# ---------------------------------------------------------------
# 2. Máquina Virtual Ubuntu 22.04
# ---------------------------------------------------------------
echo "[2/6] Criando VM: $VM_NAME ($VM_SIZE)..."
az vm create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$VM_NAME" \
  --size "$VM_SIZE" \
  --image "$VM_IMAGE" \
  --admin-username "$ADMIN_USER" \
  --generate-ssh-keys \
  --public-ip-sku Standard \
  --output table

# ---------------------------------------------------------------
# 3. Abertura de portas (NSG)
#    22   – SSH
#    8080 – PetPulse API
#    1521 – Oracle (opcional, acesso externo)
# ---------------------------------------------------------------
echo "[3/6] Abrindo portas 22, 8080 e 1521..."
az vm open-port --resource-group "$RESOURCE_GROUP" --name "$VM_NAME" --port 22   --priority 100
az vm open-port --resource-group "$RESOURCE_GROUP" --name "$VM_NAME" --port 8080 --priority 110
az vm open-port --resource-group "$RESOURCE_GROUP" --name "$VM_NAME" --port 1521 --priority 120

# ---------------------------------------------------------------
# 4. Instalação do Docker na VM via run-command
# ---------------------------------------------------------------
echo "[4/6] Instalando Docker na VM..."
az vm run-command invoke \
  --resource-group "$RESOURCE_GROUP" \
  --name "$VM_NAME" \
  --command-id RunShellScript \
  --scripts "
    apt-get update -y
    apt-get install -y ca-certificates curl gnupg lsb-release git nano
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    chmod a+r /etc/apt/keyrings/docker.gpg
    echo \"deb [arch=\$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \$(lsb_release -cs) stable\" \
      | tee /etc/apt/sources.list.d/docker.list > /dev/null
    apt-get update -y
    apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
    systemctl enable docker
    systemctl start docker
    usermod -aG docker $ADMIN_USER
  "

# ---------------------------------------------------------------
# 5. Clonar repositório e subir o docker compose
# ---------------------------------------------------------------
echo "[5/6] Clonando repositório e iniciando containers..."
az vm run-command invoke \
  --resource-group "$RESOURCE_GROUP" \
  --name "$VM_NAME" \
  --command-id RunShellScript \
  --scripts "
    cd /home/$ADMIN_USER
    git clone https://github.com/Fiap-2tdspg-Challenge/PetPulse.git || (cd PetPulse && git pull)
    cd PetPulse
    DOCKERHUB_USER=$DOCKERHUB_USER docker compose up -d
  "

# ---------------------------------------------------------------
# 6. Exibir IP público da VM
# ---------------------------------------------------------------
echo "[6/6] Obtendo IP público da VM..."
PUBLIC_IP=$(az vm show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$VM_NAME" \
  --show-details \
  --query publicIps \
  --output tsv)

echo ""
echo "=============================================="
echo " Provisionamento concluído!"
echo " IP Público da VM : $PUBLIC_IP"
echo " Swagger (API)    : http://$PUBLIC_IP:8080/swagger"
echo " SSH              : ssh $ADMIN_USER@$PUBLIC_IP"
echo "=============================================="
