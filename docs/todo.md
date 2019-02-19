Service
- Aggregate
----------------
Customers
- Customers (msg:CustomerCreated)

Sales 
- Orders, Details, Products (msg:OrderCreated, msg:ProductOrdered)

Support
- Notifications (handle:OrderCreated, OrderShipped)

Catalog
- Products (msg:ProductDeleted)
- Categories

Users
- UserAccounts (handle:CustomerCreated)

Fulfillment
- Orders, (msg:OrderShipped)
- ProductStock, ProductReservation (handle:ProductOrdered, ProductDeleted)




- Employees, 
- Shipper

Marketing


TODO:

NetCore 3.0 upgrade
 https://www.talkingdotnet.com/asp-net-core-3-0-app-with-net-core-3-preview-2-and-visual-studio-2019/

Dashboard with razor class library (operations logevents) OR razor components
  https://www.learnrazorpages.com/advanced/razor-class-library

Repositories
  Use OptionsBuilder instead of too many ctor arguments

use Codecov token from azure devop variables in YAML > $(codecov-token) 

OptionsBuilder instead of large ctors (for all non configration things like MessageBrokers/Repositories)

Inmemory Repo
  use concurrentdict instead of list https://github.com/SharpRepository/SharpRepository/blob/develop/SharpRepository.InMemoryRepository/InMemoryRepositoryBase.cs

Better Guard (+snippets)
  https://github.com/safakgur/guard/tree/master

Operations
  Dashboard for journal logevents (domainevents/commands/....)

Operations
  Logevents filtering (correlationId, since, till) API > LAna query

Commands
  CommandHandler should support Decorators (besides the Behaviors) > https://app.pluralsight.com/player?course=cqrs-in-practice&author=vladimir-khorikov&name=22d61509-b7ab-4268-96f1-258bc8a95b99&clip=6&mode=live
  create retrydecorator for commandhandler

Repositories
  Decorator setup with scrutor Decorators (services.Decorate) https://github.com/khellang/Scrutor
  Caching decorator https://github.com/thangchung/awesome-dotnet-core#caching

Documentation
  [DONE] Diagrams for ServiceDiscovery(proxy, router)
  Diagrams for Commands
  Diagrams for Messaging
  Diagrams for Scheduling

Service
  Api versioning
  Request Rate limiting https://github.com/stefanprodan/AspNetCoreRateLimit

FileStorage
  Azure Storage File Share implementation (REST + SDK) https://app.pluralsight.com/library/courses/microsoft-azure-file-storage-implementing/table-of-contents

KeyVault cache (peristent)
  improve local service startup (due to vault requests)
  https://github.com/SanderSade/Sander.KeyVaultCache
  refactor as KeyVaultClientDecorator and override GetSecretWithHttpMessagesAsync() with cache functionality
  https://github.com/MichaCo/CacheManager
  https://github.com/maldworth/CacheManager.FileCaching/tree/develop

================================================================================

azure deployment (ARM) https://markheath.net/post/arm-vs-azure-cli
                 (CLI) https://gist.github.com/pascalnaber/75412a97a0d0b059314d193c3ab37c4c

