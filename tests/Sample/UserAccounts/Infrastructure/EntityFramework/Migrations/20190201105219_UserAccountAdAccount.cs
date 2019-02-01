namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class UserAccountAdAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdAccount_Domain",
                table: "UserAccounts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdAccount_Name",
                table: "UserAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdAccount_Domain",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "AdAccount_Name",
                table: "UserAccounts");
        }
    }
}
