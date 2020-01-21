namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class UserAccountStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserAccounts");
        }
    }
}
