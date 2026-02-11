# Guia de Arquitetura - OrderManager

Documento explicativo sobre a arquitetura e decisoes tecnicas do projeto.

---

## Visao Geral

O OrderManager e um sistema de processamento de pedidos que atua como **middleware** entre dois sistemas:

- **Sistema A**: Envia pedidos para processamento
- **Sistema B**: Consome pedidos ja processados

O sistema calcula impostos dinamicamente usando **Feature Flags**, permitindo alternar entre taxa atual (30%) e taxa da reforma tributaria (20%) sem deploy.

---

## Por que Clean Architecture?

```
┌─────────────────────────────────────────┐
│              Presentation               │  ← API (Controllers)
├─────────────────────────────────────────┤
│              Application                │  ← Casos de Uso (CQRS)
├─────────────────────────────────────────┤
│              Domain                     │  ← Regras de Negocio
├─────────────────────────────────────────┤
│              Infrastructure             │  ← Detalhes Tecnicos
└─────────────────────────────────────────┘
```

**Motivos:**

1. **Independencia de Framework**: O Domain nao conhece ASP.NET, EF, etc.
2. **Testabilidade**: Cada camada pode ser testada isoladamente
3. **Manutencao**: Mudancas em uma camada nao afetam outras
4. **Substituicao**: Posso trocar o banco de dados sem mexer nas regras de negocio

---

## Camadas Explicadas

### 1. Domain (Centro da Aplicacao)

**O que e?**
O coracao da aplicacao. Contem as regras de negocio puras, sem dependencia de frameworks ou bibliotecas externas.

**O que contem?**

| Pasta | Conteudo | Responsabilidade |
|-------|----------|------------------|
| `Entities/` | Order, OrderItem | Modelos de dominio com comportamento |
| `Enums/` | OrderStatus | Estados possiveis do pedido |
| `Interfaces/` | IOrderRepository, IFeatureFlagService | Contratos (portas) |
| `Strategies/` | CurrentTaxStrategy, ReformTaxStrategy | Algoritmos de calculo |
| `Common/` | Result, Error | Objetos de valor compartilhados |

**Por que assim?**

```csharp
// Entidade com ENCAPSULAMENTO - setters privados
public class Order
{
    public Guid Id { get; private set; }           // Nao pode ser alterado de fora
    public decimal TaxAmount { get; private set; } // Somente via metodo

    public void ApplyTax(decimal amount)           // Comportamento no dominio
    {
        TaxAmount = amount;
    }
}
```

- **Setters privados**: Garante que o estado so muda via metodos controlados
- **Metodos de negocio**: `ApplyTax()`, `MarkAsProcessed()` - a logica fica na entidade
- **Factory Method**: `Order.Create()` - controla a criacao do objeto

**Perguntas de Entrevista:**

> "Por que as interfaces estao no Domain e nao na Infrastructure?"

R: Principio da Inversao de Dependencia (DIP). O Domain define O QUE precisa (interface), a Infrastructure define COMO faz (implementacao). Assim o Domain nao depende de detalhes tecnicos.

> "Por que usar Strategy Pattern para impostos?"

R: Open/Closed Principle. Se surgir uma terceira taxa (ex: 25%), crio uma nova classe `NewTaxStrategy` sem modificar as existentes. O sistema esta aberto para extensao, fechado para modificacao.

---

### 2. Application (Casos de Uso)

**O que e?**
Orquestra o fluxo da aplicacao. Recebe comandos/consultas e coordena o Domain para executar.

**O que contem?**

| Pasta | Conteudo | Responsabilidade |
|-------|----------|------------------|
| `Orders/Commands/` | CreateOrderCommand + Handler | Operacoes de escrita |
| `Orders/Queries/` | GetOrderByIdQuery + Handler | Operacoes de leitura |
| `DTOs/` | Request/Response objects | Contratos da API |
| `Validators/` | FluentValidation rules | Validacao de entrada |
| `Mappings/` | AutoMapper profiles | Conversao Entity ↔ DTO |
| `Services/` | TaxStrategyFactory | Servicos de aplicacao |

**Por que CQRS?**

