# Configuration

- Startup
- KeyVault (+local cache)

# Commands

- Command
- Command handlers
- Behaviours
  - (Decorators)

# Messaging

- Message
- Message handlers
- Message brokers
  - Azure Eventbus
  - SignalR
  - Local filesystem

# Domain Model

- Entity

# Domain Repositories

- IReadonlyRepository<T>
- IRepository<T>
- Specifications
- IFindOptions<T>
- Decorators
- Repositories
  - CosmosDb
  - SqlServer
  - InMemory

# Domain Specifications

- Specification<T>
- (CriteriaSpecification(IENumerable<Crits>))

# ServiceContext

- Product
- Capability

# ServiceExceptions

- ProblemDetails
- ExceptionResponseHandler

# RequestCorrelation

# RequestFiltering

- Criteria
- Order
- Skip/Take

# ServiceDiscovery

Service discovery is conceptually quite simple: its key component is the registry, which is a database of the network locations of the available service instances.

The service discovery mechanism updates the service registry when service instances start and stop. When a ServiceClient invokes a service, the service discovery mechanism queries the registry to obtain a list of available service instances and routes the request to one of them.

## Client-side

A service client retrieves the list of available service instances from the service registry and load balances across them.

- Registration: Services register themselves in the registry on startup and deregister on shutdown.
- ServiceClient: Uses registry to find correct and healthy endpoints for services. Selection is done based on name or tags
- Health: periodically check registered services
- Registries: FileSystem, Consul (TODO:CosmosDb/Azure Storage Table)

![overview1](docs/Naos%20-%20Service%20Discovery%20Overview%20-%20Client-side.png)

## Server-side

A service client makes a request to a router, which is responsible for service discovery and forwarding (reverse-proxy).

- Router
- Registration
- ServiceClient
- Health

![overview2](docs/Naos%20–%20Service%20Discovery%20Overview%20-%20Server-side.png)

# Operations

- Logging
- Journal
- Dashboard

# JobScheduling

- Jobs
- Schedules

# (Workflow)
