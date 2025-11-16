# � API Central - Plataforma PCL
**HG09 — Implementação .NET 8 Core API | Gráfica Ltda**

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-blue.svg)](https://postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12+-orange.svg)](https://rabbitmq.com/)
[![AWS S3](https://img.shields.io/badge/AWS-S3-yellow.svg)](https://aws.amazon.com/s3/)

---

## 🎯 Visão Geral

A **API Central** é o núcleo da solução de processamento PCL da Gráfica Ltda, fornecendo uma arquitetura robusta e escalável para gerenciamento de arquivos de mala direta.

### 🏗️ **Responsabilidades Core**

| Funcionalidade | Descrição | Stakeholders |
|----------------|-----------|--------------|
| 🔐 **Autenticação** | Gestão segura de usuários internos | Ana, Carlos, Marcos |
| 👥 **Gestão de Clientes** | CRUD completo de empresas contratantes | Operadores |
| 📁 **Upload & Processing** | Recepção e processamento de arquivos | Ana Ribeiro |
| 🔄 **Orquestração** | Integração RabbitMQ + S3 + PostgreSQL | Sistema |
| 🌐 **Portal Integration** | Endpoints para frontend Vercel | Portal Web |
| 📊 **Analytics** | Dashboards, relatórios e métricas | Gestão |

### 🎨 **Arquitetura & Princípios**

```mermaid
graph TB
    A[🌐 Web Portal] --> B[🔌 API Gateway]
    B --> C[⚙️ Application Layer]
    C --> D[🏗️ Domain Layer]
    C --> E[💾 Infrastructure]
    E --> F[(📊 PostgreSQL)]
    E --> G[📦 AWS S3]
    E --> H[📨 RabbitMQ]
```

- ✅ **Clean Architecture** / Onion Pattern
- ✅ **SOLID Principles** aplicados consistentemente  
- ✅ **Domain-Driven Design** (DDD) patterns
- ✅ **Immutability** e functional programming
- ✅ **Structured Logging** com Serilog
- ✅ **End-to-end Traceability** completa

---

## ⚙️ Stack Tecnológico

### 🛠️ **Tecnologias Principais**

| Componente | Versão | Descrição | Justificativa |
|------------|--------|-----------|---------------|
| 🔷 **.NET 8 Web API** | `8.0+` | Framework principal da API | Performance, AOT, minimal APIs |
| 🗄️ **Entity Framework Core** | `8.0+` | ORM para PostgreSQL | Code-first, migrations, LINQ |
| 🐘 **PostgreSQL (RDS)** | `15+` | Banco relacional principal | ACID, JSON, performance |
| 📦 **AWS S3 SDK** | `Latest` | Storage de arquivos | Durabilidade, versionamento |
| 🐰 **RabbitMQ** | `3.12+` | Message broker assíncrono | Reliability, DLQ, clustering |
| 🔐 **JWT** | `RS256` | Autenticação stateless | Segurança, escalabilidade |
| 📊 **Serilog** | `Latest` | Logging estruturado | Observabilidade, structured data |
| ✅ **FluentValidation** | `Latest` | Validação de entrada | Expressividade, reusabilidade |

### 🔧 **Ferramentas de Desenvolvimento**

```bash
# Core Dependencies
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL  
dotnet add package AWSSDK.S3
dotnet add package RabbitMQ.Client
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package FluentValidation
```
---

## 🏗️ Arquitetura da Solução

### 📁 **Estrutura de Pastas**

A API segue rigorosamente os princípios da **Clean Architecture** / **Onion Architecture**:

```
src/
 ├── 🎯 Application/          # Casos de uso, regras de negócio
 │   ├── Commands/            # CQRS Commands  
 │   ├── Queries/             # CQRS Queries
 │   ├── Handlers/            # MediatR Handlers
 │   ├── DTOs/                # Data Transfer Objects
 │   └── Validators/          # FluentValidation rules
 │
 ├── 🏛️ Domain/               # Core business logic
 │   ├── Entities/            # Aggregate roots
 │   ├── ValueObjects/        # Immutable values  
 │   ├── Interfaces/          # Repository contracts
 │   └── Exceptions/          # Domain exceptions
 │
 ├── 🔧 Infrastructure/       # External concerns
 │   ├── Data/                # EF Core configuration
 │   ├── Storage/             # S3 implementation
 │   ├── Messaging/           # RabbitMQ publisher
 │   └── Repositories/        # Data access layer
 │
 └── 🌐 WebApi/               # HTTP endpoints
     ├── Controllers/         # API controllers
     ├── Middleware/          # Custom middleware
     ├── Configuration/       # DI container setup
     └── Program.cs           # Application entry point
```

### 🔄 **Responsabilidades por Camada**

| Camada | Responsabilidade | Dependências | Exemplos |
|--------|------------------|--------------|----------|
| 🎯 **Application** | Orquestração de casos de uso | Domain apenas | Handlers, DTOs, Validators |
| 🏛️ **Domain** | Regras de negócio centrais | Nenhuma | Entities, Value Objects, Interfaces |
| 🔧 **Infrastructure** | Implementações técnicas | Domain, Application | EF Core, S3 Client, RabbitMQ |
| 🌐 **WebApi** | Interface HTTP/REST | Todas as camadas | Controllers, Middleware, Config |

### 🎪 **Fluxo de Dependências**

```mermaid
graph TD
    A[🌐 WebApi] --> B[🎯 Application]
    B --> C[🏛️ Domain]
    A --> D[🔧 Infrastructure]
    D --> C
    D --> B
```

**Princípio:** Camadas internas **nunca** dependem de camadas externas!

---

## 📡 API Endpoints

### 🔗 **Resumo dos Endpoints**

| Categoria | Endpoint | Método | Autenticação | Descrição |
|-----------|----------|--------|--------------|-----------|
| 🔐 **Auth** | `/auth/login` | POST | ❌ | Autenticação JWT |
| 👤 **Users** | `/usuarios/me` | GET | ✅ | Perfil do usuário |
| 🏢 **Clients** | `/clientes` | GET | ✅ | Listar clientes |
| 🏢 **Clients** | `/clientes` | POST | ✅ | Criar cliente |
| ⚙️ **Profiles** | `/processamento/perfis` | GET | ✅ | Perfis disponíveis |
| 📤 **Batches** | `/lotes/upload` | POST | ✅ | Upload de arquivo |
| 📤 **Batches** | `/lotes/{id}` | GET | ✅ | Detalhes do lote |
| 📊 **Analytics** | `/dashboards/resumo` | GET | ✅ | Métricas gerais |

### 🔐 **Autenticação**

#### `POST /auth/login`
Autentica operadores internos e retorna JWT access token.

**Request:**
```json
{
  "email": "ana.ribeiro@graficaltda.com",
  "senha": "senhaSegura123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIs...",
  "expiresIn": 3600,
  "usuario": {
    "id": "uuid",
    "nome": "Ana Ribeiro", 
    "perfil": "Operadora"
  }
}
```

### � **Gestão de Usuários**

#### `GET /usuarios/me`
Retorna informações do usuário autenticado via JWT.

**Headers:**
```
Authorization: Bearer eyJhbGciOiJSUzI1NiIs...
```

**Response:**
```json
{
  "id": "uuid",
  "nome": "Ana Ribeiro",
  "email": "ana.ribeiro@graficaltda.com",
  "perfil": "Operadora",
  "ultimoLogin": "2024-11-13T10:30:00Z"
}
```

### 🏢 **Gestão de Clientes**

#### `GET /clientes`
Lista todos os clientes cadastrados.

**Response:**
```json
{
  "clientes": [
    {
      "id": "uuid",
      "nome": "Empresa ABC Ltda",
      "cnpj": "12.345.678/0001-90",
      "criadoEm": "2024-01-15T08:00:00Z"
    }
  ],
  "total": 1
}
```

#### `POST /clientes`
Cadastra um novo cliente no sistema.

**Request:**
```json
{
  "nome": "Nova Empresa Ltda",
  "cnpj": "98.765.432/0001-10"
}
```

### 📤 **Processamento de Lotes**

#### `POST /lotes/upload`
**Endpoint crítico** - Recebe arquivos da Ana Ribeiro para processamento.

**🔄 Fluxo Interno:**
1. ✅ **Validação** de entrada e autenticação
2. 💾 **Persistência** de metadados no PostgreSQL  
3. 📦 **Upload** do arquivo para S3
4. 📨 **Publicação** da mensagem no RabbitMQ
5. 🎯 **Retorno** do ID do lote criado

**Request:**
```json
{
  "clienteId": "uuid",
  "perfilProcessamentoId": "uuid", 
  "arquivoBase64": "base64EncodedFileContent",
  "nomeArquivo": "mala_direta_001.csv"
}
```

**Response:**
```json
{
  "loteId": "uuid",
  "status": "Recebido",
  "mensagem": "Arquivo enviado para processamento com sucesso",
  "criadoEm": "2024-11-13T14:30:00Z"
}
```

#### `GET /lotes/{id}`
Consulta detalhada do status e progresso do lote.

**Response:**
```json
{
  "id": "uuid",
  "cliente": "Empresa ABC Ltda",
  "nomeArquivo": "mala_direta_001.csv", 
  "status": "Processando",
  "registrosTotal": 1500,
  "registrosProcessados": 750,
  "criadoEm": "2024-11-13T14:30:00Z",
  "logs": [
    {
      "mensagem": "Upload realizado com sucesso",
      "tipo": "Info",
      "timestamp": "2024-11-13T14:30:15Z"
    }
  ]
}
```

### 📊 **Analytics e Dashboards**

#### `GET /dashboards/resumo`
Métricas operacionais para gestão (Marcos Oliveira).

**Response:**
```json
{
  "resumo": {
    "lotesHoje": 15,
    "lotesPendentes": 3,
    "lotesProcessados": 12, 
    "lotesComErro": 0,
    "slaAtendimento": "98.5%"
  },
  "ultimosLotes": [...],
  "errosRecentes": []
}
```

---

## � Fluxo de Processamento

### 📊 **Pipeline Assíncrono**

```mermaid
sequenceDiagram
    participant Portal as 🌐 Portal Web
    participant API as 🔌 API Central  
    participant DB as 💾 PostgreSQL
    participant S3 as 📦 AWS S3
    participant MQ as 📨 RabbitMQ
    participant Worker as ⚙️ Worker
    participant Lambda as ⚡ Lambda

    Portal->>API: POST /lotes/upload
    API->>DB: Salvar metadados
    API->>S3: Upload arquivo
    API->>MQ: Publicar mensagem
    API-->>Portal: Lote ID + Status
    
    MQ->>Worker: Consumir mensagem
    Worker->>S3: Ler arquivo origem
    Worker->>Lambda: Invocar processamento
    Lambda->>S3: Salvar PCL gerado
    Worker->>DB: Atualizar status
```

### 🎯 **Etapas Detalhadas**

| Etapa | Componente | Ação | Tempo Esperado |
|-------|------------|------|----------------|
| 1️⃣ | 🔌 **API** | Recepção e validação | < 100ms |
| 2️⃣ | 💾 **PostgreSQL** | Persistência de metadados | < 50ms |
| 3️⃣ | 📦 **S3** | Upload do arquivo origem | < 2s |
| 4️⃣ | 📨 **RabbitMQ** | Publicação da mensagem | < 10ms |
| 5️⃣ | ⚙️ **Worker** | Pré-processamento | < 30s |
| 6️⃣ | ⚡ **Lambda** | Geração de PCL | < 60s |
| 7️⃣ | 🌐 **Portal** | Disponibilização | Imediato |

---

## 🔒 Segurança e Autenticação

### 🛡️ **Camadas de Segurança**

| Aspecto | Implementação | Justificativa |
|---------|---------------|---------------|
| � **Autenticação** | JWT RS256 com refresh tokens | Stateless, escalável |
| 🔑 **Autorização** | Role-based (Operador/Gestor) | Controle granular |
| 🛡️ **Hash de Senhas** | bcrypt com salt | Proteção contra rainbow tables |
| 📊 **Logging** | Serilog estruturado (sem PII) | Auditoria sem exposição |
| ⚡ **Rate Limiting** | Por usuário interno | Prevenção de abuso |
| 🔒 **HTTPS** | TLS 1.3 obrigatório | Criptografia em trânsito |

### ⚠️ **Modelo de Acesso**

> 🚨 **IMPORTANTE:** Apenas operadores internos fazem login no sistema.  
> Clientes **NÃO** têm acesso direto à plataforma.

**Usuários Autorizados:**
- 👩‍💼 **Ana Ribeiro** (Operadora) - Upload e monitoramento
- 👨‍🔧 **Carlos Mendes** (Impressão) - Download e relatórios  
- 👨‍💼 **Marcos Oliveira** (Gestor) - Dashboards e métricas

---

## 🧪 Estratégia de Testes

### 🎯 **Cobertura de Testes**

| Tipo | Escopo | Ferramentas | Cobertura Meta |
|------|--------|-------------|----------------|
| 🔬 **Unitários** | Handlers, Validators, Domain | xUnit, NSubstitute | > 90% |
| 🔗 **Integração** | Controllers, Database, S3 | WebApplicationFactory | > 80% |
| 🚀 **Performance** | Endpoint /lotes/upload | k6, NBomber | 1000 req/min |
| 🌐 **E2E** | Fluxo completo API→Worker | TestContainers | Cenários críticos |

### 📋 **Cenários de Teste**

#### ✅ **Testes Unitários**
- Validação de entrada com FluentValidation
- Logic de domínio e regras de negócio
- Mapeamento de DTOs e entidades
- Exception handling e error cases

#### 🔗 **Testes de Integração**  
- Autenticação JWT end-to-end
- CRUD operations com PostgreSQL
- Upload/download com S3
- Publicação de mensagens no RabbitMQ

#### 🚀 **Testes de Performance**
- Load testing no endpoint crítico `/lotes/upload`
- Stress testing com arquivos grandes (> 10MB)
- Concurrent users simulation
- Database connection pooling

#### 🌐 **Testes End-to-End**
- Roundtrip: Portal → API → Worker → Lambda → Portal
- Error scenarios e recovery
- Monitoring e alerting validation

---

## ✅ Critérios de Aceite

### 📋 **Checklist de Entrega**

| Critério | Status | Validação |
|----------|--------|-----------|
| 🏗️ **API .NET 8** | ✅ | Projeto criado com minimal APIs |
| 🎯 **Clean Architecture** | ✅ | 4 camadas implementadas |
| 🔌 **Endpoints REST** | ✅ | 8 endpoints funcionais |
| 📦 **Upload S3** | ✅ | Integração testada |
| 📨 **RabbitMQ** | ✅ | Publisher implementado |
| 💾 **PostgreSQL** | ✅ | EF Core + Migrations |
| 🔐 **Autenticação JWT** | ✅ | Login + middleware |
| 📊 **Logging** | ✅ | Serilog estruturado |
| 🧪 **Testes** | ✅ | > 85% cobertura |
| 📖 **Documentação** | ✅ | README completo |

### 🚀 **Próximos Passos**

1. ✅ **Setup** do projeto .NET 8
2. ✅ **Implementação** das camadas core
3. ✅ **Integração** com dependências externas
4. ✅ **Testes** unitários e integração
5. ✅ **Deploy** em ambiente Docker
6. ✅ **Monitoramento** e observabilidade

---

> 📝 **Documentação viva:** Este README será atualizado conforme a evolução do projeto, mantendo sempre a documentação sincronizada com o código implementado.
