# 📋 Estrutura do Upload de Arquivo - API Central

## 🎯 **Formato do Request**

### **Endpoint**: `POST /lotes/upload`

```json
{
  "clienteId": 1,
  "perfilProcessamentoId": 2,
  "arquivoBase64": "bm9tZSxlbmRlcmVjbyxiYWlycm8sY2lkYWRlLHVmLGNlcA0KSm9hbyBTaWx2YSxSdWEgQSAxMjMsQ2VudHJvLFNhbyBQYXVsbyxTUCwwMTAwMC0wMDA=",
  "nomeArquivo": "clientes_impressao.csv"
}
```

## 📄 **Formatos de Arquivo Suportados**

### **1. CSV (Comma Separated Values)**
**Estrutura do arquivo antes da codificação Base64:**
```csv
nome,endereco,bairro,cidade,uf,cep
João Silva,Rua A 123,Centro,Sao Paulo,SP,01000-000
Maria Santos,Av B 456,Jardins,Rio de Janeiro,RJ,20000-000
Pedro Oliveira,Rua C 789,Vila Nova,Belo Horizonte,MG,30000-000
```

### **2. TXT (Delimitado por ponto-e-vírgula)**
**Estrutura do arquivo antes da codificação Base64:**
```txt
nome;endereco;bairro;cidade;uf;cep
João Silva;Rua A 123;Centro;Sao Paulo;SP;01000-000
Maria Santos;Av B 456;Jardins;Rio de Janeiro;RJ;20000-000
Pedro Oliveira;Rua C 789;Vila Nova;Belo Horizonte;MG;30000-000
```

### **3. PCL (Printer Command Language) - Template**
**Estrutura específica para impressão:**
```
@PJL ENTER LANGUAGE=PCL
&l1O&l6D&k2G
Nome: {nome}
Endereço: {endereco}, {bairro}
Cidade: {cidade}-{uf}
CEP: {cep}
```

## 🔄 **Fluxo de Processamento**

### **1. Recebimento do Arquivo**
```json
// Request Body
{
  "clienteId": 1,              // ID do cliente (obrigatório)
  "perfilProcessamentoId": 2,  // ID do perfil de processamento (obrigatório)
  "arquivoBase64": "...",      // Arquivo codificado em Base64 (obrigatório)
  "nomeArquivo": "dados.csv"   // Nome original do arquivo (obrigatório)
}
```

### **2. Dados Gravados no Banco**

#### **Tabela: lotes_processamento**
```sql
INSERT INTO lotes_processamento (
  id,                    -- Auto increment
  cliente_id,            -- 1
  usuario_id,            -- ID do usuário logado (JWT)
  perfil_processamento_id, -- 2
  nome_arquivo,          -- "dados.csv"
  caminho_s3,            -- "lotes/guid-único/dados.csv"
  status,                -- "Recebido"
  data_criacao           -- NOW()
);
```

#### **Tabela: processamento_logs**
```sql
INSERT INTO processamento_logs (
  id,                      -- Auto increment
  lote_processamento_id,   -- ID do lote criado
  mensagem,                -- "Upload realizado com sucesso"
  tipo_log,                -- "Info"
  data_hora                -- NOW()
);
```

### **3. Processamento Assíncrono (RabbitMQ)**

#### **Mensagem Publicada:**
```json
{
  "loteId": 123,
  "clienteId": 1,
  "nomeArquivo": "dados.csv",
  "caminhoS3": "lotes/guid-único/dados.csv",
  "perfilId": 2
}
```

#### **Consumidor Processa:**
1. **Download do S3**: Baixa arquivo usando `caminhoS3`
2. **Parse do Arquivo**: Baseado no `delimitador` do perfil
3. **Geração PCL**: Usa o `templatePcl` do perfil
4. **Update Status**: Atualiza para "Processando" → "Concluido"

## 🎨 **Perfis de Processamento**

### **Exemplo de Perfis no Banco:**
```sql
-- Perfil para CSV com vírgula
INSERT INTO perfis_processamento VALUES (
  1, 1, 'Padrão CSV', 'Impressão offset padrão', 'CSV', ',', 'template_offset.pcl'
);

-- Perfil para TXT com ponto-e-vírgula
INSERT INTO perfis_processamento VALUES (
  2, 1, 'Rápido TXT', 'Impressão digital rápida', 'TXT', ';', 'template_digital.pcl'
);
```

## 📈 **Estados do Processamento**

1. **"Recebido"** - Arquivo recebido e salvo no S3
2. **"Processando"** - Consumer iniciou processamento
3. **"Concluido"** - Processamento finalizado com sucesso
4. **"Erro"** - Falha no processamento

## 🔍 **Validações Implementadas**

### **FluentValidation Rules:**
- `ClienteId` > 0
- `PerfilProcessamentoId` > 0
- `ArquivoBase64` não vazio e Base64 válido
- `NomeArquivo` não vazio e extensão válida (.csv, .txt, .pcl)
- Tamanho máximo do arquivo: 10MB

## 📊 **Response de Sucesso**

```json
{
  "loteId": 123,
  "status": "Recebido",
  "mensagem": "Arquivo enviado para processamento com sucesso",
  "dataCriacao": "2025-11-14T18:30:00Z"
}
```

## 🚀 **Exemplo Completo de Uso**

### **1. Preparar Arquivo CSV:**
```csv
nome,endereco,bairro,cidade,uf,cep
Ana Ribeiro,Rua das Flores 100,Centro,São Paulo,SP,01310-100
Carlos Mendes,Av Paulista 1000,Bela Vista,São Paulo,SP,01310-200
```

### **2. Codificar em Base64:**
```javascript
// No JavaScript/Frontend
const arquivo = "nome,endereco,bairro,cidade,uf,cep\nAna Ribeiro,Rua das Flores 100,Centro,São Paulo,SP,01310-100";
const base64 = btoa(arquivo);
```

### **3. Fazer Request:**
```bash
curl -X POST "http://localhost:5149/lotes/upload" \
  -H "Authorization: Bearer SEU_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": 1,
    "perfilProcessamentoId": 1,
    "arquivoBase64": "bm9tZSxlbmRlcmVjbyxiYWlycm8sY2lkYWRlLHVmLGNlcA0KQW5hIFJpYmVpcm8sUnVhIGRhcyBGbG9yZXMgMTAwLENlbnRybyxTw6NvIFBhdWxvLFNQLDAxMzEwLTEwMA==",
    "nomeArquivo": "clientes.csv"
  }'
```

---

> 💡 **Nota**: O arquivo é processado de forma assíncrona. Use os endpoints `/lotes/{id}` e `/lotes/{id}/logs` para acompanhar o progresso!