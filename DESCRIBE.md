📋 Общая структура
```
Ms-Product-7.sln
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   └── Product.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       ├── EntityNotFoundException.cs
│   │       └── ConflictException.cs
│   │
│   ├── Application/
│   │   ├── Shared/                    # ✅ ТОЛЬКО переиспользуемые компоненты
│   │   │   ├── DTOs/
│   │   │   │   └── ProductDto.cs
│   │   │   └── Interfaces/
│   │   │       └── IProductRepository.cs
│   │   ├── Features/                  # ✅ ВСЁ по фичам (Command + Handler + Validator)
│   │   │   ├── Products/
│   │   │   │   ├── CreateProduct/
│   │   │   │   │   ├── CreateProductCommand.cs
│   │   │   │   │   ├── CreateProductCommandValidator.cs
│   │   │   │   │   └── CreateProductHandler.cs
│   │   │   │   └── GetProductById/
│   │   │   │       ├── GetProductByIdQuery.cs
│   │   │   │       ├── GetProductByIdQueryValidator.cs
│   │   │   │       └── GetProductByIdHandler.cs
│   │   │   └── Stock/
│   │   │       ├── ReserveStock/
│   │   │       │   ├── ReserveStockCommand.cs
│   │   │       │   ├── ReserveStockCommandValidator.cs
│   │   │       │   └── ReserveStockHandler.cs
│   │   │       └── CancelReservation/
│   │   │           ├── CancelReservationCommand.cs
│   │   │           ├── CancelReservationCommandValidator.cs
│   │   │           └── CancelReservationHandler.cs
│   │   ├── DependencyInjection.cs
│   │   └── Application.csproj
│   │
│   ├── Infrastructure/
│   │   ├── Database/
│   │   │   ├── Context/                          
│   │   │   │   └── ProductDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   └── ProductConfiguration.cs
│   │   │   └── Migrations/                       
│   │   ├── Repositories/
│   │   │   └── ProductRepository.cs
│   │   └── DependencyInjection.cs
│   │
│   └── Web/
│       ├── DTOs/
│       │   ├── CreateProductRequest.cs
│       │   ├── ReserveStockRequest.cs
│       │   ├── ApiResponse.cs
│       │   └── ValidationErrorResponse.cs
│       ├── Controllers/
│       │   └── ProductsController.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── CorrelationIdMiddleware.cs
│       ├── Validators/
│       │   └── CreateProductRequestValidator.cs
│       ├── Program.cs
│       └── Web.csproj
│
└── tests/
    └── Product.IntegrationTests/
```

создание миграций
```bash
# Из корня решения Ms-Product-7.sln
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project Web --output-dir Database/Migrations
```

### Структура DbContext

1️⃣ Единый стандарт: Конфигурация сущностей через IEntityTypeConfiguration<T>
```csharp
// OrderService.Infrastructure/Database/Context/ProductDbContext.cs

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 👇 вынос конфигурации в отдельные классы
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }
}
```

2️⃣ DbContextFactory для миграций

Фабрика для создания DbContext при дизайне (миграции, EF Core CLI).
Не используется в runtime — только для dotnet ef commands.



📁 Структура тестового проекта
```
    /IntegrationTests/
    ├── Product.IntegrationTests.csproj
    ├── Fixtures/
    │   └── ProductWebApplicationFactory.cs  # Фабрика с Testcontainers
    ├── Tests/
    │   ├── Products/
    │   │   ├── CreateProductTests.cs
    │   │   ├── GetProductByIdTests.cs
    │   │   └── ReserveStockTests.cs
    │   └── Infrastructure/
    │       └── ProductRepositoryTests.cs
    ├── Mocks/
    │   └── WireMockServerFixture.cs  # Для мокирования внешних сервисов
    ├── Helpers/
    │   └── HttpClientExtensions.cs  # Утилиты для HTTP-запросов
    └── appsettings.Test.json  # Конфигурация для тестов
```


