namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class UserAccountDomainRemove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Domain",
                table: "UserAccounts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "UserAccounts",
                nullable: true);
        }
    }
}
