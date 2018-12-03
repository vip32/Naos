pattern cqs https://www.dotnetcurry.com/patterns-practices/1461/command-query-separation-cqs
cqrs https://github.com/OpenCQRS/OpenCQRS
crs https://github.com/gautema/CQRSlite (eventstore)
api 404s https://www.strathweb.com/2018/10/convert-null-valued-results-to-404-in-asp-net-core-mvc/
logging: scopes https://www.initpals.com/net-core/scoped-logging-using-microsoft-logger-with-serilog-in-net-core-application/
xunit configuration https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects
criteria > linq https://blogs.msdn.microsoft.com/mattwar/2007/07/31/linq-building-an-iqueryable-provider-part-ii/
validation : entity.Validate(handler)
ef sqlite + inmemory : https://www.thereformedprogrammer.net/using-in-memory-databases-for-unit-testing-ef-core-applications/
webapi test + jwt https://www.domstamand.com/testing-a-webapi-in-net-core-with-integration-tests/
host multiple mvc apps https://damienbod.com/2018/12/01/using-mvc-asp-net-core-apps-in-a-host-asp-net-core-app/
file repo https://github.com/selmaohneh/Repository/tree/master/Repository.FileRepository
properties https://github.com/schotime/NPoco/blob/master/src/NPoco/PocoExpando.cs
spec mapping with visitor https://fabiomarreco.github.io/blog/2018/specificationpattern-with-entityframework/
cqrs (customer changed events) https://www.pluralsight.com/courses/cqrs-in-practice
spec vs cqrs https://enterprisecraftsmanship.com/2018/11/06/cqrs-vs-specification-pattern/
tenant resolver https://stackoverflow.com/questions/41820206/c-sharp-architecture-pattern-for-tenant-specific-business-logic
operations: serilog app insight https://github.com/serilog/serilog-sinks-applicationinsights/issues/37
idea: configuration validation https://www.stevejgordon.co.uk/asp-net-core-2-2-options-validation
graphql? https://fullstackmark.com/post/17/building-a-graphql-api-with-aspnet-core-2-and-entity-framework-core
workflow https://github.com/danielgerlag/workflow-core
web exception handling https://github.com/JosephWoodward/GlobalExceptionHandlerDotNet
exception enricher demistify https://github.com/nblumhardt/serilog-enrichers-demystify
identity provider example https://alejandroruizvarela.blogspot.com/2018/11/aspnet-core-identity-with-cosmos-db.html
correlationid https://www.stevejgordon.co.uk/asp-net-core-correlation-ids
polly logging ctx https://github.com/stevejgordon/PollyLoggingContextSample
multi tenant (tenancy) https://github.com/Finbuckle/Finbuckle.MultiTenant
code coverage https://github.com/tonerdo/coverlet + https://codecov.io/gh/vip32/Naos.Core
api problem details https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
miniprofiler https://miniprofiler.com/dotnet/
miniprofiler + swagger ui https://stackoverflow.com/questions/49150492/wire-up-miniprofiler-to-asp-net-core-web-api-swagger
                          https://community.miniprofiler.com/t/can-i-use-mini-profiler-for-asp-net-web-api-and-have-results-still-seen-on-url/365/2

messaging: message broker based on rabitmq (for local usage)
messaging: singalr based provider?

repo: file based https://github.com/ttu/json-flatfile-datastore
repo: litedb repo https://github.com/mbdavid/LiteDB/wiki/Repository-Pattern
repo: ef sql logging https://wildermuth.com/2018/11/07/EntityFrameworkCore-Logging-in-ASP-NET-Core
repo: ef dynamic schema support https://weblogs.thinktecture.com/pawel/2018/06/entity-framework-core-changing-database-schema-at-runtime.html
repo: ef sqlite https://docs.microsoft.com/en-us/ef/core/get-started/netcore/new-db-sqlite

service: task scheduler (coravel) https://github.com/jamesmh/coravel/blob/master/Docs/Scheduler.md
service: service registry (consul) https://www.codeproject.com/Articles/1248381/Microservices-Service-Discovery
                                   http://michaco.net/blog/ServiceDiscoveryAndHealthChecksInAspNetCoreWithConsul
                                   https://open.microsoft.com/2018/10/04/use-case-modern-service-discovery-consul-azure-part-1/

operations: log analytics dashboard (operations) https://blogs.technet.microsoft.com/livedevopsinjapan/2017/08/23/log-analytics-log-search-rest-api-for-c/   
            https://techcommunity.microsoft.com/t5/Azure-Log-Analytics/Authenticate-with-client-credentials-Log-Analytics/td-p/104996  
            https://dev.int.loganalytics.io/documentation/1-Tutorials/Direct-API
operations: log analytics rest api (repo) https://dev.loganalytics.io/reference
operations: logevent repos (1-loganalytics, 2-cosmosdb) + specifications (ForCorrelationId, Since, Till, etc...)
operations: render razor without mcv https://blogs.u2u.be/peter/post/using-razor-outside-of-mvc-for-building-custom-middleware-or-other-generation-stuff
                                     https://github.com/toddams/RazorLight
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
Messaging.Infrastructure.RabitMQ

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
