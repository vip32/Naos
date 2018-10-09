# Naos

# A modern cloud service architecture blueprint & reference implementation

[![Build Status](https://dev.azure.com/doomsday32/Naos/_apis/build/status/vip32.Naos)](https://dev.azure.com/doomsday32/Naos/_build/latest?definitionId=1)

- arch style: hexagonal/onion
- pattern: cqs https://www.dotnetcurry.com/patterns-practices/1461/command-query-separation-cqs
- pattern: domainevents
- pattern: integrationevents
- pattern: repositories
- pattern: specifications

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

deps:

- autofac
- ensurethat
- mediator
- xunit

stack:

- c#, netcore 2.1
- azure, docker
- builds.prop
- azure pipelines + Xyaml build/release
- core nuget publish
- proper configuration
- proper secrets
