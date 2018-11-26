namespace Naos.Sample.UserAccounts.EntityFramework
{
    using Microsoft.EntityFrameworkCore;
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
            //modelBuilder.Entity<UserAccount>().Ignore(e => e.State); // TODO: map the state
            modelBuilder.Entity<UserAccount>().OwnsOne(e => e.State, od =>
            {
                //od.OwnsOne(typeof(DateTimeEpoch), "CreatedDate");
                //od.OwnsOne(typeof(DateTimeEpoch), "UpdatedDate");
                //od.OwnsOne(typeof(DateTimeEpoch), "ExpiredDate");
                //od.OwnsOne(typeof(DateTimeEpoch), "DeactivatedDate");
                //od.OwnsOne(typeof(DateTimeEpoch), "DeletedDate");
                //od.OwnsOne(typeof(DateTimeEpoch), "LastAccessedDate");
                //od.OwnsOne(c => c.CreatedDate);
                //od.OwnsOne(c => c.UpdatedDate);
                //od.OwnsOne(c => c.ExpiredDate);
                //od.OwnsOne(c => c.DeactivatedDate);
                //od.OwnsOne(c => c.DeletedDate);
                //od.OwnsOne(c => c.LastAccessedDate);
                od.Ignore(c => c.DeactivatedReasons);
                od.Ignore(c => c.UpdatedReasons); // TODO
                od.ToTable("UserAccountStates");
            });

            modelBuilder.Entity<UserAccount>().OwnsOne(e => e.LastVisitDate);
            modelBuilder.Entity<UserAccount>().OwnsOne(e => e.RegisterDate);
        }
    }
}