assembly builddate https://www.meziantou.net/2018/09/24/getting-the-date-of-build-of-a-net-assembly-at-runtime 
pattern cqs https://www.dotnetcurry.com/patterns-practices/1461/command-query-separation-cqs
cqrs https://github.com/OpenCQRS/OpenCQRS
crs https://github.com/gautema/CQRSlite (eventstore)
api 404s https://www.strathweb.com/2018/10/convert-null-valued-results-to-404-in-asp-net-core-mvc/
templating (razor) https://github.com/toddams/RazorLight
httpclient (polly/CorrelationIdDelegatingHandler) https://rehansaeed.com/optimally-configuring-asp-net-core-httpclientfactory/
logging: scopes https://www.initpals.com/net-core/scoped-logging-using-microsoft-logger-with-serilog-in-net-core-application/
xunit configuration https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects
validation : entity.Validate(handler)
ef sqlite + inmemory : https://www.thereformedprogrammer.net/using-in-memory-databases-for-unit-testing-ef-core-applications/
webapi test + jwt https://www.domstamand.com/testing-a-webapi-in-net-core-with-integration-tests/
webapi caching (client/server) https://github.com/aliostad/CacheCow#getting-started---client
nested app https://github.com/damianh/AspNetCoreNestedApps
embedded dashboard https://github.com/dotnetcore/CAP/tree/master/src/DotNetCore.CAP/Dashboard
vuejs + signalr dashboard https://www.dotnetcurry.com/aspnet-core/1480/aspnet-core-vuejs-signalr-app
angular with netcore 2.2 https://www.codeproject.com/Articles/1274513/Angular-7-with-NET-Core-2-2-Global-Weather-Part-1
host multiple mvc apps https://damienbod.com/2018/12/01/using-mvc-asp-net-core-apps-in-a-host-asp-net-core-app/
file repo https://github.com/selmaohneh/Repository/tree/master/Repository.FileRepository
mongo repo https://github.com/grandnode/grandnode/blob/develop/Grand.Data/MongoDBRepository.cs
properties https://github.com/schotime/NPoco/blob/master/src/NPoco/PocoExpando.cs
roslyn compiler https://github.com/grandnode/grandnode/blob/develop/Grand.Core/Roslyn/RoslynCompiler.cs
spec mapping with visitor https://fabiomarreco.github.io/blog/2018/specificationpattern-with-entityframework/
cqrs (customer changed events) https://www.pluralsight.com/courses/cqrs-in-practice
spec vs cqrs https://enterprisecraftsmanship.com/2018/11/06/cqrs-vs-specification-pattern/
tenant resolver https://stackoverflow.com/questions/41820206/c-sharp-architecture-pattern-for-tenant-specific-business-logic
operations: serilog app insight https://github.com/serilog/serilog-sinks-applicationinsights/issues/37
idea: configuration validation https://www.stevejgordon.co.uk/asp-net-core-2-2-options-validation
graphql? https://fullstackmark.com/post/17/building-a-graphql-api-with-aspnet-core-2-and-entity-framework-core
workflow https://github.com/danielgerlag/workflow-core
web exception handling https://github.com/JosephWoodward/GlobalExceptionHandlerDotNet
app service docker container (rabbitmq?) https://docs.microsoft.com/en-us/azure/app-service/containers/tutorial-custom-docker-image
exception enricher demistify https://github.com/nblumhardt/serilog-enrichers-demystify
identity provider example https://alejandroruizvarela.blogspot.com/2018/11/aspnet-core-identity-with-cosmos-db.html
correlationid https://www.stevejgordon.co.uk/asp-net-core-correlation-ids
polly logging ctx https://github.com/stevejgordon/PollyLoggingContextSample
paged console https://github.com/damianh/EasyConsoleStd
builder pattern https://code-maze.com/builder-design-pattern/ & https://code-maze.com/fluent-builder-recursive-generics/
multi tenant (tenancy) https://github.com/Finbuckle/Finbuckle.MultiTenant
role based authorization / access control http://jasonwatmore.com/post/2019/01/08/aspnet-core-22-role-based-authorization-tutorial-with-example-api
attr based decorators https://app.pluralsight.com/player?course=cqrs-in-practice&author=vladimir-khorikov&name=22d61509-b7ab-4268-96f1-258bc8a95b99&clip=5&mode=live
azure easy auth https://docs.microsoft.com/en-us/azure/app-service/overview-authentication-authorization?view=aspnetcore-2.2 & https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-auth-aad
signalr client-server streaming https://docs.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-2.2
                                core 3 https://blogs.msdn.microsoft.com/webdev/2019/01/29/aspnet-core-3-preview-2/
metrics (AppMetrics)     https://github.com/FoundatioFx/Foundatio#metrics
inter service JSON RPC http://www.jsonrpc.org/specification
                       on hold till MS stabalizes this (gRPC): https://github.com/grpc/grpc-dotnet
                       service-bus: https://github.com/Azure/azure-service-bus/tree/master/samples/DotNet/Microsoft.Azure.ServiceBus/QueuesRequestResponse
                       https://github.com/ipjohnson/EasyRpc (incl authorization, client, dotnet di, messagepack transport)
                       https://github.com/alexanderkozlenko/aspnetcore-json-rpc (simple, only http transport) + client https://github.com/alexanderkozlenko/json-rpc-client
                       https://github.com/edjCase/JsonRpc
                       https://github.com/httpjsonrpcnet/httpjsonrpcnet
                       https://www.rabbitmq.com/tutorials/tutorial-six-python.html
                       http://gigi.nullneuron.net/gigilabs/abstracting-rabbitmq-rpc-with-taskcompletionsource/
                       https://github.com/OctopusDeploy/Halibut
                       https://github.com/Cysharp/MagicOnion

