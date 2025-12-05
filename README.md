# Reis Inventory

**Sistema de Gestão de Estoque Inteligente com IA**

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)

## ?? Sobre o Projeto

Reis Inventory é uma plataforma completa de gestão de estoque que utiliza Inteligência Artificial para otimizar operações de inventário. O sistema oferece previsões de demanda, identificação de riscos de ruptura e sugestões automáticas de compra.

### ? Principais Funcionalidades

- ?? **Previsão de Demanda** - Algoritmos de ML para prever demanda futura
- ?? **Alertas de Ruptura** - Identificação proativa de riscos de falta de estoque
- ?? **Sugestões de Compra** - Recomendações automáticas de quantidades a comprar
- ?? **Agente de IA** - Análises e insights via Ollama (LLM local)
- ?? **Observabilidade** - Monitoramento completo com OpenTelemetry e Jaeger

## ??? Stack Tecnológica

| Componente | Tecnologia |
|------------|------------|
| **Backend API** | ASP.NET Core 10.0 Web API |
| **Worker Service** | .NET 10.0 BackgroundService |
| **Banco de Dados** | PostgreSQL 16 |
| **Cache** | Redis 7 |
| **Observabilidade** | OpenTelemetry + Jaeger |
| **IA/LLM** | Ollama (mistral:7b) |
| **Containerização** | Docker Compose |

## ?? Estrutura do Repositório

```
reis-inventory/
??? SmartInventoryAI/           # Solução principal
?   ??? src/
?   ?   ??? SmartInventoryAI.Api/           # API REST
?   ?   ??? SmartInventoryAI.Domain/        # Entidades e regras de negócio
?   ?   ??? SmartInventoryAI.Infrastructure/# Repositórios e serviços externos
?   ?   ??? SmartInventoryAI.Worker/        # Serviço de background
?   ??? tests/
?   ?   ??? SmartInventoryAI.Tests/         # Testes unitários
?   ??? docker/                             # Docker Compose e Dockerfiles
??? .gitignore
??? README.md
```

## ?? Início Rápido

### Pré-requisitos

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) e Docker Compose
- (Opcional) GPU NVIDIA para melhor performance do Ollama

### Executando com Docker

```bash
# Clone o repositório
git clone https://github.com/wellissonreis/ReisInventory.git
cd ReisInventory

# Inicie a infraestrutura
cd SmartInventoryAI/docker
docker compose up -d

# Baixe o modelo de IA
docker exec -it smartinventory-ollama ollama pull mistral:7b

# Execute a API
cd ../src/SmartInventoryAI.Api
dotnet run
```

A API estará disponível em:
- ?? **Swagger UI**: http://localhost:5000
- ?? **Jaeger UI**: http://localhost:16686

## ?? Documentação

Para documentação detalhada sobre a arquitetura, endpoints e configuração, consulte o [README do SmartInventoryAI](./SmartInventoryAI/README.md).

## ?? Testes

```bash
cd SmartInventoryAI/tests/SmartInventoryAI.Tests
dotnet test
```

## ?? Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.

1. Fork o projeto
2. Crie sua branch de feature (`git checkout -b feature/NovaFeature`)
3. Commit suas mudanças (`git commit -m 'Add: nova feature'`)
4. Push para a branch (`git push origin feature/NovaFeature`)
5. Abra um Pull Request

## ?? Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## ?? Autor

**Wellisson Reis**

- GitHub: [@wellissonreis](https://github.com/wellissonreis)

---

? Se este projeto te ajudou, considere dar uma estrela!
