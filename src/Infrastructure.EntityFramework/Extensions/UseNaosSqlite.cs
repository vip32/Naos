namespace Naos.Core.Infrastructure.EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public static partial class Extensions
    {
        public static DbContextOptionsBuilder UseNaosSqlite(
            this DbContextOptionsBuilder source,
            IConfiguration configuration,
            string name = null,
            string section = "naos:service:entityFramework")
        {
            var entityFrameworkConfiguration = configuration.GetSection(section).Get<EntityFrameworkConfiguration>();

            //source.EnableSensitiveDataLogging = true;
            return source.UseSqlite(entityFrameworkConfiguration?.ConnectionString?.StartsWith("Data Source") == true
                    ? entityFrameworkConfiguration.ConnectionString
                    : $"Data Source={name}.db");
        }

        public static DbContextOptionsBuilder UseNaosSqliteInMemory(
            this DbContextOptionsBuilder source)
        {
            return source
                .UseSqlite("DataSource=:memory:")
                .EnableSensitiveDataLogging();
        }
    }
}
