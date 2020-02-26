namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "useraccounts");

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                schema: "useraccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IdentifierHash = table.Column<string>(nullable: true),
                    State_CreatedBy = table.Column<string>(nullable: true),
                    State_CreatedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_CreatedDescription = table.Column<string>(nullable: true),
                    State_UpdatedBy = table.Column<string>(nullable: true),
                    State_UpdatedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_UpdatedDescription = table.Column<string>(nullable: true),
                    State_UpdatedReasons = table.Column<string>(nullable: true),
                    State_ExpiredBy = table.Column<string>(nullable: true),
                    State_ExpiredDate = table.Column<DateTimeOffset>(nullable: true),
                    State_ExpiredDescription = table.Column<string>(nullable: true),
                    State_Deactivated = table.Column<bool>(nullable: true),
                    State_DeactivatedReasons = table.Column<string>(nullable: true),
                    State_DeactivatedBy = table.Column<string>(nullable: true),
                    State_DeactivatedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_DeactivatedDescription = table.Column<string>(nullable: true),
                    State_Deleted = table.Column<bool>(nullable: true),
                    State_DeletedBy = table.Column<string>(nullable: true),
                    State_DeletedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_DeletedReason = table.Column<string>(nullable: true),
                    State_DeletedDescription = table.Column<string>(nullable: true),
                    State_LastAccessedBy = table.Column<string>(nullable: true),
                    State_LastAccessedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_LastAccessedDescription = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    VisitCount = table.Column<int>(nullable: false),
                    LastVisitDate = table.Column<DateTimeOffset>(nullable: true),
                    RegisterDate = table.Column<DateTimeOffset>(nullable: true),
                    TenantId = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserVisits",
                schema: "useraccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IdentifierHash = table.Column<string>(nullable: true),
                    State_CreatedBy = table.Column<string>(nullable: true),
                    State_CreatedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_CreatedDescription = table.Column<string>(nullable: true),
                    State_UpdatedBy = table.Column<string>(nullable: true),
                    State_UpdatedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_UpdatedDescription = table.Column<string>(nullable: true),
                    State_UpdatedReasons = table.Column<string>(nullable: true),
                    State_ExpiredBy = table.Column<string>(nullable: true),
                    State_ExpiredDate = table.Column<DateTimeOffset>(nullable: true),
                    State_ExpiredDescription = table.Column<string>(nullable: true),
                    State_Deactivated = table.Column<bool>(nullable: true),
                    State_DeactivatedReasons = table.Column<string>(nullable: true),
                    State_DeactivatedBy = table.Column<string>(nullable: true),
                    State_DeactivatedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_DeactivatedDescription = table.Column<string>(nullable: true),
                    State_Deleted = table.Column<bool>(nullable: true),
                    State_DeletedBy = table.Column<string>(nullable: true),
                    State_DeletedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_DeletedReason = table.Column<string>(nullable: true),
                    State_DeletedDescription = table.Column<string>(nullable: true),
                    State_LastAccessedBy = table.Column<string>(nullable: true),
                    State_LastAccessedDate = table.Column<DateTimeOffset>(nullable: true),
                    State_LastAccessedDescription = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(nullable: true),
                    TenantId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVisits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdAccounts",
                schema: "useraccounts",
                columns: table => new
                {
                    UserAccountId = table.Column<Guid>(nullable: false),
                    Domain = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdAccounts", x => x.UserAccountId);
                    table.ForeignKey(
                        name: "FK_AdAccounts_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalSchema: "useraccounts",
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdAccounts",
                schema: "useraccounts");

            migrationBuilder.DropTable(
                name: "UserVisits",
                schema: "useraccounts");

            migrationBuilder.DropTable(
                name: "UserAccounts",
                schema: "useraccounts");
        }
    }
}
