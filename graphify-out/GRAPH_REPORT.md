# Graph Report - .  (2026-05-13)

## Corpus Check
- Corpus is ~22,157 words - fits in a single context window. You may not need a graph.

## Summary
- 622 nodes · 734 edges · 70 communities (39 shown, 31 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 36 edges (avg confidence: 0.79)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Service Interfaces & Abstractions|Service Interfaces & Abstractions]]
- [[_COMMUNITY_Database Migrations|Database Migrations]]
- [[_COMMUNITY_Auth & Domain Models|Auth & Domain Models]]
- [[_COMMUNITY_Controller Integration Tests|Controller Integration Tests]]
- [[_COMMUNITY_Cross-Cutting Infrastructure|Cross-Cutting Infrastructure]]
- [[_COMMUNITY_Test Fixtures & DbContext|Test Fixtures & DbContext]]
- [[_COMMUNITY_Invoice Repository Tests|Invoice Repository Tests]]
- [[_COMMUNITY_Go OCR Worker|Go OCR Worker]]
- [[_COMMUNITY_HTTP Invoice Services|HTTP Invoice Services]]
- [[_COMMUNITY_InvoiceList ViewModel|InvoiceList ViewModel]]
- [[_COMMUNITY_API Controllers|API Controllers]]
- [[_COMMUNITY_Domain Models & Dates|Domain Models & Dates]]
- [[_COMMUNITY_Repository Implementation|Repository Implementation]]
- [[_COMMUNITY_Core Domain Models|Core Domain Models]]
- [[_COMMUNITY_Avalonia App Shell|Avalonia App Shell]]
- [[_COMMUNITY_Auth Token & JWT Services|Auth Token & JWT Services]]
- [[_COMMUNITY_Auth Service Login Flow|Auth Service Login Flow]]
- [[_COMMUNITY_InvoiceUpload ViewModel|InvoiceUpload ViewModel]]
- [[_COMMUNITY_Background Invoice Consumer|Background Invoice Consumer]]
- [[_COMMUNITY_MainWindow ViewModel|MainWindow ViewModel]]
- [[_COMMUNITY_User Repository|User Repository]]
- [[_COMMUNITY_User Repository Interface|User Repository Interface]]
- [[_COMMUNITY_Test Factory|Test Factory]]
- [[_COMMUNITY_Upload VM Base Classes|Upload VM Base Classes]]
- [[_COMMUNITY_Login ViewModel|Login ViewModel]]
- [[_COMMUNITY_DbContext Model Snapshot|DbContext Model Snapshot]]
- [[_COMMUNITY_View Locator|View Locator]]
- [[_COMMUNITY_AppDbContext Definition|AppDbContext Definition]]
- [[_COMMUNITY_Go Invoice Models|Go Invoice Models]]
- [[_COMMUNITY_Frontend Entry Point|Frontend Entry Point]]
- [[_COMMUNITY_Auth Token Handler|Auth Token Handler]]
- [[_COMMUNITY_DbContext Factory|DbContext Factory]]
- [[_COMMUNITY_StandardizeTimestamps Designer|StandardizeTimestamps Designer]]
- [[_COMMUNITY_NormalizeEstablishments Designer|NormalizeEstablishments Designer]]
- [[_COMMUNITY_InitialCreate Designer|InitialCreate Designer]]
- [[_COMMUNITY_AddUsersTable Designer|AddUsersTable Designer]]
- [[_COMMUNITY_MakeItemsNullable Designer|MakeItemsNullable Designer]]
- [[_COMMUNITY_AddRefreshTokens Designer|AddRefreshTokens Designer]]
- [[_COMMUNITY_IncreaseAccessKeyLength Designer|IncreaseAccessKeyLength Designer]]
- [[_COMMUNITY_DTO Models|DTO Models]]
- [[_COMMUNITY_Go Image Preprocessing|Go Image Preprocessing]]
- [[_COMMUNITY_Go ParsedInvoice Publisher|Go ParsedInvoice Publisher]]
- [[_COMMUNITY_Excel Export Service|Excel Export Service]]
- [[_COMMUNITY_NormalizeDateTypes Designer|NormalizeDateTypes Designer]]
- [[_COMMUNITY_API Program Entry Point|API Program Entry Point]]
- [[_COMMUNITY_Seed Data|Seed Data]]
- [[_COMMUNITY_Export Service Interface|Export Service Interface]]
- [[_COMMUNITY_BatchDeleteRequest Model|BatchDeleteRequest Model]]
- [[_COMMUNITY_InvoiceDto|InvoiceDto]]
- [[_COMMUNITY_UpdateInvoiceRequest|UpdateInvoiceRequest]]
- [[_COMMUNITY_InvoiceListItemDto|InvoiceListItemDto]]
- [[_COMMUNITY_ProcessInvoiceRequest|ProcessInvoiceRequest]]
- [[_COMMUNITY_ParsedItemDto|ParsedItemDto]]
- [[_COMMUNITY_User Repository Ref|User Repository Ref]]
- [[_COMMUNITY_IInvoiceConsumer Interface|IInvoiceConsumer Interface]]
- [[_COMMUNITY_PendingFile Model|PendingFile Model]]
- [[_COMMUNITY_Docker Infrastructure|Docker Infrastructure]]

