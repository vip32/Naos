## Sample services

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

- `Add-Migration [NAME] -OutputDir .\UserAccounts\Infrastructure\EntityFramework\Migrations -Project Sample`
- `Script-Migration -StartupProject Sample.Application.Web -Project Sample`
- `Update-Database -StartupProject Sample.Application.Web -Project Sample`
