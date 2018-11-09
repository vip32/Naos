[![Build Status](https://dev.azure.com/doomsday32/Naos/_apis/build/status/vip32.Naos)](https://dev.azure.com/doomsday32/Naos/_build/latest?definitionId=1)
[![CodeFactor](https://www.codefactor.io/repository/github/vip32/naos.core/badge)](https://www.codefactor.io/repository/github/vip32/naos.core)

![Alt text](/docs/logo.png?raw=true "Naos")

<p align="center"><h1>A mildly opiniated modern cloud service architecture blueprint & reference implementation</h1></p>

## architectural concepts
- architectural style: hexagonal/onion
- pattern: domainevents
- pattern: repositories (inmemory, cosmosdb, entityframework)
- pattern: decorators
- pattern: specifications
- pattern: app commands/handlers (+behaviors)
- pattern: messaging (servicebus, rabbitmq)

## dev stack
C#, .Net Core 2.x, EnsureThat, Serilog, SimpleInjector, Mediator, FluentValidation, AutoMapper, XUnit, Shouldly, NSubstitute

## secrets setup (Azure Key Vault)
- Create a key vault [^](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-get-started)
- Store key vault name in an environment variable
  - `naos:secrets:vault:name`
- Register an application with Azure Active Directory [^](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-get-started)
- Store application clientId (ApplicationId) and clientSecret (Key) in environment variables
  - `naos:secrets:vault:clientId` & `naos:secrets:vault:clientSecret`
- Authorize the application to use the key or secret [^](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-get-started)

*or* use the following [json configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1#file-configuration-provider).
```
{
  "naos": {
    "secrets": {
      "vault": {
        "name": "[VAULT_NAME]",
        "clientId": "[AAD_APPLICATION_ID]",
        "clientSecret": "[AAD_APPLICATION_KEY]"
      }
    },
    "messaging": {
      "serviceBus": {
        "enabled":  true,
        "connectionString": "",
        "subscriptionId": "",
        "resourceGroup": "",
        "namespaceName": "",
        "tenantId":  "",
        "clientId": "",
        "clientSecret": "",
      },
      "rabbitMQ": {
      }
    }
  }
}
```

*or* use a multitude of the usual [netcore configuration providers](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1#providers) to store the settings


## messaging setup (Azure ServiceBus)

**NOTE**:
Naos:Messaging needs to administer the Azure ServiceBus by using the [Azure Resource Manager](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-overview) API. To use this API some [extra setup](https://tomasherceg.com/blog/post/azure-servicebus-in-net-core-managing-topics-queues-and-subscriptions-from-the-code) has to be
done.

- create a servicebus (standard pricing)
- `Get-AzureRmSubscription` (=subscriptionId/tenantId)
- `$p = ConvertTo-SecureString -asplaintext -string "PRINCIPAL_PASSWORD" -force` (=clientSecret)
- `$sp = New-AzureRmADServicePrincipal -DisplayName "PRINCIPAL_NAME" -Password $p`
- `Write-Host "clientid:" $sp.ApplicationId` (=clientId)
- `New-AzureRmRoleAssignment -ServicePrincipalName $sp.ApplicationId -ResourceGroupName RESOURCE_GROUP -ResourceName RESOURCE_NAME -RoleDefinitionName Contributor -ResourceType Microsoft.ServiceBus/namespaces`

key vault keys: (or use the json configuration above)
- `development-naos--messaging--serviceBus--subscriptionId`
- `development-naos--messaging--serviceBus--tenantId`
- `development-naos--messaging--serviceBus--connectionString`
- `development-naos--messaging--serviceBus--resourceGroup` (=RESOURCE_GROUP)
- `development-naos--messaging--serviceBus--namespaceName` (=RESOURCE_NAME)
- `development-naos--messaging--serviceBus--clientId`
- `development-naos--messaging--serviceBus--clientSecret`


## sample services

##### Countries
- domain, specifications, 
- inmemory mapped repository (entity > dto), tenant decorator
- automapper

##### Customers
- domain, specifications
- cosmosdb repository, tenant decorator
- app command (CreateCustomer) + handler (CreateCustomerCommandHandler) TODO which triggers CreatedCustomer message 
- TODO: validate command (behavior)

##### UserAccounts
- domain, specifications, 
- messagehandler (CustomerCreated)
- entityframework sql repository, tenant decorator
- TODO: handle CreatedCustomer message

- `Add-Migration [NAME] -OutputDir .\UserAccounts\Infrastructure\EntityFramework\Migrations -StartupProject Sample.App.Web -Project Sample`
- `Script-Migration -StartupProject Sample.App.Web -Project Sample`
- `Update-Database -StartupProject Sample.App.Web -Project Sample`

### core components
- Naos:App
- Naos:App.Web
- Naos:Common
- Naos:Domain
- Naos:Infrastructure
- Naos:Messaging
- Naos:Commands
- Naos:...

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