code coverage https://github.com/tonerdo/coverlet + https://codecov.io/gh/vip32/Naos.Core
api problem details https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
performance test websurge https://github.com/edjCase/JsonRpc/blob/master/test/PerformanceTests/BasicTests.websurge
miniprofiler https://miniprofiler.com/dotnet/
miniprofiler + swagger ui https://stackoverflow.com/questions/49150492/wire-up-miniprofiler-to-asp-net-core-web-api-swagger
                          https://community.miniprofiler.com/t/can-i-use-mini-profiler-for-asp-net-web-api-and-have-results-still-seen-on-url/365/2
service healthcheck https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
                    https://github.com/Xabaril/BeatPulse
cosmosdb provider redo https://www.nuget.org/packages/Microsoft.Azure.Cosmos/3.0.0.1-preview

identity overview      https://developer.okta.com/blog/2018/03/23/token-authentication-aspnetcore-complete-guide
identity.openiddict    https://github.com/openiddict/openiddict-core
identity.b2c

messaging: message broker based on rabittmq (for local usage)
messaging: in memory provider https://docs.microsoft.com/en-us/dotnet/standard/io/memory-mapped-files
                              https://dotnetcodr.com/2015/11/18/messaging-through-memory-mapped-files-in-net-c/
                              (single process) https://github.com/FoundatioFx/Foundatio/blob/master/src/Foundatio/Messaging/InMemoryMessageBus.cs
messaging: redis              https://github.com/FoundatioFx/Foundatio.Redis/blob/master/src/Foundatio.Redis/Messaging/RedisMessageBus.cs

criteria:
  https://blogs.msdn.microsoft.com/mattwar/2007/07/31/linq-building-an-iqueryable-provider-part-ii/
  https://stackoverflow.com/questions/43685229/using-predicatebuilder-to-build-query-searching-across-multiple-columns-of-entit
  https://stackoverflow.com/questions/16208214/construct-lambdaexpression-for-nested-property-from-string

  criteria: /logevents?q=type=111,correlationId=eq:2b34cc25-cd06-475c-8f9c-c42791f49b46,timestamp=gte:01-01-1980,level=eq:debug,OR,level=eq:information
  pagination: &skip=XX&take=XX
  order: &order=desc:timestamp,asc:level

  ?q=type=111,correlationId=eq:2b34cc25-cd06-475c-8f9c-c42791f49b46,timestamp=gte:01-01-1980,level=eq:debug,OR,level=eq:information&skip=0&take=100&orderby=desc:timestamp,asc:level

  middleware > criteria builder (request) > criteriacontext > controller (ctor) > repository (use criteria as specificaton)

repo: decorator and di https://andrewlock.net/adding-decorated-classes-to-the-asp.net-core-di-container-using-scrutor/
repo: in memory https://github.com/zzzprojects/nmemory
repo: file based https://github.com/ttu/json-flatfile-datastore
repo: litedb repo https://github.com/mbdavid/LiteDB/wiki/Repository-Pattern
repo: ef sql logging https://wildermuth.com/2018/11/07/EntityFrameworkCore-Logging-in-ASP-NET-Core
repo: ef dynamic schema support https://weblogs.thinktecture.com/pawel/2018/06/entity-framework-core-changing-database-schema-at-runtime.html
repo: ef sqlite https://docs.microsoft.com/en-us/ef/core/get-started/netcore/new-db-sqlite
repo: persistent mongo docker container https://blog.jeremylikness.com/mongodb-on-windows-in-minutes-with-docker-3e412f076762

service: task scheduler (coravel) https://github.com/jamesmh/coravel/blob/master/Docs/Scheduler.md
service: service registry (steeltoe) https://thenewstack.io/steeltoe-modernize-net-apps-for-a-microservices-architecture/
                                     https://steeltoe.io/docs/steeltoe-discovery/

