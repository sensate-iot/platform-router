using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class UpdateOnDeleteRules : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_AuditLogs_Users_AuthorId",
				table: "AuditLogs");

			migrationBuilder.DropForeignKey(
				name: "FK_PhoneNumberTokens_Users_UserId",
				table: "PhoneNumberTokens");

			migrationBuilder.AlterColumn<string>(
				name: "UserId",
				table: "PhoneNumberTokens",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text",
				oldNullable: true);

			migrationBuilder.AddForeignKey(
				name: "FK_AuditLogs_Users_AuthorId",
				table: "AuditLogs",
				column: "AuthorId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_PhoneNumberTokens_Users_UserId",
				table: "PhoneNumberTokens",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_SensorLinks_Users_UserId",
				table: "SensorLinks",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_AuditLogs_Users_AuthorId",
				table: "AuditLogs");

			migrationBuilder.DropForeignKey(
				name: "FK_PhoneNumberTokens_Users_UserId",
				table: "PhoneNumberTokens");

			migrationBuilder.DropForeignKey(
				name: "FK_SensorLinks_Users_UserId",
				table: "SensorLinks");

			migrationBuilder.AlterColumn<string>(
				name: "UserId",
				table: "PhoneNumberTokens",
				type: "text",
				nullable: true,
				oldClrType: typeof(string));

			migrationBuilder.AddForeignKey(
				name: "FK_AuditLogs_Users_AuthorId",
				table: "AuditLogs",
				column: "AuthorId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "FK_PhoneNumberTokens_Users_UserId",
				table: "PhoneNumberTokens",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
