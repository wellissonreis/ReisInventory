# SmartInventoryAI

**Plataforma de estoque inteligente com previsões e agente de IA usando Ollama**

## 📋 Descrição

SmartInventoryAI é uma solução SaaS para gestão inteligente de estoque que utiliza machine learning e IA generativa para:

- 📊 Prever demanda futura de produtos
- ⚠️ Identificar riscos de ruptura de estoque
- 🛒 Sugerir quantidades de compra automaticamente
- 🤖 Fornecer análises e recomendações via agente de IA (Ollama)

## 🛠️ Tecnologias

| Componente | Tecnologia |
|------------|------------|
| Backend API | ASP.NET Core 9.0 Web API |
| Worker | .NET 9.0 BackgroundService |
| Banco de Dados | PostgreSQL 16 |
| Cache | Redis 7 |
| Observabilidade | OpenTelemetry + Jaeger |
| IA | Ollama (mistral:7b) |
| Containerização | Docker Compose |

## 📁 Estrutura do Projeto

```
SmartInventoryAI/
├── docker/
│   ├── docker-compose.yml      # Orquestração de containers
│   ├── Dockerfile.api          # Dockerfile da API
│   └── Dockerfile.worker       # Dockerfile do Worker
├── src/
│   ├── SmartInventoryAI.Api/           # ASP.NET Core Web API
│   │   ├── Controllers/                # Endpoints REST
│   │   ├── DTOs/                       # Data Transfer Objects
│   │   └── Program.cs
│   ├── SmartInventoryAI.Worker/        # Background Service
│   │   ├── Configuration/
│   │   ├── ForecastWorker.cs
│   │   └── Program.cs
│   ├── SmartInventoryAI.Domain/        # Domínio (DDD)
│   │   ├── Entities/                   # Entidades de domínio
│   │   ├── Interfaces/                 # Interfaces de repositório
│   │   └── Services/                   # Serviços de domínio
│   └── SmartInventoryAI.Infrastructure/  # Infraestrutura
│       ├── Data/                       # DbContext e configurações
│       ├── Repositories/               # Implementações de repositório
│       ├── ExternalServices/           # Clientes externos (Ollama)
│       ├── Observability/              # OpenTelemetry
│       └── Caching/                    # Redis
└── tests/
    └── SmartInventoryAI.Tests/         # Testes unitários
        └── Domain/
```

## 🚀 Como Executar

### Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) e Docker Compose
- (Opcional) GPU NVIDIA para melhor performance do Ollama

### 1. Iniciar a Infraestrutura

```bash
cd docker
docker compose up -d
```

Isso iniciará:
- PostgreSQL na porta `5432`
- Redis na porta `6379`
- Jaeger UI na porta `16686`
- Ollama na porta `11434`

### 2. Baixar o Modelo de IA

```bash
docker exec -it smartinventory-ollama ollama pull mistral:7b
```

> ⏳ O download pode levar alguns minutos dependendo da sua conexão.

### 3. Aplicar Migrações do Banco

```bash
cd src/SmartInventoryAI.Api
dotnet ef database update
```

### 4. Executar a API

```bash
cd src/SmartInventoryAI.Api
dotnet run
```

A API estará disponível em:
- http://localhost:5000 (HTTP)
- https://localhost:5001 (HTTPS)
- Swagger UI: http://localhost:5000

### 5. Executar o Worker

Em outro terminal:

```bash
cd src/SmartInventoryAI.Worker
dotnet run
```

## 📡 Endpoints da API

### Produtos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/products` | Lista todos os produtos |
| GET | `/api/products/{id}` | Obtém um produto pelo ID |
| GET | `/api/products/sku/{sku}` | Obtém um produto pelo SKU |
| GET | `/api/products/low-stock` | Lista produtos com estoque baixo |
| POST | `/api/products` | Cria um novo produto |
| PUT | `/api/products/{id}` | Atualiza um produto |
| DELETE | `/api/products/{id}` | Remove um produto |

### Movimentação de Estoque

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/stock/history/{productId}` | Histórico de movimentação |
| GET | `/api/stock/history/{productId}/recent?days=30` | Histórico recente |
| POST | `/api/stock/history` | Registra movimentação |

### Previsões

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/forecast/{productId}` | Previsões de um produto |
| GET | `/api/forecast/high-risk` | Previsões de alto risco |

### Sugestões de IA

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/ai/suggestions/{productId}` | Sugestão da IA em tempo real |
| GET | `/api/ai/suggestions/{productId}/latest` | Última sugestão salva |
| GET | `/api/ai/health` | Status do serviço de IA |

## 📊 Observabilidade

### Jaeger (Tracing)

Acesse http://localhost:16686 para visualizar:
- Traces de requisições HTTP
- Traces de chamadas ao banco de dados
- Traces de chamadas ao Ollama
- Métricas de performance

### Métricas

OpenTelemetry exporta métricas de:
- Duração de requisições HTTP
- Operações de banco de dados
- Ciclos do Worker de previsão

## ⚙️ Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=smartinventory;Username=postgres;Password=postgres123"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "SmartInventoryAI_"
  },
  "Jaeger": {
    "Endpoint": "http://localhost:4317"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "ModelName": "mistral:7b",
    "TimeoutSeconds": 120
  },
  "ForecastWorker": {
    "IntervalMinutes": 5,
    "ForecastDays": 7,
    "HistoryDays": 30,
    "GeneratePurchaseSuggestions": true
  }
}
```

## 🧪 Testes

```bash
cd tests/SmartInventoryAI.Tests
dotnet test
```

Os testes cobrem:
- Entidades de domínio (Product, Forecast)
- Serviço de previsão (ForecastService)
- Serviço de sugestão de compra (PurchaseSuggestionService)

## 🗺️ Próximos Passos

- [ ] Frontend Angular/React
- [ ] Integração com marketplaces (Mercado Livre, Amazon, Shopee)
- [ ] Autenticação e autorização (multi-tenant)
- [ ] Dashboard de analytics
- [ ] Alertas por email/SMS
- [ ] Modelos de ML mais avançados (LSTM, Prophet)
- [ ] API para integração com ERPs
- [ ] Kubernetes deployment

## 📝 Licença

Este projeto está sob a licença MIT.

## 👥 Contribuição

Contribuições são bem-vindas! Por favor, abra uma issue ou pull request.

---

Desenvolvido com ❤️ usando .NET 9.0 e Clean Architecture
