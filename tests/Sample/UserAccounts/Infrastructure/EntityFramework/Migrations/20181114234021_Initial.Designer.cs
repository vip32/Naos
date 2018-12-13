﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Naos.Sample.UserAccounts.EntityFramework;

namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework.Migrations
{
    [DbContext(typeof(UserAccountsContext))]
    [Migration("20181114234021_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Naos.Sample.UserAccounts.Domain.UserAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("IdentifierHash");

                    b.Property<string>("TenantId");

                    b.Property<int>("VisitCount");

                    b.HasKey("Id");

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("Naos.Sample.UserAccounts.Domain.UserAccount", b =>
                {
                    b.OwnsOne("Naos.Core.Domain.State", "State", b1 =>
                        {
                            b1.Property<Guid?>("UserAccountId");

                            b1.Property<string>("CreatedBy");

                            b1.Property<DateTime>("CreatedDate");

                            b1.Property<string>("CreatedDescription");

                            b1.Property<long>("CreatedEpoch");

                            b1.Property<bool?>("Deactivated");

                            b1.Property<string>("DeactivatedBy");

                            b1.Property<DateTime?>("DeactivatedDate");

                            b1.Property<string>("DeactivatedDescription");

                            b1.Property<long?>("DeactivatedEpoch");

                            b1.Property<bool?>("Deleted");

                            b1.Property<string>("DeletedBy");

                            b1.Property<DateTime?>("DeletedDate");

                            b1.Property<string>("DeletedDescription");

                            b1.Property<long?>("DeletedEpoch");

                            b1.Property<string>("DeletedReason");

                            b1.Property<string>("ExpiredBy");

                            b1.Property<DateTime?>("ExpiredDate");

                            b1.Property<string>("ExpiredDescription");

                            b1.Property<long?>("ExpiredEpoch");

                            b1.Property<string>("LastAccessedBy");

                            b1.Property<DateTime?>("LastAccessedDate");

                            b1.Property<string>("LastAccessedDescription");

                            b1.Property<long?>("LastAccessedEpoch");

                            b1.Property<string>("UpdatedBy");

                            b1.Property<DateTime>("UpdatedDate");

                            b1.Property<string>("UpdatedDescription");

                            b1.Property<long>("UpdatedEpoch");

                            b1.ToTable("UserAccountStates");

                            b1.HasOne("Naos.Sample.UserAccounts.Domain.UserAccount")
                                .WithOne("State")
                                .HasForeignKey("Naos.Core.Domain.State", "UserAccountId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne("Naos.Core.Common.DateTimeEpoch", "LastVisitDate", b1 =>
                        {
                            b1.Property<Guid>("UserAccountId");

                            b1.Property<DateTime>("DateTime");

                            b1.ToTable("UserAccounts");

                            b1.HasOne("Naos.Sample.UserAccounts.Domain.UserAccount")
                                .WithOne("LastVisitDate")
                                .HasForeignKey("Naos.Core.Common.DateTimeEpoch", "UserAccountId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne("Naos.Core.Common.DateTimeEpoch", "RegisterDate", b1 =>
                        {
                            b1.Property<Guid>("UserAccountId");

                            b1.Property<DateTime>("DateTime");

                            b1.ToTable("UserAccounts");

                            b1.HasOne("Naos.Sample.UserAccounts.Domain.UserAccount")
                                .WithOne("RegisterDate")
                                .HasForeignKey("Naos.Core.Common.DateTimeEpoch", "UserAccountId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
