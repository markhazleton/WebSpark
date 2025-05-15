# WebSpark.SharedKernel

A robust, reusable .NET class library providing foundational building blocks for domain-driven design (DDD) and clean architecture projects. This package is designed to be the shared kernel for the WebSpark ecosystem, encapsulating core abstractions, value objects, and utilities that can be leveraged across multiple services and solutions.

## Features

- **BaseEntity<TId>**: Abstract base class for entities, supporting domain events, audit fields, and strong-typed IDs.
- **BaseDomainEvent**: Abstract base for domain events, compatible with MediatR for event-driven architecture.
- **SafeDictionary<TKey, TValue>**: A type-safe, null-safe dictionary wrapper with convenience methods for safe access and mutation.
- **Value Objects**: Example implementation with `PersonName` supporting equality, formatting, and initials.
- **.NET 9.0**: Modern language features, nullable reference types, and implicit usings enabled.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Installation

Add a project reference to `WebSpark.SharedKernel` in your solution:

```sh
# From your solution directory
 dotnet add reference ../WebSpark.SharedKernel/WebSpark.SharedKernel.csproj
```

Or, if published as a NuGet package:

```sh
 dotnet add package WebSpark.SharedKernel
```

### Usage Example

#### Defining an Entity

```csharp
public class User : BaseEntity<Guid>
{
    public PersonName Name { get; set; }
    // ...other properties...
}
```

#### Working with SafeDictionary

```csharp
var dict = new SafeDictionary<string, int>();
dict.SetValue("apples", 5);
int? apples = dict.GetValue("apples");
```

#### Domain Events

```csharp
public class UserCreatedEvent : BaseDomainEvent
{
    public User User { get; }
    public UserCreatedEvent(User user) => User = user;
}
```

## Project Structure

- `BaseEntity.cs` — Base class for all entities.
- `BaseDomainEvent.cs` — Base class for domain events.
- `SafeDictionary.cs` — Safe, generic dictionary implementation.
- `ValueObjects/PersonName.cs` — Example value object for person names.

## Dependencies

- [MediatR](https://github.com/jbogard/MediatR) — In-process messaging for .NET
- [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Microsoft.Extensions.Hosting](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/api/system.text.json)

## Best Practices

- Use value objects to encapsulate domain concepts and ensure immutability.
- Leverage domain events for decoupled, event-driven business logic.
- Use SafeDictionary for scenarios requiring robust, null-safe key/value storage.
- Follow DDD and clean architecture principles for maintainable, scalable solutions.

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements or new features. Ensure code is well-documented and covered by tests.

## License

This project is licensed under the MIT License. See [LICENSE](../LICENSE) for details.

## Maintainers

- Mark Hazleton

---

> Built with ❤️ following .NET and Azure best practices.
