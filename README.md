[![Build Status](https://vip32.visualstudio.com/Naos/_apis/build/status/vip32.Naos?branchName=master)](https://vip32.visualstudio.com/Naos/_build/latest?definitionId=4&branchName=master)
[![CodeFactor](https://www.codefactor.io/repository/github/vip32/Naos/badge)](https://www.codefactor.io/repository/github/vip32/Naos)
[![codecov](https://codecov.io/gh/vip32/Naos/branch/master/graph/badge.svg)](https://codecov.io/gh/vip32/Naos)
[![GitHub issues](https://img.shields.io/github/issues/vip32/Naos.svg)](https://github.com/vip32/Naos/issues)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/vip32/Naos/master/LICENSE)

![logo](docs/logo5.png)

<p align="center"><h1>A mildly opiniated modern cloud service architecture blueprint & reference implementation</h1></p>

# Dev stack

C#, .Net Core 2.x, EnsureThat, Serilog, Mediator, FluentValidation, AutoMapper, xUnit, Shouldly, NSubstitute

## Architectural concepts

- architectural style: hexagonal/onion
- [Domain Drive Design](https://martinfowler.com/tags/domain%20driven%20design.html)
- pattern: Domain Events
- pattern: Domain Entity
- pattern: Domain Value Object
- pattern: Repository (inmemory, cosmosdb, entityframework)
- pattern: Decorators
- pattern: Specifications
- pattern: Query Object (Criteria)
- pattern: Layer Supertype (Entity)
- pattern: Commands
- pattern: Queries
- pattern: Messaging (servicebus, signalr, filesystem, rabbitmq)

#### Service API

- Style:
  - Pragmatic REST: resources, http verbs, crud + actions/commands
  - RPC: commands?
- Patterns:

  - Stateless: clients maintain state [Controller]
  - Facade: the API acts as a facade, domain logic exists beyond it [Controller]
  - Proxy: wrapper for external entities (+validation, +transformation) [ServiceDiscoveryClient]

#### Messaging

- Patterns:
  - Competing workers,
  - Fanout

# Getting started

## Sample

[^](SAMPLE.md)

## Secrets Setup

Setup key vault:
- Create a key vault [^](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-get-started) [=RESOURCE_NAME]
- Register an application with Azure Active Directory [^](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-get-started) [=AAD_APPLICATION_ID]
- Create a client secret for the application (Certificates & secrets) [=AAD_APPLICATION_KEY]

Add key vault access policy:
- `Set-AzureRmKeyVaultAccessPolicy -VaultName '[RESOURCE_NAME]' -ServicePrincipalName [AAD_APPLICATION_ID] -PermissionsToKeys decrypt,sign,get,unwrapKey,list -PermissionsToSecrets get,list -PermissionsToCertificates get,list`
- `az keyvault set-policy --name [RESOURCE_NAME] --certificate-permissions get list --key-permissions get list decrypt unwrapKey verify --object-id [AAD_APPLICATION_ID] --secret-permissions get list`

or

- Open createad Key Vault in Azure Portal
- Select Access Policies
- Click the [+ Add new] button at the top of the blade
- Click Select Principal to select the created Azure Active Directory Application
- From the Key permissions drop down, select "Decrypt", "Sign", "Get", "UnwrapKey" permissions
- Save changes

##### Local setup

- (1) Either Store application clientId (AAD_APPLICATION_ID) and clientSecret (AAD_APPLICATION_KEY) in environment variables
  - `naos__secrets__vault__name=[RESOURCE_NAME]`
  - `naos__secrets__vault__clientId=[AAD_APPLICATION_ID]`
  - `naos__secrets__vault__clientSecret=[AAD_APPLICATION_KEY]`
- (2) Or store application clientId (AAD_APPLICATION_ID) and clientSecret (AAD_APPLICATION_KEY) in an user secrets files
- (3) Or store the application clientId (AAD_APPLICATION_ID) and clientSecret (AAD_APPLICATION_KEY) in the appsettings file
- Authorize the application to use the key or secret [^](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-get-started)

optionally use a multitude of the usual [netcore configuration providers](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1#providers) to store the settings

(2) When using the [usersecrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) configuration provider use the following json file:

```
{
  "naos:secrets:vault:name": "[RESOURCE_NAME]",
  "naos:secrets:vault:clientId": "[AAD_APPLICATION_ID]",
  "naos:secrets:vault:clientSecret": "[AAD_APPLICATION_KEY]",
}
```

Place the file in the following location.
`C:\Users\[USERNAME]\AppData\Roaming\Microsoft\UserSecrets\[SECRETSID]\secrets.json`
The [SECRETSID] should be a guid and must be configured in the appsettings file.

```
{
  "naos": {
    "secrets": {
      "userSecretsId": "[SECRETSID]"
    }
  }
}
```

(3) When using the [file](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1#file-configuration-provider) configuration provider:

```
{
  "naos": {
    "secrets": {
      "vault": {
        "name": "[RESOURCE_NAME]",
        "clientId": "[AAD_APPLICATION_ID]",
        "clientSecret": "[AAD_APPLICATION_KEY]"
      }
    }
  }
}
```

##### Azure Setup

- Store application clientId (ApplicationId) and clientSecret (ApplicationKey) in the App Service application settings (Environment):
  - `naos__secrets__vault__name=[VAULT_NAME]`
  - `naos__secrets__vault__clientId=[AAD_APPLICATION_ID]`
  - `naos__secrets__vault__clientSecret=[AAD_APPLICATION_KEY]`

## Service Setup

#### Dependencies

- nugets ?? (meta packages)

#### Configuration

- TODO (appsettings)
- InProcess [*](https://weblog.west-wind.com/posts/2019/Mar/16/ASPNET-Core-Hosting-on-IIS-with-ASPNET-Core-22#outofprocess-or-inprocess-use-inprocess)

#### Bootstrapping

- Program.cs

```
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    NaosConfigurationFactory.Extend(config);
                })
                .UseStartup<Startup>()
                .UseSerilog();
```

- Startup.cs

```
TODO
```

# Service Features Overview

![overview0](docs/Naos%20–%20Architecture%20Overview.png)

# Service External View

A service is a standalone, independently deployable software component that implements useful functionality (seperated by [Bounded Context](https://martinfowler.com/bliki/BoundedContext.html)).
The external view of the service looks like this:
![overview](docs/Naos%20–%20Service%20External%20View.png)
The service has an API (inbound) thats provides its consumers access to its functionality. This API can consist API of the normal REST interface, or even RPC style if needed.
Besides the usual API Commands and Queries can be used. A command performs actions and updates data. A query retrieves data.

<p>Also the service itself has some ways to interact with other services (outbound).
By using a ServiceClient (wiht ServiceDiscovery) other services can be consumed (Rest/Rpc)
A service can subscribe to specific messages, but can also publish messages byitself.
</p>

A service's (inboud) API encapsulates its internal implementation. The service cannot be used in any other way, this API enforces the
application's modularity.

# Microservice

### Benefits

- Independent development: Developers can work on different microservices at the same time and choose the best technologies for the problem they are solving.
- Independent release and deployment: Services can be updated individually on their own schedule.
- Granular scaling: Individual services can scale independently, reducing the overall cost and increasing reliability.
- Simplicity: Smaller services are easier to understand which expedites development, testing, debugging, and launching a product.
- Fault isolation: Failure of a service does not have to translate into failure of other services.

### Design Principles

- High cohesion
- Autonomous
- Domain centric
- Resiliency
- Observable
- Automation

# Layers & Dependencies

```
  .-------------.
  | Application | - Commands + Handlers 
  | Layer       | - Messages + Handlers
  |             | - REST Controllers
  "-------------" 
     |     | 
     |     |      .-------------.
     |     "--- > |    Domain   | - Models
     |            |    Layer    | - Repositories
     |            |             |
     |            "-------------"
     |                 ^
     V                 |
   .----------------.  |
   | Infrastructure |--"
   | Layer          |
   |                | - Repository Implementierungen
   "----------------" 
```

# Service Integration
```
    Service A                                  Service B
  .-----------------.                             .------------------.
  | .-------------. |                             | .-------------.  |
  | | Application |-|---------------------------->|>| Application |  |
  | "-------------" |         - HTTP              | "-------------"  |
  |                 |         - Messaging         |                  |
  |  /""""""""\     |         - Queueing          |  /""""""""\      |
  | / Domain   \    |                             | / Domain   \     |
  | \  Model   /    |                             | \  Model   /     | 
  |  \--------/     |                             |  \--------/      |
  |                 |                             |                  |
  | .-----------.   |                             | .-----------.    |
  | | Storage   |   |                             | | Storage   |    |
  | "-----------"   |                             | "-----------"    |
  "-----------------"                             "------------------"
        TEAM                                              TEAM

```

# Configuration

- Startup
- KeyVault (+local cache)

# Commands

- Command/Query
- Command handlers
- Behaviours
  - (Decorators)

Startup.cs configuration:
- ConfigureServices(...)
```
.AddNaos(this.Configuration, "Product", "Capability", new[] { "All" }, n => n
    ...
    .AddCommands(o => o
        .AddBehavior<ValidateCommandBehavior>()
        .AddBehavior<JournalCommandBehavior>()
        .AddBehavior(new FileStoragePersistCommandBehavior(
            new FolderFileStorage(o => o.Folder(...)))))
```
- Configure(...)
```
.UseNaos(s => s
    ...
    .UseRequestCommands()
    ...);
```
#### Sending Commands
Use the mediator (IMediator) and send the command. A registered handler will handle the command and provide a response.
```
var response = await mediator.Send(command).AnyContext();
```

#### Request Command Dispatcher
Provides HTTP access to specific commands, these commands need to be registered. The commands are available on endpoints as specified in the AddCommands configuration.
All configured commands are also available in the swagger documentation (with request/response schemas).
```

                   CommandRequest
 H               .----------------.                                                           CommandHandler
 T               | -Id            |     RequestCommandDispatcher                             .--------------.
 T-------------> .----------------.     Middleware                 Mediator             .--> | Handle()     |
 P   (request)   | -CorrelationId |--->.------------.              .------------.      /     `--------------`
                 `----------------`    | Invoke()   |------------->| Send()     |-----`             |
  <------------------------------------|            |<-------------|            |<----.             V
     (response)                        `------------`              `------------`      \      CommandResponse
                                     (match method/route)                               \    .--------------.
                                                                                         `---| -Result      |
                                                                                             | -Cancelled   |
                                                                                             `--------------`
                                                                                                 (result)
```

Startup.cs configuration:
- ConfigureServices(...)
```
.AddNaos(this.Configuration, "Product", "Capability", new[] { "All" }, n => n
    ...
    .AddCommands(o => o
        ...
        .AddRequestDispatcher(o => o
            .Post<CreateCustomerCommand>("/api/commands/customers/create", 201)
            .Get<GetActiveCustomersQuery, IEnumerable<Customer>>("/api/commands/customers/active", 200)))
```


# Messaging

- Message
- Message handlers
- Message brokers
  - Azure Eventbus
  - SignalR
  - Local filesystem
- Publish DomainEvent as Message (DomainEventMessagingPublisher)
```
 .----.                                                                       .
 | a  |    .----.                              .----------.                  /
 "----"    | c  |             .--------.       | Domain   |                 /
      |    "----"   x-------> | Domain |       | Event    |                /
    .----.   /                | Event  x-----> | Message  |            .----------.
    | b  |--"                 "--------"       | PUBLISHER|            | Message  |
    "----"                                     |----------|            | Broker   |
  Domain Model                                 |-Map()    |            |----------|                  External Service
                                               |-Handle() x----------> |-Publish()|             .-----------------------.
                                               |          |            "----x-----"            / .----. Domain Model   /
                                               "----------"          /      |  +message       /  | x  |    .----.     /
                                                                    /       "--------------> /   "----"    | y  |    /
    Internal Service (origin                                       /              subscribed/         |    "----"   /
 -----------------------------------------------------------------"                        /        .----.   /     /
                                                                                          /         |  z |--"     /
                                                                                         /          "----"       /
                                                                                        "-----------------------"
```

### Messaging setup (Azure ServiceBus)

**NOTE**:
Naos:Messaging needs to administer the Azure ServiceBus by using the [Azure Resource Manager](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-overview) API. To use this API some [extra setup](https://tomasherceg.com/blog/post/azure-servicebus-in-net-core-managing-topics-queues-and-subscriptions-from-the-code) has to be
done.

- create a servicebus (standard pricing)
- `Get-AzureRmSubscription` (=subscriptionId/tenantId)
- `$sp = New-AzureRmADServicePrincipal -DisplayName "PRINCIPAL_NAME"`
- `Write-Host "clientid:" $sp.ApplicationId` (=clientId)
- `$pBSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($sp.Secret)`
- `$p = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($pBSTR)`
- `Write-Host "clientsecret:" $p` (=clientSecret)
- `New-AzureRmRoleAssignment -ServicePrincipalName $sp.ApplicationId -ResourceGroupName RESOURCE_GROUP -ResourceName RESOURCE_NAME -RoleDefinitionName Contributor -ResourceType Microsoft.ServiceBus/namespaces`

key vault keys: (or use the json configuration above)

- `development-naos--messaging--serviceBus--subscriptionId`
- `development-naos--messaging--serviceBus--tenantId`
- `development-naos--messaging--serviceBus--connectionString`
- `development-naos--messaging--serviceBus--resourceGroup` (=RESOURCE_GROUP)
- `development-naos--messaging--serviceBus--namespaceName` (=RESOURCE_NAME)
- `development-naos--messaging--serviceBus--clientId`
- `development-naos--messaging--serviceBus--clientSecret`

# Queueing

- Enqueue
  ```
  await this.queue.EnqueueAsync(
      new EchoQueueEventData { Message = "+++ hello from queue item +++" }).AnyContext();
  ```
- Dequeue
  ```
  var item = await queue.DequeueAsync();
  Console.WriteLine(item.Data.Message);
  ```
- Queue Processing (ProcessItemsAsync)
  - Handle by using a function
    ```
	await queue.ProcessItemsAsync(async i =>
        {
            Console.WriteLine(i.Data.Message);
            await i.CompleteAsync();
        });	
    ```
  - Handle by sending events which a single handler can pick up (Mediatr)
    ```
	await this.queue.ProcessItemsAsync(true).AnyContext();
	```
	will be handled by 'EchoQueueEventHandler' (EchoQueueEventData)

- Implementations
  - InMemory Queue
  - Azure Queue Storage
  - Azure ServiceBus
- Decorators

Cross Service usage:
![inter service](docs/Naos%20–%20Queueing.png)
```
        .-------.                       .-------.
       /         \                     /         \
      /  Service  \                   /  Service  \
      \  A    .-----------.      .-----------. B  / 
       \......| Queueing  |      | Queueing  |.../ 
              | <enqueue> |      | <dequeue> | - EchoQueueEventHandler
              "---x-------"      "-------x---"
 + EchoQueueEventData                    |
                  |      .-----------.   |
                  `----->| Queueing  |<--`
                         | <enqueue> |
                         "-----------"
```

Inside Service usage:
![inside service](docs/Naos%20–%20Queueing2.png)
```
         + EchoQueueEventData
              .-----------.
              | Queueing  x-----------------.
        .-----| <enqueue> |                 |
       /      "-----------"            .----V--------.
      /           \                    | Queue       |
      |  Service  |                    | (storage/   |
      |  A        |                    |  servicebus)|
      \       .-----------.            "----^--------"
       \......| Queueing  |                 |
              | <dequeue> |-----------------"      
              "-----------"      
         - EchoQueueEventHandler
```

# Domain

- Model

  - [Aggregate](https://martinfowler.com/bliki/DDD_Aggregate.html) [^](https://deviq.com/aggregate-pattern/)
    <p>An aggregate is a collection of one or more related entities (and possibly value objects). Each aggregate has a single root entity, referred to as the aggregate root. The aggregate root is responsible for controlling access to all of the members of its aggregate. It’s perfectly acceptable to single-entity aggregates, in which case that entity is itself the root of its aggregate. In addition to controlling access, the aggregate root is also responsible for ensuring the consistency of the aggregate.</p>
    <p>When applying the aggregate pattern, it is also important that persistence operations apply only at the aggregate root level. Thus, the aggregate root must be an entity, not a value object, so that it can be persisted to and from a data store using its ID</P
  - [Domain Entity](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/microservice-domain-model) [^](https://deviq.com/entity/)

    <p>An Entity is an object that has some intrinsic identity, apart from the rest of its state. Even if its properties are the same as another instance of the same type, it remains distinct because of its unique identity.</p>

    ![domainentity](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/media/image9.png)

  - [ValueObject](https://martinfowler.com/bliki/ValueObject.html) [^](https://deviq.com/value-object/)
    <p>A Value Object is an immutable type that is distinguishable only by the state of its properties. That is, unlike an Entity, which has a unique identifier and remains distinct even if its properties are otherwise identical, two Value Objects with the exact same properties can be considered equal.</p>
  - Specification [^](https://deviq.com/specification-pattern/)
    <p>A solution to the problem of where to place querying logic is to use a Specification. The Specification design pattern describes a query in an object.
    Repository methods (FindAll) will accept an ISpecification and will be able to produce the expected result given the specification.
    </p>

- Domain Events
  - Handlers

# Domain Repository

<p>
Provides an abstraction of data, so that your service can work with a simple abstraction that has an interface approximating that of a collection.
Adding, removing, updating, and selecting items from this collection is done through a series of straightforward methods (FindAll, FindOne, Upsert),
without the need to deal with database/storage concerns like connections, commands, cursors, or readers. Using this pattern can help achieve loose coupling and
can keep domain objects persistence ignorant.
</p>

[^](https://deviq.com/repository-pattern/)

- IReadonlyRepository<T>
- IRepository<T>
- Specification<T>
- IFindOptions<T>
- Repositories
  - InMemory
  - CosmosDb (Document)
  - EntityFramework (Sql, Sqlite)
- Decorators
```
 .-----------.
 | Decorator |
 `-----------`        .------------.
       `------------> | decoratee  |
         (forward)    `------------`
``` 

# Domain Specifications

- Specification<T>
- (CriteriaSpecification(IENumerable<Crits>))

# ServiceContext

- Product
- Capability

# RequestCorrelation

# RequestFiltering

- Criteria
- Order
- Skip/Take

some example filtered requests:

```
GET https://localhost:5001/api/countries?q=name=Belgium&order=name&take=1
```

# ServiceExceptions

- Middleware: ConfigureServices() + service.AddNaos().AddServiceExceptions()
- ProblemDetails [^](https://tools.ietf.org/html/rfc7807)
- ResponseHandlers (based on specific exceptions)
  - BadRequestException handled by BadRequestExceptionResponseHandler with ValidationProblemDetails response
  - ValidationException handled by ClientFormatExceptionResponseHandler with ValidationProblemDetails response
  - ClientFormatException handled by FluentValidationExceptionResponseHandler with ValidationProblemDetails response
  - BadHttpRequestException handled by KestrelExceptionResponseHandler with ProblemDetails response
- Create your own handler by implementing Naos.Commands.Exceptions.Web.IExceptionResponseHandler

example http response when requesting an invalid criteria (or ordering) name:

```
GET https://localhost:5001/api/countries?q=nameX=Belgium&order=name&take=1
{
"errors": {},
"type": "Naos.Common.NaosClientFormatException",
"title": "A request content client format error has occurred while executing the request",
"status": 400,
"detail": "'nameX' is not a member of type 'Naos.Sample.Countries.Domain.Country'",
"instance": "7SOLEZT597QXB (SSFSG)"
}
```

# ServiceDiscovery

Service discovery is conceptually quite simple: its key component is the registry, which is a database of the network locations of the available service instances.

The service discovery mechanism updates the service registry when service instances start and stop. When a ServiceClient invokes a service, the service discovery mechanism queries the registry to obtain a list of available service instances and routes the request to one of them.

## Client-side (Client)

A service client retrieves the list of available service instances from the service registry and load balances across them.

- Registration: Services register themselves in the registry on startup and deregister on shutdown.
- ServiceClient: Uses registry to find correct and healthy endpoints for services. Selection is done based on name or tags
- Health: periodically check registered services

### Registries

- FileSystem
- Consul
- (TODO: CosmosDb/Azure Storage Table)

![overview1](docs/Naos%20–%20Service%20Discovery%20Overview%20-%20Client-side.png)

## Server-side (Router)

A service client makes a request to a router, which is responsible for service discovery and forwarding (reverse-proxy).

- Router
- Registration
- ServiceClient
- Health

### Registries

The same registries as Client-side can be used in the router.

- FileSystem
- Consul
- (TODO: CosmosDb/Azure Storage Table)

![overview2](docs/Naos%20–%20Service%20Discovery%20Overview%20-%20Server-side.png)

# Operations

### Logging
  - Debug
  - Console
  - File
    - Rolling
  - Azure DiagnosticsLogStream
  - Azure ApplicationInsights
  - Azure LogAnalytics
   
```
development-naos--operations--logging--azureLogAnalytics--apiAuthentication--clientId
development-naos--operations--logging--azureLogAnalytics--apiAuthentication--clientSecret
development-naos--operations--logging--azureLogAnalytics--apiAuthentication--tenantId
development-naos--operations--logging--azureLogAnalytics--authenticationId
development-naos--operations--logging--azureLogAnalytics--resourceGroupName
development-naos--operations--logging--azureLogAnalytics--subscriptionId
development-naos--operations--logging--azureLogAnalytics--workspaceId
development-naos--operations--logging--azureLogAnalytics--workspaceName
```

### Tracing
```
           (scoped)                         (scoped)                  (scoped)
       ┌─────────┐                  ┌─────────────┐             ┌──────────┐
       │ Tracer  │                  │ ScopeManager│             │ Mediator │
       └─────────┘  ┌──────────── * └─────────────┘             └──────────┘
            │       │ SpanBuilder │        │                          │
            │       └─────────────┘        │                          │
            │       create │               │                          │
            x------------->│               │                          │
            │      withtag │               │                          │
            x------------->│               │                          │
            │              x─┐             │                          │
            │              │ │build()      │                          │
            │              │<┘             │        ┌────── *         │
            │              | activate(span)│        │ Scope │         │
            |              x-------------->│        └───────┘         │
            │              │               x----------->│             │
            │              │               │     create │             │
            │        scope │               │<-----------x             │
            │<-----------------------------x            │             │
            │              │               │            │             │
 DISPOSE   ...            ...             ...          ...            │
   scope    │              │               │            │             │
            │              │               │<-----------x             │
            │              │               │  deactivate              │
            │              │               │                          │
            │              │               x------------------------->│
            │              │               │      publish             │─┐----> handler
            │              │               │      SpanEndedDomainEvent│ │----> handler
                                                                      │<┘
* = newly created, no shared state
```
### Journal
### Dashboard

### operations setup (Azure Log Analytics)

- logging setup: workspaceId, authenticationKey
- apiAuthentication identity: https://dev.loganalytics.io/documentation/1-Tutorials/ARM-API

# JobScheduling

- Jobs
- REST Api
- Registrations
	- Handle by an anonymous function
	```
	.Register("anonymousjob2", Cron.Minutely(), (j) => System.Diagnostics.Trace.WriteLine("+++ hello from task " + j))
	```
	- Handle by a type based function
	```
	.Register<EchoJob>("testjob1", Cron.Minutely(), (j) => j.EchoAsync("+++ hello from testjob1 +++", CancellationToken.None))
	```
	- Handle by sending events which a single handler can pick up (Mediatr)
	```
	.Register("jobrequest1", Cron.Minutely(), () => new EchoJobEventData { Message = "+++ hello from jobrequest1 +++" })
	```
	will be handled by 'EchoJobEventHandler'

- Schedules
  - Cron

# FileStorage

- Serializers (json/bson/messagepack)
- Providers:
  - Folder
  - Azure Blob Storage
  - Azure FileShare Storage
  - InMemory
  - SSH
  - Embedded (resources)

# KeyValueStorage

- FileStorage
- Azure Table Storage
- CosmosDb (Table)

# Authentication
- Basic
- ApiKey
- EasyAuth

  Provides support for filling up the request ClaimsPrincipal for an Azure webapp that has been setup to use [EasyAuth](https://docs.microsoft.com/en-us/azure/app-service/overview-authentication-authorization?WT.mc_id=easyauth-github-marouill).
  ```
  services.AddNaos(this.Configuration, "Product", "Capability", new[] { "All" }, n => n
          ...
          .AddEasyAuthentication(o => o.Provider = EasyAuthProviders.AzureActiveDirectory)
  ```
  Registers an AuthenticationHandler that will interpret the X-MS-CLIENT-PRINCIPAL-IDP and X-MS-CLIENT-PRINCIPAL HTTP headers that are sent by EasyAuth once a user is logged in.
  If no user is logged in a PolicyEvaluator will redirect to the appropriate login page.

  ```
                             .--Naos-------------Azure-----.
   R      .---Azure---.       |  Easy     |       App       |
   e ---->| EasyAuth  |-----> |  Auth     |       Service   |
   q      `-----------`       |  Handler  |     ^           |
   u            |             `-----x-----------|-----------`
   e            |                   |           |  
   s            |                   `-----------`
   t            V                      provide 
            redirect to                claimsprincipal
            auth provider (login)
  ``` 
# (Workflow)?

# Foundation
- Domain
  - Model
  - Repository
  - Specifications
- Serialization
- Web
  - Client
  - Filters
  - Streaming
  - StartupTasks
```
.-----------.    .-----------.       .----------------.             .-----------.   .-----------.
| Program   |    | WebHost   |       | StartupTask    |             | MyTask    |   | Kester    |
`-----------`    `-----------`       | ServerDecorator|             `-----------`   | Server    |
      |               |              `----------------`                 |           `-----------`
      |  RunAsync()   |                     |  [decorator]              |                |  [decoratee]
      |-------------->|=--.                 |                           |                |
      |               |=  | Build           |                           |                |
      |               |=  | App             |                           |                |
      |               |=<-"                 |                           |                |
      |               |   StartAsync()      |                           |                |
      |               |-------------------->|   StartAsync()            |                |
      |               |                     |-------------------------->|  StartAsync()  |
      |               |                     |                           |--------------->|
      |               |                     |                           |                |
```
- Mapping
- Criteria
- Console
  - Interactive
  - Commands


[![](https://codescene.io/projects/5888/status.svg) Get more details at **codescene.io**.](https://codescene.io/projects/5888/jobs/latest-successful/results)

