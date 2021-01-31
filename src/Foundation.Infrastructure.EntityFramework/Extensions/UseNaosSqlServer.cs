//namespace Naos.Foundation.Infrastructure
//{
//    using Microsoft.EntityFrameworkCore;
//    using Microsoft.Extensions.Configuration;

//    public static partial class Extensions
//    {
//        public static DbContextOptionsBuilder UseNaosSqlServer(
//            this DbContextOptionsBuilder source,
//            IConfiguration configuration,
//            string section = "naos:service:entityFramework")
//        {
//            var entityFrameworkConfiguration = configuration.GetSection(section).Get<EntityFrameworkConfiguration>();

//            return source.UseSqlServer(entityFrameworkConfiguration?.ConnectionString ?? "Server=(localdb)\\mssqllocaldb;Database=naos;Trusted_Connection=True;MultipleActiveResultSets=True;")
//                //.ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
//                .EnableSensitiveDataLogging()
//                .EnableDetailedErrors();
//        }

//        public static DbContextOptionsBuilder UseNaosSqlServer(
//            this DbContextOptionsBuilder source,
//            string connectionString)
//        {
//            return source.UseSqlServer(connectionString ?? "Server=(localdb)\\mssqllocaldb;Database=naos;Trusted_Connection=True;MultipleActiveResultSets=True;")
//                //.ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
//                .EnableSensitiveDataLogging()
//                .EnableDetailedErrors();
//        }
//    }
//}
