api 404s https://www.strathweb.com/2018/10/convert-null-valued-results-to-404-in-asp-net-core-mvc/
secrets https://social.technet.microsoft.com/wiki/contents/articles/51871.net-core-2-managing-secrets-in-web-apps.aspx
vault config https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.1
log scopes https://www.initpals.com/net-core/scoped-logging-using-microsoft-logger-with-serilog-in-net-core-application/
xunit configuration https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects
criteria > linq https://blogs.msdn.microsoft.com/mattwar/2007/07/31/linq-building-an-iqueryable-provider-part-ii/
validation : entity.Validate(handler)
ef sqlite + inmemory : https://www.thereformedprogrammer.net/using-in-memory-databases-for-unit-testing-ef-core-applications/
webapi test + jwt https://www.domstamand.com/testing-a-webapi-in-net-core-with-integration-tests/
litedb repo https://github.com/mbdavid/LiteDB/wiki/Repository-Pattern
file repo https://github.com/selmaohneh/Repository/tree/master/Repository.FileRepository
properties https://github.com/schotime/NPoco/blob/master/src/NPoco/PocoExpando.cs
spec mapping with visitor https://fabiomarreco.github.io/blog/2018/specificationpattern-with-entityframework/
tenant resolver https://stackoverflow.com/questions/41820206/c-sharp-architecture-pattern-for-tenant-specific-business-logic

repo insert/update
repo return status when upsert (insert or update) > + entity = tuple

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