## God Nodes (most connected - your core abstractions)
1. `InvoiceRepositoryTests` - 26 edges
2. `InvoiceListViewModel` - 25 edges
3. `InvoiceDetailViewModel` - 24 edges
4. `InvoicesControllerTests` - 22 edges
5. `InvoiceRepository` - 19 edges
6. `IInvoiceRepository` - 17 edges
7. `InvoiceServiceTests` - 17 edges
8. `AuthService` - 17 edges
9. `InvoiceUploadViewModel` - 13 edges
10. `IInvoiceService` - 12 edges

## Surprising Connections (you probably didn't know these)
- `Program (API Entry Point)` --references--> `PostgreSQL Database`  [EXTRACTED]
  invoices-dotnet/invoices.api/Program.cs → docker-compose.yml
- `AppDbContext` --references--> `PostgreSQL Database`  [EXTRACTED]
  invoices-dotnet/invoices.api/Data/Context/AppDbContext.cs → docker-compose.yml
- `InvoicesController` --references--> `RabbitMQ Message Broker`  [INFERRED]
  invoices-dotnet/invoices.api/Controllers/InvoicesController.cs → docker-compose.yml
- `RabbitMQ Message Broker` --references--> `InvoiceServiceTests`  [EXTRACTED]
  docker-compose.yml → invoices-dotnet/invoices.tests/Services/InvoiceServiceTests.cs
- `Window` --implements--> `MainWindow`  [INFERRED]
  invoices.front/App.axaml.cs → invoices-dotnet/invoices.front/Views/MainWindow.axaml.cs

## Communities (70 total, 31 thin omitted)

### Community 0 - "Service Interfaces & Abstractions"
Cohesion: 0.05
Nodes (16): IEstablishmentService, IInvoiceConsumer, IInvoiceRepository, IInvoiceService, EstablishmentService, InvoiceConsumer, InvoiceService, HttpEstablishmentService (+8 more)

### Community 1 - "Database Migrations"
Cohesion: 0.05
Nodes (17): Migration, InitialCreate, invoices.api.Migrations, AddUsersTable, invoices.api.Migrations, invoices.api.Migrations, StandardizeTimestamps, IncreaseAccessKeyLength (+9 more)

### Community 2 - "Auth & Domain Models"
Cohesion: 0.07
Nodes (30): AuthResult, Establishment, IAuthClient, IAuthService, IEstablishmentService, IInvoiceRepository, IInvoiceService, InvoiceDetailViewModel (+22 more)

### Community 3 - "Controller Integration Tests"
Cohesion: 0.09
Nodes (9): AuthControllerTests, InvoicesControllerTests, IAsyncLifetime, IClassFixture, IEstablishmentService, EstablishmentService, HttpEstablishmentService, JsonSerializerOptions (+1 more)

### Community 4 - "Cross-Cutting Infrastructure"
Cohesion: 0.09
Nodes (31): AppDbContext, AppDbContextFactory, AuthController, AuthControllerTests, AuthServiceTests, AuthTokenHandler (Frontend), EstablishmentsController, ExtractText (OCR) (+23 more)

### Community 5 - "Test Fixtures & DbContext"
Cohesion: 0.07
Nodes (8): AppDbContext, AuthService, IConfiguration, TestAppDbContext, InvoiceService, Mock, AuthServiceTests, InvoiceServiceTests

### Community 6 - "Invoice Repository Tests"
Cohesion: 0.12
Nodes (4): IDisposable, InvoiceRepository, InvoiceRepositoryTests, SqliteConnection

### Community 7 - "Go OCR Worker"
Cohesion: 0.13
Nodes (18): main(), PrepareForOCR(), toGrayscale(), PublishParsedInvoice(), ToRawInvoice(), PreprocessImage(), ExtractText(), cleanOCRText() (+10 more)

### Community 8 - "HTTP Invoice Services"
Cohesion: 0.09
Nodes (3): IInvoiceService, HttpInvoiceService, InvoiceService

