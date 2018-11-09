namespace Naos.Sample.UserAccounts.EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using Naos.Core.Common;
    using Naos.Sample.UserAccounts.Domain;

    public class UserAccountContext : DbContext
    {
        public UserAccountContext()
        {
        }

        public UserAccountContext(DbContextOptions<UserAccountContext> options)
            : base(options)
        {
        }

        public DbSet<UserAccount> UserAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=naos;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccount>().Ignore(e => e.State); // TODO
            modelBuilder.Entity<UserAccount>().OwnsOne(typeof(DateTimeEpoch), "LastVisitDate");
            modelBuilder.Entity<UserAccount>().OwnsOne(typeof(DateTimeEpoch), "RegisterDate");
        }
    }
}