```
ANTES (Service monolitico):
┌─────────────────────────────────┐
│         OrderService            │
│  - CreateOrder()                │
│  - GetById()                    │  ← Tudo junto, dificil testar
│  - GetAll()                     │
│  - GetProcessed()               │
└─────────────────────────────────┘

DEPOIS (CQRS):
┌─────────────────┐  ┌─────────────────┐
│    Commands     │  │     Queries     │
│  CreateOrder    │  │  GetOrderById   │
│                 │  │  GetAllOrders   │  ← Separados, facil testar
└─────────────────┘  └─────────────────┘
```

**Beneficios:**
1. **Single Responsibility**: Cada handler faz UMA coisa
2. **Testabilidade**: Testo um handler sem carregar os outros
3. **Escalabilidade**: Posso escalar leitura e escrita separadamente
4. **Pipeline**: Posso adicionar logging, validacao via MediatR Behaviors

**Exemplo de Handler:**

```csharp
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderResponse>>
{
    // 1. Verifica duplicidade
    // 2. Cria entidade
    // 3. Calcula imposto (via Strategy)
    // 4. Persiste
    // 5. Retorna Result (sucesso ou erro)
}
```

**Por que Result Pattern?**

```csharp
// ANTES - Exceptions para fluxos esperados (RUIM)
if (exists) throw new DuplicateOrderException();  // Exception = custo alto

// DEPOIS - Result Pattern (BOM)
if (exists) return Result.Failure(OrderErrors.Duplicate());  // Explicito, sem custo
```

- Exceptions sao para situacoes **excepcionais** (banco caiu, rede falhou)
- Pedido duplicado e um fluxo **esperado**, nao excepcional
- Result forca o chamador a tratar o erro (compile-time safety)

---

### 3. Infrastructure.Data (Persistencia)

**O que e?**
Implementacao dos repositorios. Sabe COMO persistir dados.

**O que contem?**

| Arquivo | Responsabilidade |
|---------|------------------|
| `InMemoryOrderRepository.cs` | Implementa IOrderRepository usando ConcurrentDictionary |
| `DependencyInjection.cs` | Registra servicos no container |

**Por que In-Memory?**

```csharp
public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();
    // Thread-safe para alta volumetria (150-200k pedidos/dia)
}
```

- Requisito do projeto: armazenamento em memoria
- `ConcurrentDictionary`: Thread-safe para multiplas requisicoes simultaneas
- Facilmente substituivel por EF Core + SQL Server (so trocar a implementacao)

**Por que Singleton?**

```csharp
services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
```

- In-memory precisa manter estado entre requisicoes
- Se fosse Scoped, cada request teria um dicionario novo (vazio)

---

### 4. Infrastructure.ExternalServices (Servicos Externos)

**O que e?**
Implementacao de servicos que dependem de recursos externos (configuracao, APIs, etc).

**O que contem?**

| Arquivo | Responsabilidade |
|---------|------------------|
| `FeatureFlagService.cs` | Le configuracao do appsettings.json |
| `DependencyInjection.cs` | Registra servicos no container |

**Por que separado do Data?**

```
Infrastructure/
├── Data/              ← Persistencia (banco, cache)
└── ExternalServices/  ← Integracao externa (APIs, config)
```

- **Separacao de responsabilidades**: Banco e servico externo sao coisas diferentes
- **Substituicao independente**: Posso trocar Feature Flag por LaunchDarkly sem mexer no repositorio
- **Testes**: Posso mockar separadamente

**Feature Flag - Como funciona:**

```csharp
public bool IsNewTaxCalculationEnabled()
{
    return _configuration.GetValue<bool>("FeatureFlags:UseNewTaxCalculation");
}
```

```json
// appsettings.json
{
  "FeatureFlags": {
    "UseNewTaxCalculation": false  // Muda para true = taxa 20%
  }
}
```

- Alternar comportamento SEM deploy
- Em producao, usaria servico como LaunchDarkly, Azure App Configuration

---

### 5. API (Apresentacao)

**O que e?**
Ponto de entrada da aplicacao. Recebe HTTP, converte para Commands/Queries, retorna JSON.

**O que contem?**

| Arquivo | Responsabilidade |
|---------|------------------|
| `OrdersController.cs` | Endpoints REST |
| `Program.cs` | Configuracao da aplicacao |

**Controller - O que faz:**

