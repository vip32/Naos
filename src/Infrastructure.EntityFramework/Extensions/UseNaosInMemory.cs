namespace Naos.Core.Infrastructure.EntityFramework
{
    using EnsureThat;
    using Microsoft.EntityFrameworkCore;

    public static partial class Extensions
    {
        public static DbContextOptionsBuilder UseNaosInMemory(
            this DbContextOptionsBuilder source,
            string name)
        {
            EnsureArg.IsNotNullOrEmpty(name);

            return source
                .UseInMemoryDatabase(name);
                //.EnableSensitiveDataLogging();
        }
    }
}
