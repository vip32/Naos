namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IdentifierHash = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    VisitCount = table.Column<int>(nullable: false),
                    LastVisitDate_DateTime = table.Column<DateTime>(nullable: false),
                    RegisterDate_DateTime = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccountStates",
                columns: table => new
                {
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedEpoch = table.Column<long>(nullable: false),
                    CreatedDescription = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedEpoch = table.Column<long>(nullable: false),
                    UpdatedDescription = table.Column<string>(nullable: true),
                    ExpiredBy = table.Column<string>(nullable: true),
                    ExpiredDate = table.Column<DateTime>(nullable: true),
                    ExpiredEpoch = table.Column<long>(nullable: true),
                    ExpiredDescription = table.Column<string>(nullable: true),
                    Deactivated = table.Column<bool>(nullable: true),
                    DeactivatedBy = table.Column<string>(nullable: true),
                    DeactivatedDate = table.Column<DateTime>(nullable: true),
                    DeactivatedEpoch = table.Column<long>(nullable: true),
                    DeactivatedDescription = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: true),
                    DeletedBy = table.Column<string>(nullable: true),
                    DeletedDate = table.Column<DateTime>(nullable: true),
                    DeletedEpoch = table.Column<long>(nullable: true),
                    DeletedReason = table.Column<string>(nullable: true),
                    DeletedDescription = table.Column<string>(nullable: true),
                    LastAccessedBy = table.Column<string>(nullable: true),
                    LastAccessedDate = table.Column<DateTime>(nullable: true),
                    LastAccessedEpoch = table.Column<long>(nullable: true),
                    LastAccessedDescription = table.Column<string>(nullable: true),
                    UserAccountId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountStates", x => x.UserAccountId);
                    table.ForeignKey(
                        name: "FK_UserAccountStates_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccountStates");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
