[![Build Status](https://dev.azure.com/doomsday32/Naos/_apis/build/status/vip32.Naos)](https://dev.azure.com/doomsday32/Naos/_build/latest?definitionId=1)
[![CodeFactor](https://www.codefactor.io/repository/github/vip32/naos.core/badge)](https://www.codefactor.io/repository/github/vip32/naos.core)
[![codecov](https://codecov.io/gh/vip32/Naos.Core/branch/master/graph/badge.svg)](https://codecov.io/gh/vip32/Naos.Core)
[![GitHub issues](https://img.shields.io/github/issues/vip32/Naos.Core.svg)](https://github.com/vip32/Naos.Core/issues)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/vip32/Naos.Core/master/LICENSE)

![logo](docs/logo.png)

<p align="center"><h1>A mildly opiniated modern cloud service architecture blueprint & reference implementation</h1></p>

# Overview

![overview0](docs/Naos%20–%20Architecture%20Overview.png)

# Configuration

- Startup
- KeyVault (+local cache)

# AppCommands

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

# Domain

- Model
  - AggregateRoot
  - Entity
  - ValueObject
- Repository
- Specifications
- Events + Handlers

* Entity

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

- Logging
- Journal
- Dashboard

# JobScheduling

- Jobs
- Schedules

# (Workflow)
