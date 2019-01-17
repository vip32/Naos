namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class UserAccountDomain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "UserAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Domain",
                table: "UserAccounts");
        }
    }
}
