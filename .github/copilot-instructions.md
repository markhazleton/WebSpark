# WebSpark Solution - GitHub Copilot Instructions

## Project Overview

WebSpark is a comprehensive .NET 9.0 multi-project solution designed for web application development with modular architecture, AI integration, and robust data processing capabilities. The solution includes multiple ASP.NET Core MVC applications, domain libraries, and utility projects with modern security practices, internationalization support, and advanced chart visualization features.

## Solution Structure

### Web Applications (3 projects)

- **WebSpark.Portal** - Main portal application with extensive area-based modular architecture
- **WebSpark.Web** - Secondary web application with SignalR integration
- **DataSpark.Web** - Data analytics web application with ML.NET and chart visualization

### Libraries (9 projects)

- **WebSpark.Core** - Core infrastructure with Entity Framework, internationalization, and base models
- **WebSpark.SharedKernel** - Shared domain primitives and abstractions
- **WebSpark.UserIdentity** - ASP.NET Core Identity implementation
- **WebSpark.RecipeCookbook** - Recipe management domain library
- **PromptSpark.Domain** - AI/OpenAI integration with Semantic Kernel
- **TriviaSpark.Domain** - Trivia game domain with external API integration
- **InquirySpark.Domain** - Inquiry management domain
- **OpenWeatherMapClient** - Weather service integration
- **DocSpark** - Documentation generation utility

### Test Projects (3 projects)

- **PromptSpark.Recipe.Console** - Console application for recipe testing
- **PromptSpark.SchemaTest** - Schema validation testing
- **TriviaSpark.JShow** - Trivia show implementation

## Technology Stack

### Core Technologies

- **.NET 9.0** - Latest LTS framework with nullable reference types enabled
- **ASP.NET Core MVC** - Web framework with areas and modular architecture
- **Entity Framework Core** - ORM with SQLite databases and multiple contexts
- **SignalR** - Real-time communication for chat and live updates
- **Serilog** - Structured logging with multiple sinks (console, file, debug)

### Frontend & UI

- **Bootstrap 5** - CSS framework with custom SCSS
- **jQuery** - JavaScript library for DOM manipulation
- **Font Awesome** - Icon library
- **Chart.js & ScottPlot** - Chart visualization libraries
- **WebSpark.Bootswatch** - Custom theme switching functionality

### AI & Machine Learning

- **Microsoft Semantic Kernel** - AI orchestration framework
- **OpenAI API** - GPT integration for AI-powered features
- **ML.NET** - Machine learning framework for data analysis
- **TensorFlow.NET** - Deep learning capabilities

### Data & Analytics

- **CsvHelper** - CSV file processing
- **Apache.Arrow** - Columnar data processing
- **Microsoft.Data.Analysis** - Data analysis framework
- **ScottPlot** - Scientific plotting library

### Security & Infrastructure

- **ASP.NET Core Identity** - Authentication and authorization
- **Content Security Policy (CSP)** - Advanced security headers
- **CORS** - Cross-origin resource sharing configuration
- **Memory caching** - Performance optimization

## Coding Standards & Conventions

### General Principles

- Follow C# naming conventions (PascalCase for public members, camelCase for private fields)
- Use nullable reference types consistently
- Implement proper dependency injection patterns
- Follow SOLID principles and clean architecture patterns
- Use async/await for all I/O operations

### File Organization

- Controllers: `Controllers/` with area-specific organization
- Models: `Models/` with subdirectories for different concerns
- Services: `Services/` with interface-implementation pairs
- Views: `Views/` following MVC conventions with shared layouts
- Static files: `wwwroot/` with organized CSS, JS, and asset folders

### Naming Patterns

- Controllers: `{Name}Controller` (e.g., `HomeController`, `ChartController`)
- Services: `I{Name}Service` interfaces with `{Name}Service` implementations
- Models: Descriptive names ending with `Model` for view models
- Database contexts: `{Name}Context` (e.g., `PromptSparkContext`)

### Security Guidelines

- Always validate user input
- Use parameterized queries for database operations
- Implement proper CORS policies
- Apply CSP headers with nonce-based script execution
- Use HTTPS redirects and secure cookie settings

## Project-Specific Patterns

### WebSpark.Portal Architecture

- **Areas**: Organized into logical modules (DataSpark, GitHubSpark, etc.)
- **Base Controllers**: Inherit from shared base classes with common functionality
- **Service Registration**: Comprehensive DI container setup in `Program.cs`
- **Configuration**: Hierarchical configuration with user secrets support

### DataSpark.Web Patterns

- **Chart Services**: Modular chart generation with validation and rendering services
- **CSV Processing**: Robust file upload and data processing pipeline
- **AI Integration**: OpenAI file analysis with structured prompts
- **Base Controller**: Shared functionality for CSV and chart operations

### Domain Library Patterns

- **Repository Pattern**: Interface-based data access with Entity Framework
- **Service Layer**: Business logic encapsulation with dependency injection
- **Configuration Objects**: Strongly-typed configuration with options pattern
- **External API Integration**: HttpClient with proper error handling and caching

## Database Architecture

### Entity Framework Setup

