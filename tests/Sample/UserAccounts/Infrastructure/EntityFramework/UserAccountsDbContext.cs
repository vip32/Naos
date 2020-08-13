namespace Naos.Sample.UserAccounts.Infrastructure
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Naos.Sample.UserAccounts.Domain;

    public class UserAccountsDbContext : DbContext
    {
        public UserAccountsDbContext() // needed for migrations
        {
        }

        public UserAccountsDbContext(DbContextOptions<UserAccountsDbContext> options)
            : base(options)
        {
        }

        // All (and only) aggregate roots are exposed as dbsets
        public DbSet<UserAccount> UserAccounts { get; set; }

        public DbSet<UserVisit> UserVisits { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=naos;Trusted_Connection=True;");
        //    optionsBuilder.UseSqlite($"Data Source={nameof(UserAccount).Pluralize()}.db");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("Development"); // TODO: this is too static, as the migration contains the environment (fixed)

            // all about owned entities: https://docs.microsoft.com/en-us/ef/core/modeling/owned-entities
            modelBuilder.HasDefaultSchema("useraccounts");
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

            modelBuilder.Entity<UserAccount>().OwnsOne(e => e.AdAccount, od => // map valueobject to same table
            {
                od.Property(p => p.Name).HasColumnName("AdName");
                od.Property(p => p.Domain).HasColumnName("AdDomain");
                //od.ToTable("AdAccounts"));
            });

            modelBuilder.Entity<UserAccount>().OwnsOne(e => e.Status)
                .Property(b => b.Value)
                .HasColumnName(nameof(UserAccount.Status)) // map valueobject to single column
                .HasConversion(new EnumToStringConverter<UserAccountStatusType>());

            //modelBuilder.Entity<UserAccount>().Ignore(e => e.State);
            modelBuilder.Entity<UserVisit>().OwnsOne(e => e.State, od =>
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
        }
    }
}
