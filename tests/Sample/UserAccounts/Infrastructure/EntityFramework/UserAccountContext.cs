namespace Naos.Sample.UserAccounts.EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using Naos.Sample.UserAccounts.Domain;

    public class UserAccountContext : DbContext
    {
        public UserAccountContext(DbContextOptions<UserAccountContext> options)
            : base(options)
        {
        }

        public DbSet<UserAccount> UserAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccount>().Ignore(e => e.State); // TODO
            //modelBuilder.Entity<StubEntity>().OwnsOne(typeof(DateTimeEpoch), "CreatedDate");
            //modelBuilder.Entity<StubEntity>().OwnsOne(typeof(DateTimeEpoch), "UpdatedDate");
        }
    }
}