### Community 10 - "API Controllers"
Cohesion: 0.1
Nodes (4): ControllerBase, AuthController, EstablishmentsController, InvoicesController

### Community 11 - "Domain Models & Dates"
Cohesion: 0.11
Nodes (7): DateTime, DateTimeOffset, decimal, Establishment, Invoice, ParsedItem, InvoiceDetailViewModel

### Community 12 - "Repository Implementation"
Cohesion: 0.13
Nodes (3): Dictionary, IInvoiceRepository, InvoiceRepository

### Community 13 - "Core Domain Models"
Cohesion: 0.24
Nodes (17): AuthResult, BatchDeleteRequest, Establishment, Invoice, ParsedItem, RawInvoice, RefreshToken, User (+9 more)

### Community 14 - "Avalonia App Shell"
Cohesion: 0.13
Nodes (8): LoginWindow, MainWindow, Application, App, ServiceProvider, LoginWindow, MainWindow, Window

### Community 15 - "Auth Token & JWT Services"
Cohesion: 0.24
Nodes (4): byte, CancellationTokenSource, HttpClient, AuthService

### Community 16 - "Auth Service Login Flow"
Cohesion: 0.2
Nodes (4): IAuthService, AuthService, RefreshToken, User

### Community 17 - "InvoiceUpload ViewModel"
Cohesion: 0.2
Nodes (3): int, ObservableCollection, InvoiceUploadViewModel

### Community 18 - "Background Invoice Consumer"
Cohesion: 0.31
Nodes (4): BackgroundService, IChannel, IInvoiceConsumer, InvoiceConsumer

### Community 19 - "MainWindow ViewModel"
Cohesion: 0.29
Nodes (3): IAuthClient, IServiceProvider, MainWindowViewModel

### Community 23 - "Upload VM Base Classes"
Cohesion: 0.33
Nodes (4): ObservableObject, string, PendingFile, ViewModelBase

### Community 24 - "Login ViewModel"
Cohesion: 0.33
Nodes (4): bool, IAuthService, ViewModelBase, LoginViewModel

### Community 25 - "DbContext Model Snapshot"
Cohesion: 0.4
Nodes (3): AppDbContextModelSnapshot, invoices.api.Migrations, ModelSnapshot

### Community 28 - "Go Invoice Models"
Cohesion: 0.4
Nodes (3): ParsedInvoice, ParsedItem, RawInvoice

### Community 39 - "DTO Models"
Cohesion: 0.67
Nodes (4): InvoiceDto, InvoiceListItemDto, ParsedItemDto, UpdateInvoiceRequest

### Community 40 - "Go Image Preprocessing"
Cohesion: 0.5
Nodes (4): PrepareForOCR, toGrayscale, ToRawInvoice, RawInvoice

### Community 41 - "Go ParsedInvoice Publisher"
Cohesion: 0.5
Nodes (4): PublishParsedInvoice, ParsedInvoice, ParsedItem, Validate

## Ambiguous Edges - Review These
- `Invoice` → `Migration: StandardizeTimestamps`  [AMBIGUOUS]
  invoices-dotnet/invoices.api/Migrations/20260512221838_StandardizeTimestamps.cs · relation: references

## Knowledge Gaps
- **72 isolated node(s):** `BatchDeleteRequest`, `InvoiceDto`, `UpdateInvoiceRequest`, `InvoiceListItemDto`, `ProcessInvoiceRequest` (+67 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **31 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **What is the exact relationship between `Invoice` and `Migration: StandardizeTimestamps`?**
  _Edge tagged AMBIGUOUS (relation: references) - confidence is low._
- **Why does `AuthService` connect `Auth Token & JWT Services` to `MainWindow ViewModel`, `Login ViewModel`, `Controller Integration Tests`?**
  _High betweenness centrality (0.140) - this node is a cross-community bridge._
- **Why does `JsonSerializerOptions` connect `Controller Integration Tests` to `HTTP Invoice Services`, `Test Fixtures & DbContext`, `Auth Token & JWT Services`?**
  _High betweenness centrality (0.128) - this node is a cross-community bridge._
- **Why does `IAuthClient` connect `Auth & Domain Models` to `Login ViewModel`?**
  _High betweenness centrality (0.116) - this node is a cross-community bridge._
- **What connects `BatchDeleteRequest`, `InvoiceDto`, `UpdateInvoiceRequest` to the rest of the system?**
  _72 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Service Interfaces & Abstractions` be split into smaller, more focused modules?**
  _Cohesion score 0.05 - nodes in this community are weakly interconnected._
- **Should `Database Migrations` be split into smaller, more focused modules?**
  _Cohesion score 0.05 - nodes in this community are weakly interconnected._