service: service discovery (consul)https://www.codeproject.com/Articles/1248381/Microservices-Service-Discovery
                                   http://michaco.net/blog/ServiceDiscoveryAndHealthChecksInAspNetCoreWithConsul
                                   https://open.microsoft.com/2018/10/04/use-case-modern-service-discovery-consul-azure-part-1/
                           docker run -p 8500:8500 consul agent -dev -ui -client=0.0.0.0 -bind=127.0.0.1  (https://stackoverflow.com/questions/41228968/accessing-consul-ui-running-in-docker-on-osx)
         service discovery reverse proxy https://auth0.com/blog/building-a-reverse-proxy-in-dot-net-core/

operations: log analytics dashboard (operations) https://blogs.technet.microsoft.com/livedevopsinjapan/2017/08/23/log-analytics-log-search-rest-api-for-c/
            https://techcommunity.microsoft.com/t5/Azure-Log-Analytics/Authenticate-with-client-credentials-Log-Analytics/td-p/104996
            https://dev.int.loganalytics.io/documentation/1-Tutorials/Direct-API
operations: log analytics rest api (repo) https://dev.loganalytics.io/reference
operations: logevent repos (1-loganalytics, 2-cosmosdb) + specifications (ForCorrelationId, Since, Till, etc...)
operations: render razor without mcv https://blogs.u2u.be/peter/post/using-razor-outside-of-mvc-for-building-custom-middleware-or-other-generation-stuff
                                     https://github.com/toddams/RazorLight
            razor with ajax https://www.thereformedprogrammer.net/asp-net-core-razor-pages-how-to-implement-ajax-requests/
            streamed json oboejs https://medium.com/@deaniusaur/how-to-stream-json-data-over-rest-with-observables-80e0571821d3
                                 http://oboejs.com/examples#extracting-objects-from-the-json-stream
operations: stream logevents https://www.tpeczek.com/2017/02/server-sent-events-sse-support-for.html
operations: signalr https://github.com/dmitry26/Serilog.Sinks.SignalR.NetCore (console client example)

scheduling: cron is due but check with optional Specification<DateTime> if the job should really run (maybe only on business days, or other datetime logic)

messaging: azure storage queue messagebroker implementation
messaging: transport alternatives https://github.com/rebus-org/RebusSamples/tree/master/PubSub

http://localhost:15672/ (rabbitmq)
https://localhost:44347/api/values (billing)
https://localhost:44377/api/values (ordering)

azure resource group: Naos
keyvault
sevicebus
app service plan

ENV-naos-billing
ENV-naos-billing-db
ENV-naos-ordering
ENV-naos-ordering-db

=== MODULES === Operations

Operations.App
\Domain (Logevents, Repo, ContainerExtension)
Operations.App.Serilog (serilog log setup + loggerfactory for .netcore ILogger)
Operations.App.Web (ApiController + html dashboard)
Operations.Infrastructure.Azure.CosmosDb
\Repositories (logevents repo)

=== MODULES === Messaging
Messaging
Messaging.Infrastructure.Azure
Messaging.Infrastructure.RabittMQ

=== MODULES === Journaling
Journaling
Journaling.Domain (journal entity, wraps entity)
Journaling.Domain.Events.Handlers (journaling event handlers)
Journaling.Messaging (imessagebus decorator: publish/subscribe/process)
Journaling.Infrastructure.Azure.CosmosDb
\Repositories (irepositopry decorator + journal for journal entity)

projects:

- Naos.Core.App (servicedescriptor, servicecontext)
- Naos.Core.App.Web (host/webservicecontextbuilder)
- Naos.Core.App.Console (host)

- Naos.Core.Common (json/hashhelper)
- Naos.Core.Common.Extensions
- Naos.Core.Common.Web
- Naos.Core.Common.Web.Extensions
- Naos.Core.Domain (model/repo/specs/services)
- Naos.Core.Infrastructure.azure.cosmosdb (repo for documents/sql)
- Naos.Core.Infrastructure.azure.sqlserver (repo for ef core)

- Naos.Core.App.Messaging (model/repo/imessagebroker/message+handler)
- Naos.Core.App.Messaging.Infrastructure.Azure.Servicebus (messagebroker)
- Naos.Core.App.Messaging.Infrastructure.RabbitMQ (messagebroker)
- Naos.Core.App.Messaging.Infrastructure.Azure.Cosmosdb

- Naos.Core.App.Queries/Commands
- Naos.Core.App.Cqs (commands+behaviors/queries)
- Naos.Core.App.Cqs.Infrastructure.Azure.Cosmosdb (command repo)

naos-shop-app

- Naos.Reference.Shop.App.Web (models/services/mvc/razor)
- Naos.Reference.Shop.Provisioning.Arm/Cli

naos-orderung-app

- Naos.Reference.Ordering.App (commands/integration)
- Naos.Reference.Ordering.App.Web (webhost + controllers)
- Naos.Reference.Ordering.App.SignalR (message handlers > hub)
- Naos.Reference.Ordering.Domain (domain/repo/specs/services)
- Naos.Reference.Ordering.Infrastructure.Azure.CosmosDb (repo)
- Naos.Reference.Ordering.Provisioning.Arm/Cli

naos-billing-app

- Naos.Reference.Billing.App (commands/integration)
- Naos.Reference.Billing.App.Web (webhost + controllers)
- Naos.Reference.Billing.Domain (domain/repo/specs/services)
- Naos.Reference.Billing.Infrastructure.Azure.SqlServer (repo)
- Naos.Reference.Billing.Provisioning.Arm/Cli
