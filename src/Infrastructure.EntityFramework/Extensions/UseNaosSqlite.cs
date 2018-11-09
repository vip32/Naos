namespace Naos.Core.Infrastructure.EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public static partial class Extensions
    {
        public static DbContextOptionsBuilder UseNaosSqlServer(this DbContextOptionsBuilder source, IConfiguration configuration, string section = "naos:sample:userAccounts:entityFramework")
        {
            var entityFrameworkConfiguration = configuration.GetSection(section).Get<EntityFrameworkConfiguration>();
            return source.UseSqlServer(entityFrameworkConfiguration?.ConnectionString ?? "Server=(localdb)\\mssqllocaldb;Database=naos;Trusted_Connection=True;");
        }
    }
}