как работает 
```
┌─────────────────────────────────────────┐
│  Интеграционный тест (xUnit)            │
└─────────────┬───────────────────────────┘
              │ HttpClient
              ▼
┌─────────────────────────────────────────┐
│  WebApplicationFactory + Testcontainers │
│  • PostgreSQL в Docker                  │
│  • Подмена ConnectionString             │
│  • Авто-миграции                        │
└─────────────┬───────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│  Приложение (Clean Architecture)        │
│  Controller → MediatR → Handler         │
│                      ↑                  │
│  ValidationBehavior (FluentValidation)  │
│                      ↑                  │
│  ExceptionHandler (IExceptionHandler)   │
│                      ↑                  │
│  ProblemDetails + RFC 7807              │
└─────────────────────────────────────────┘
```


📋 Чек-лист: что решалось по пути

| Проблема                                 | Решение                                                                   | Файл                                                  |
|------------------------------------------|---------------------------------------------------------------------------|-------------------------------------------------------|
| Ambiguous invocation для PostAsJsonAsync | Явный вызов System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync | HttpClientExtensions.cs                               |
| testhost.deps.json not found             | MSBuild Target для копирования .deps.json                                 | IntegrationTests.csproj                               |
| Program недоступен из тестов             | public partial class Program                                              | Web.csproj + Program.cs                               |
| No public InvokeAsync method             | Убрал UseMiddleware<ExceptionHandler>, остался UseExceptionHandler()      | Program.cs                                            |
| Service collection is read-only          | Перенес AddProblemDetails до builder.Build()                              | Program.cs                                            |
| FluentValidation не срабатывал           | Добавил ValidationBehavior<,> и регистрацию в DI                          | Application/Shared/Pipeline/ + DependencyInjection.cs |
| Кракозябры в логах                       | Console.OutputEncoding = Encoding.UTF8                                    | ProductWebApplicationFactory.cs                       |
| ReadProblemDetailsAsync возвращал null   | Создал TestProblemDetails DTO с [JsonPropertyName]                        | TestProblemDetails.cs + HttpClientExtensions.cs       |

1️⃣ MSBuild Target для копирования .deps.json
Проблема:
WebApplicationFactory<Program> ищет файл testhost.deps.json в выходной папке тестов, но .NET создаёт файл с именем сборки: IntegrationTests.deps.json. Без ожидаемого файла тесты падают с ошибкой:
```
Can't find '...testhost.deps.json'. This file is required for functional tests to run properly.
```
Решение (в IntegrationTests/IntegrationTests.csproj):
```xml
<Target Name="CopyDepsFileForWebApplicationFactory" AfterTargets="Build">
    <Copy 
        SourceFiles="$(OutDir)IntegrationTests.deps.json" 
        DestinationFiles="$(OutDir)testhost.deps.json" 
        SkipUnchangedFiles="true" />
</Target>
```
Что это делает:

| Атрибут                   | Значение                                              |
|---------------------------|-------------------------------------------------------|
| AfterTargets="Build"      | Запускается после успешной компиляции проекта         |
| SourceFiles               | Берёт сгенерированный файл IntegrationTests.deps.json |
| DestinationFiles          | Копирует его как testhost.deps.json (ожидаемое имя)   |
| SkipUnchangedFiles="true" | Не копирует, если файл не изменился (экономит время)  |

Почему это работает:
WebApplicationFactory находит файл с ожидаемым именем → может загрузить метаданные зависимостей → успешно запускает приложение в памяти.

2️⃣ Классы одной сборки видимы для другой сборки (например для тестов):
```csharp
app.Run();

// 👇 Делаем сгенерированный класс Program публичным
public partial class Program { }
```

Как это работает:

| Шаг | Что происходит                                                            |
|-----|---------------------------------------------------------------------------|
| 1   | Компилятор видит top-level statements → генерирует internal class Program |
| 2   | Добавление public partial class Program → компилятор объединяет части     |
| 3   | Итоговый класс становится public → виден всем сборкам, включая тесты      |
| 4   | WebApplicationFactory<Program> успешно находит точку входа                |


Изменение цены в рублях на цену в копейках
Этап 1: Dual-Price в Product Service (сначала!)
├─ Шаг 1: Обновить Product entity (PriceInKopecks + метод GetPriceAsMoney) + 
├─ Шаг 2: Обновить CreateProductRequest (два поля цены) + 
├─ Шаг 3: создал валидатор (XOR логика) + (добавил регистрацию DI) +
├─ Шаг 4: Обновить ProductDto (отдавать оба поля) +
└─ Шаг 5: Обновить тесты Product Service
