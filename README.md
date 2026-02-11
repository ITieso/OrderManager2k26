# OrderManager

Sistema de gerenciamento de pedidos em .NET 8 que atua como middleware entre Sistema A (origem) e Sistema B (consumidor).

## Indice

- [Arquitetura](#arquitetura)
- [Padroes Utilizados](#padroes-utilizados)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Configuracao](#configuracao)
- [Execucao](#execucao)
- [Testes](#testes)
- [Endpoints da API](#endpoints-da-api)
- [Exemplos de Uso](#exemplos-de-uso)

---

## Arquitetura

O projeto segue **Clean Architecture** com separacao clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────┐
│                         API                             │
│              (Controllers, Program.cs)                  │
└────────────────────────┬────────────────────────────────┘
                         │
         ┌───────────────┼───────────────┐
         ▼               ▼               ▼
┌─────────────┐   ┌─────────────┐   ┌─────────────┐
│ Application │   │ Infra.Data  │   │Infra.External│
│  (CQRS)     │   │(Repository) │   │(FeatureFlag)│
└──────┬──────┘   └──────┬──────┘   └──────┬──────┘
       │                 │                 │
       └────────────────►│◄────────────────┘
                         ▼
                  ┌─────────────┐
                  │   Domain    │
                  │ (Entidades) │
                  └─────────────┘
```

### Fluxo de Dependencias

- **Domain**: Sem dependencias externas (centro da arquitetura)
- **Application**: Depende apenas do Domain
- **Infrastructure**: Depende apenas do Domain
- **API**: Depende de Application e Infrastructure

---

## Padroes Utilizados

### SOLID

| Principio | Implementacao |
|-----------|---------------|
| **S**ingle Responsibility | Cada Handler tem uma unica responsabilidade |
| **O**pen/Closed | Strategies podem ser adicionadas sem modificar codigo existente |
| **L**iskov Substitution | Strategies sao intercambiaveis |
| **I**nterface Segregation | Interfaces pequenas e focadas |
| **D**ependency Inversion | Dependencias via interfaces (IOrderRepository, ITaxStrategyFactory) |

### Design Patterns

| Padrao | Uso |
|--------|-----|
| **Strategy** | Calculo de impostos (30% atual, 20% reforma) |
| **Factory** | TaxStrategyFactory seleciona strategy via Feature Flag |
| **Repository** | Abstrai persistencia de dados |
| **CQRS** | Separacao de Commands (escrita) e Queries (leitura) |
| **Result Pattern** | Tratamento de erros sem exceptions |
| **Mediator** | MediatR para desacoplamento de handlers |

### Feature Flags

Sistema de alternancia de calculo de impostos:

| Flag | Taxa | Descricao |
|------|------|-----------|
| `UseNewTaxCalculation: false` | 30% | Taxa atual (padrao) |
| `UseNewTaxCalculation: true` | 20% | Taxa reforma tributaria |

---

## Estrutura do Projeto

```
OrderManager/
├── OrderManager.sln
│
├── OrderManager.Domain/
│   ├── Common/
│   │   ├── Result.cs              # Result Pattern
│   │   └── Error.cs               # Erros tipados
│   ├── Entities/
│   │   ├── Order.cs               # Agregado raiz
│   │   └── OrderItem.cs           # Entidade filha
│   ├── Enums/
│   │   └── OrderStatus.cs         # Estados do pedido
│   ├── Interfaces/
│   │   ├── IOrderRepository.cs    # Contrato repositorio
│   │   └── IFeatureFlagService.cs # Contrato feature flag
│   └── Strategies/
│       ├── ITaxCalculationStrategy.cs
│       ├── CurrentTaxStrategy.cs  # 30%
│       └── ReformTaxStrategy.cs   # 20%
│
├── OrderManager.Application/
│   ├── DTOs/
│   │   ├── CreateOrderRequest.cs
│   │   ├── OrderItemRequest.cs
│   │   ├── OrderResponse.cs
│   │   └── OrderItemResponse.cs
│   ├── Interfaces/
│   │   └── ITaxStrategyFactory.cs
│   ├── Mappings/
│   │   └── OrderProfile.cs        # AutoMapper
│   ├── Orders/
│   │   ├── Commands/
│   │   │   └── CreateOrder/
│   │   │       ├── CreateOrderCommand.cs
│   │   │       └── CreateOrderCommandHandler.cs
│   │   └── Queries/
│   │       ├── GetOrderById/
│   │       ├── GetOrderByPedidoId/
│   │       ├── GetAllOrders/
│   │       └── GetProcessedOrders/
│   ├── Services/
│   │   └── TaxStrategyFactory.cs
│   ├── Validators/
│   │   └── CreateOrderRequestValidator.cs
│   └── DependencyInjection.cs
│
├── OrderManager.Infrastructure.Data/
│   ├── Repositories/
│   │   └── InMemoryOrderRepository.cs  # ConcurrentDictionary
│   └── DependencyInjection.cs
│
├── OrderManager.Infrastructure.ExternalServices/
│   ├── Services/
│   │   └── FeatureFlagService.cs
│   └── DependencyInjection.cs
│
├── OrderManager.API/
│   ├── Controllers/
│   │   └── OrdersController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
└── OrderManager.Tests/
    ├── Domain/
    │   ├── CurrentTaxStrategyTests.cs
    │   ├── ReformTaxStrategyTests.cs
    │   └── ResultTests.cs
    ├── Application/
    │   ├── CreateOrderCommandHandlerTests.cs
    │   ├── GetOrderByIdQueryHandlerTests.cs
    │   ├── TaxStrategyFactoryTests.cs
    │   └── OrderProfileTests.cs
    └── Integration/
        └── OrdersControllerIntegrationTests.cs
```

---

## Configuracao

### Requisitos

- .NET 8 SDK
- Visual Studio 2022 / VS Code / Rider

### appsettings.json

```json
{
  "FeatureFlags": {
    "UseNewTaxCalculation": false
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
  },
  "AllowedHosts": "*"
}
```

Para ativar a nova taxa de 20%, altere:
```json
"UseNewTaxCalculation": true
```

---

## Execucao

### Clonar e Restaurar

```bash
cd C:\Users\it\Source\Repos\OrderManager
dotnet restore
```

### Compilar

```bash
dotnet build
```

### Executar a API

```bash
dotnet run --project OrderManager.API
```

A API estara disponivel em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

### Executar com Watch (Hot Reload)

```bash
dotnet watch --project OrderManager.API
```

---

## Testes

### Executar Todos os Testes

```bash
dotnet test
```

### Executar com Detalhes

```bash
dotnet test --verbosity normal
```

### Executar Testes Especificos

```bash
# Apenas testes de Domain
dotnet test --filter "FullyQualifiedName~Domain"

# Apenas testes de Application
dotnet test --filter "FullyQualifiedName~Application"

# Apenas testes de Integracao
dotnet test --filter "FullyQualifiedName~Integration"
```

### Executar com Cobertura

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Estrutura de Testes

| Categoria | Quantidade | Descricao |
|-----------|------------|-----------|
| Domain | 9 | Strategies, Result Pattern |
| Application | 12 | Handlers, Factory, AutoMapper |
| Integration | 10 | API end-to-end |
| **Total** | **34** | |

---

## Endpoints da API

| Metodo | Endpoint | Descricao | Sucesso | Erro |
|--------|----------|-----------|---------|------|
| `POST` | `/api/orders` | Criar pedido (Sistema A) | 201 | 400, 409 |
| `GET` | `/api/orders/{id}` | Buscar por ID interno | 200 | 404 |
| `GET` | `/api/orders/pedido/{pedidoId}` | Buscar por ID externo | 200 | 404 |
| `GET` | `/api/orders` | Listar todos | 200 | - |
| `GET` | `/api/orders/processed` | Listar processados (Sistema B) | 200 | - |

### Codigos de Resposta

| Codigo | Significado |
|--------|-------------|
| 200 | OK - Requisicao bem-sucedida |
| 201 | Created - Pedido criado |
| 400 | Bad Request - Validacao falhou |
| 404 | Not Found - Pedido nao encontrado |
| 409 | Conflict - Pedido duplicado (PedidoId ja existe) |
| 500 | Internal Server Error - Erro inesperado |

---

## Exemplos de Uso

### Criar Pedido

**Request:**
```bash
curl -X POST https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "pedidoId": "PED-001",
    "items": [
      {
        "productName": "Produto A",
        "quantity": 2,
        "unitPrice": 50.00
      },
      {
        "productName": "Produto B",
        "quantity": 1,
        "unitPrice": 25.00
      }
    ]
  }'
```

**Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "pedidoId": "PED-001",
  "items": [
    {
      "id": "...",
      "productName": "Produto A",
      "quantity": 2,
      "unitPrice": 50.00,
      "totalPrice": 100.00
    },
    {
      "id": "...",
      "productName": "Produto B",
      "quantity": 1,
      "unitPrice": 25.00,
      "totalPrice": 25.00
    }
  ],
  "totalAmount": 125.00,
  "taxAmount": 37.50,
  "status": "Processed",
  "createdAt": "2026-02-09T22:30:00Z",
  "processedAt": "2026-02-09T22:30:00Z"
}
```

### Buscar por ID

```bash
curl https://localhost:5001/api/orders/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### Buscar por PedidoId

```bash
curl https://localhost:5001/api/orders/pedido/PED-001
```

### Listar Pedidos Processados (Sistema B)

```bash
curl https://localhost:5001/api/orders/processed
```

### Erro de Validacao (400)

**Request com dados invalidos:**
```json
{
  "pedidoId": "",
  "items": []
}
```

**Response:**
```json
[
  { "propertyName": "PedidoId", "errorMessage": "PedidoId is required." },
  { "propertyName": "Items", "errorMessage": "At least one item is required." }
]
```

### Erro de Duplicidade (409)

**Response ao tentar criar pedido com mesmo PedidoId:**
```json
{
  "code": "Order.Duplicate",
  "message": "Order with identifier 'PED-001' already exists."
}
```

### Erro Not Found (404)

```json
{
  "code": "Order.NotFound",
  "message": "Order with identifier 'PED-999' was not found."
}
```

---

## Tecnologias

| Tecnologia | Versao | Uso |
|------------|--------|-----|
| .NET | 8.0 | Framework |
| MediatR | 14.x | CQRS / Mediator |
| AutoMapper | 12.x | Object Mapping |
| FluentValidation | 11.x | Validacao |
| Serilog | 10.x | Logging |
| xUnit | 2.x | Testes |
| FluentAssertions | 8.x | Assertions |
| NSubstitute | 5.x | Mocking |

---

## Licenca

Projeto desenvolvido para fins de estudo e demonstracao de boas praticas.
