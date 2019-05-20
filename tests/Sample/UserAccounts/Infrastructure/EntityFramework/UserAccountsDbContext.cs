namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Naos.Sample.UserAccounts.Domain;

    public class UserAccountsDbContext : DbContext
    {
        public UserAccountsDbContext()
        {
        }

        public UserAccountsDbContext(DbContextOptions options)
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
                od.Property(p => p.DeactivatedReasons)
                    .HasConversion(
                        v => string.Join(";", v),
                        v => v.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                od.Property(p => p.UpdatedReasons)
                    .HasConversion(
                        v => string.Join(";", v),
                        v => v.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                //od.ToTable("EntityStates");
            });

            modelBuilder.Entity<UserAccount>().OwnsOne(e => e.AdAccount, od =>
                od.ToTable("AdAccounts"));
        }
    }
}
