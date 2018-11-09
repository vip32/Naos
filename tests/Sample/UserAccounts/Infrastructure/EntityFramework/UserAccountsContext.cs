namespace Naos.Sample.UserAccounts.EntityFramework
{
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Naos.Core.Common;
    using Naos.Sample.UserAccounts.Domain;

    public class UserAccountsContext : DbContext
    {
        public UserAccountsContext()
        {
        }

        public UserAccountsContext(DbContextOptions options)
            : base(options)
        {
        }

        // All (and only) aggregate roots are exposed as dbsets
        public DbSet<UserAccount> UserAccounts { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=naos;Trusted_Connection=True;");
        //    optionsBuilder.UseSqlite($"Data Source={nameof(UserAccount).Pluralize()}.db");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("Development"); // TODO: this is too static, as the migration contains the environment (fixed)
            modelBuilder.Entity<UserAccount>().Ignore(e => e.State); // TODO: map the state
            modelBuilder.Entity<UserAccount>().OwnsOne(typeof(DateTimeEpoch), "LastVisitDate");
            modelBuilder.Entity<UserAccount>().OwnsOne(typeof(DateTimeEpoch), "RegisterDate");
        }
    }
}