- **Multiple Contexts**: Domain-specific database contexts
- **SQLite Databases**: File-based databases in `Data/` directories
- **Migrations**: Code-first approach with automatic migration application
- **Connection Strings**: Configuration-based with development overrides

### Data Access Patterns

- Use repository pattern for complex queries
- Implement proper async methods for all database operations
- Apply appropriate indexing for performance
- Use proper transaction scoping for data consistency

## Build & Development Instructions

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension
- Node.js (for frontend asset processing)

### Build Commands

```bash
# Restore all NuGet packages
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build WebSpark.Portal

# Run with hot reload
dotnet watch run --project WebSpark.Portal
```

### Database Setup

- Databases are created automatically on first run
- Migrations are applied automatically in development
- No manual database setup required

### Configuration

- Use User Secrets for sensitive configuration (OpenAI API keys, etc.)
- Development settings in `appsettings.Development.json`
- Environment-specific configuration supported

## Testing & Validation

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test PromptSpark.SchemaTest

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Quality Checks

- Build must complete without errors (warnings are acceptable)
- All tests must pass before deployment
- Code analysis rules enforced via .editorconfig
- Security headers validation required

## Common Patterns & Examples

### Service Registration

```csharp
builder.Services.AddScoped<IChartService, ChartService>();
builder.Services.AddSingleton<IMemoryCacheManager, MemoryCacheManager>();
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
```

### Controller Pattern

```csharp
public class BaseController : Controller
{
    protected readonly ILogger _logger;
    protected readonly IWebHostEnvironment _env;

    public BaseController(ILogger logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }
}
```

### Service Interface Pattern

```csharp
public interface IChartService
{
    Task<ChartResult> GenerateChartAsync(ChartConfiguration config);
    Task<ValidationResult> ValidateConfigurationAsync(ChartConfiguration config);
}
```

### Configuration Pattern

```csharp
public class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string AssistantId { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4";
}
```

## AI Integration Guidelines

### OpenAI Service Pattern

- Use Microsoft Semantic Kernel for AI orchestration
- Implement proper prompt engineering with system and user messages
- Apply rate limiting and error handling for API calls
- Cache responses when appropriate to reduce API costs

### Machine Learning Integration

- Use ML.NET for local machine learning tasks
- Implement proper data preprocessing pipelines
- Apply appropriate model validation and evaluation metrics
- Cache model predictions for performance

## Security Requirements

### Authentication & Authorization

- Use ASP.NET Core Identity for user management
- Implement role-based authorization where needed
- Apply proper session management and timeout policies
- Use secure password policies and two-factor authentication

### Content Security Policy

- Generate nonces for inline scripts and styles
- Restrict resource loading to trusted domains
- Apply proper CORS policies for API endpoints
- Use HTTPS enforcement in production

## Internationalization

### Resource Management

- Use .resx files for string localization
- Support multiple languages (en, es, fr, de, etc.)
- Implement proper culture-specific formatting
- Apply RTL support where required

### Implementation Pattern

```csharp
services.AddLocalization(options => options.ResourcesPath = "Resources");
services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
```

## Important Notes for AI Assistant

### When Working with This Solution:

1. **Always check existing patterns** - Look for similar implementations before creating new code
2. **Follow dependency injection** - Register services properly in Program.cs
3. **Use base classes** - Inherit from existing base controllers and services
4. **Apply security headers** - Ensure CSP and security middleware are configured
5. **Handle nullable types** - Solution uses nullable reference types extensively
6. **Respect area organization** - Place code in appropriate areas and folders
7. **Use configuration patterns** - Apply options pattern for settings
8. **Implement proper logging** - Use Serilog structured logging throughout
9. **Apply async patterns** - Use async/await for all I/O operations
10. **Test thoroughly** - Ensure new code integrates with existing test infrastructure

### Common Pitfalls to Avoid:

- Don't ignore nullable reference type warnings
- Don't bypass existing security middleware
- Don't create duplicate service registrations
- Don't hardcode configuration values
- Don't skip input validation in controllers
- Don't forget to apply proper authorization attributes
- Don't ignore existing logging patterns
- Don't create circular dependencies in DI

### File Organization Rules:

- Controllers go in `Controllers/` or area-specific directories
- Services go in `Services/` with proper interface separation
- Models go in `Models/` with logical grouping
- Configuration classes use the Options pattern
- Static resources follow wwwroot organization
- Database contexts are domain-specific

### Documentation Organization:

- **Always create documentation files (.md) in `/copilot` directory** - All new documentation should be placed in the root `/copilot` folder
- **Exception**: Keep the root `README.md` file for GitHub repository description
- **Project Documentation**: Technical documentation, guides, implementation summaries, and debugging information belong in `/copilot`
- **Naming Convention**: Use descriptive names like `IMPLEMENTATION_GUIDE.md`, `DEBUGGING_CHART_ISSUES.md`, `API_INTEGRATION_NOTES.md`
- **Cross-References**: When referencing documentation in code comments, use relative paths like `../copilot/GUIDE_NAME.md`

This solution emphasizes **modularity**, **security**, **performance**, and **maintainability**. Always consider these principles when making changes or additions to the codebase.