```csharp
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    // 1. Valida request (FluentValidation)
    // 2. Envia Command para MediatR
    // 3. Converte Result em HTTP Response
}
```

**Por que Controller magro?**

```csharp
// RUIM - Controller gordo
[HttpPost]
public async Task<IActionResult> Create(Request request)
{
    // Validacao aqui
    // Logica de negocio aqui
    // Persistencia aqui
    // 100 linhas...
}

// BOM - Controller magro
[HttpPost]
public async Task<IActionResult> Create(Request request)
{
    var result = await _mediator.Send(new CreateOrderCommand(...));
    return result.IsSuccess ? Created(...) : ToErrorResponse(result.Error);
}
```

- Controller so faz: receber request → enviar para handler → retornar response
- Logica fica no Handler (Application layer)
- Facil de testar, facil de manter

---

## Fluxo Completo de uma Requisicao

```
1. POST /api/orders
       │
       ▼
2. OrdersController
   - Valida com FluentValidation
   - Cria CreateOrderCommand
       │
       ▼
3. MediatR.Send(command)
       │
       ▼
4. CreateOrderCommandHandler
   - Verifica duplicidade (Repository)
   - Cria Order (Domain)
   - Calcula imposto (Strategy via Factory)
   - Persiste (Repository)
   - Mapeia para DTO (AutoMapper)
       │
       ▼
5. Result<OrderResponse>
       │
       ▼
6. Controller converte em HTTP 201/400/409
```

---

## Perguntas Frequentes de Entrevista

### Sobre Arquitetura

**P: Por que tantas camadas? Nao e over-engineering?**

R: Para um CRUD simples, sim. Mas para sistemas que crescem, a separacao paga dividendos:
- Posso trocar o banco sem mexer nas regras
- Posso testar regras de negocio sem subir API
- Novos desenvolvedores entendem onde cada coisa fica

**P: O Domain pode chamar o Repository diretamente?**

R: O Domain DEFINE a interface (IOrderRepository), mas NAO chama. Quem orquestra e a Application. O Domain so tem regras puras.

**P: Por que nao usou Entity Framework no Domain?**

R: EF e detalhe de infraestrutura. Se amanha eu trocar por Dapper ou MongoDB, o Domain nao muda. Isso e o principio da Persistance Ignorance.

### Sobre Patterns

**P: Qual a diferenca entre Factory e Strategy?**

R:
- **Strategy**: Diferentes algoritmos para a mesma operacao (calcular imposto)
- **Factory**: Decide QUAL strategy usar (baseado na Feature Flag)

**P: Por que Result ao inves de Exceptions?**

R: Exceptions tem custo de performance e sao para situacoes excepcionais. "Pedido duplicado" e um fluxo esperado. Result:
- E explicito (retorno tipado)
- Forca tratamento (nao esqueco de fazer try/catch)
- Melhor performance

**P: CQRS completo ou simplificado?**

R: Simplificado. CQRS completo teria bancos separados para leitura/escrita. Aqui uso o mesmo repositorio, mas com Commands e Queries separados. Beneficio: codigo organizado, facil evoluir para CQRS completo se precisar.

### Sobre SOLID

**P: Me de um exemplo de cada principio no codigo:**

| Principio | Exemplo |
|-----------|---------|
| **S**RP | Cada Handler faz uma coisa (CreateOrderHandler so cria) |
| **O**CP | Nova TaxStrategy nao modifica as existentes |
| **L**SP | CurrentTaxStrategy e ReformTaxStrategy sao intercambiaveis |
| **I**SP | IOrderRepository so tem metodos de Order, nao de outras entidades |
| **D**IP | Application depende de IOrderRepository, nao de InMemoryOrderRepository |

---

## Resumo para Memorizar

| Camada | Responsabilidade | Depende de |
|--------|------------------|------------|
| **Domain** | Regras de negocio, entidades, interfaces | Nada |
| **Application** | Casos de uso, orquestracao, DTOs | Domain |
| **Infrastructure** | Implementacao tecnica (banco, APIs) | Domain |
| **API** | Entrada HTTP, Controllers | Application + Infrastructure |

**Frase-chave**: "As dependencias sempre apontam para o centro (Domain). O Domain nao conhece ninguem."
