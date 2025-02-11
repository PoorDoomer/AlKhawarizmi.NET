# Clean Architecture Pattern Guide 🏗️

## Overview

Clean Architecture is a software design philosophy that separates concerns into concentric layers, promoting independence, testability, and maintainability.

## Layer Structure

```
YourProject/
├── Domain/                 # Enterprise Business Rules
│   ├── Entities/
│   ├── ValueObjects/
│   └── Interfaces/
├── Application/           # Application Business Rules
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   └── Validators/
├── Infrastructure/        # Frameworks & Drivers
│   ├── Persistence/
│   ├── ExternalServices/
│   └── Logging/
└── WebApi/               # Interface Adapters
    ├── Controllers/
    ├── Middleware/
    └── ViewModels/
```

## Key Principles

1. **Independence of Frameworks**: The architecture doesn't depend on frameworks
2. **Testability**: Business rules can be tested without UI, database, or external elements
3. **Independence of UI**: UI can change without changing the system
4. **Independence of Database**: Business rules aren't bound to the database

## Implementation Guide

### 1. Domain Layer

```csharp
public class Order
{
    public Guid Id { get; private set; }
    public Customer Customer { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    
    private readonly List<OrderItem> _items = new();
    
    public void AddItem(Product product, int quantity)
    {
        _items.Add(new OrderItem(product, quantity));
    }
}
```

### 2. Application Layer

```csharp
public class CreateOrderCommand : IRequest<OrderDto>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.CustomerId);
        // Add items, validate, etc.
        await _orderRepository.AddAsync(order);
        return new OrderDto(order);
    }
}
```

### 3. Infrastructure Layer

```csharp
public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Order> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
```

### 4. WebApi Layer

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

## Best Practices

1. **Dependency Rule**: Dependencies only point inward
2. **Interface Segregation**: Define focused interfaces
3. **Single Responsibility**: Each class has one reason to change
4. **Dependency Injection**: Use DI for loose coupling

## Common Pitfalls

1. ❌ Mixing domain logic with infrastructure concerns
2. ❌ Exposing domain entities directly through API
3. ❌ Tight coupling between layers
4. ❌ Violating the dependency rule

## Testing Strategy

1. **Domain Tests**: Pure unit tests
2. **Application Tests**: Use mocked repositories
3. **Integration Tests**: Test infrastructure
4. **API Tests**: End-to-end testing

## Recommended Tools

1. **MediatR**: For CQRS implementation
2. **FluentValidation**: For validation rules
3. **AutoMapper**: For object mapping
4. **EF Core**: For data access

## Migration Path

1. Start with domain model
2. Add application services
3. Implement infrastructure
4. Create API endpoints
5. Add cross-cutting concerns 