namespace Naos.Sample.UserAccounts.Infrastructure.EntityFramework.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class StateEpochRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedEpoch",
                table: "UserAccountStates");

            migrationBuilder.DropColumn(
                name: "DeactivatedEpoch",
                table: "UserAccountStates");

            migrationBuilder.DropColumn(
                name: "DeletedEpoch",
                table: "UserAccountStates");

            migrationBuilder.DropColumn(
                name: "ExpiredEpoch",
                table: "UserAccountStates");

            migrationBuilder.DropColumn(
                name: "LastAccessedEpoch",
                table: "UserAccountStates");

            migrationBuilder.DropColumn(
                name: "UpdatedEpoch",
                table: "UserAccountStates");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "UserAccountStates",
                nullable: false,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastAccessedDate",
                table: "UserAccountStates",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiredDate",
                table: "UserAccountStates",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "UserAccountStates",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeactivatedDate",
                table: "UserAccountStates",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "UserAccountStates",
                nullable: false,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "UserAccountStates",
                nullable: false,
                oldClrType: typeof(DateTimeOffset));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastAccessedDate",
                table: "UserAccountStates",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiredDate",
                table: "UserAccountStates",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedDate",
                table: "UserAccountStates",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeactivatedDate",
                table: "UserAccountStates",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserAccountStates",
                nullable: false,
                oldClrType: typeof(DateTimeOffset));

            migrationBuilder.AddColumn<long>(
                name: "CreatedEpoch",
                table: "UserAccountStates",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DeactivatedEpoch",
                table: "UserAccountStates",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedEpoch",
                table: "UserAccountStates",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExpiredEpoch",
                table: "UserAccountStates",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastAccessedEpoch",
                table: "UserAccountStates",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedEpoch",
                table: "UserAccountStates",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
