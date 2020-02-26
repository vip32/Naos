install the tools:
- `dotnet tool install --global dotnet-ef`

add the migration:
- `Add-Migration [NAME] -OutputDir .\UserAccounts\Infrastructure\EntityFramework\Migrations -Project Sample`
- or
- `dotnet-ef migrations add [NAME] -o .\UserAccounts\Infrastructure\EntityFramework\Migrations -p .\tests\Sample\Sample.csproj  -s .\tests\Sample.Application.Web\Sample.Application.Web.csproj`
- `Script-Migration -StartupProject Sample.Application.Web -Project Sample`

update the database: (also done by migration startuptask)
- `Update-Database -StartupProject Sample.Application.Web -Project Sample`
