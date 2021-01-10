using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class StandardizeTableNames : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_ChangePhoneNumberTokens_AspNetUsers_UserId",
				table: "ChangePhoneNumberTokens");

			migrationBuilder.DropPrimaryKey(
				name: "PK_ChangePhoneNumberTokens",
				table: "ChangePhoneNumberTokens");

			migrationBuilder.DropPrimaryKey(
				name: "PK_ChangeEmailTokens",
				table: "ChangeEmailTokens");

			migrationBuilder.RenameTable(
				name: "ChangePhoneNumberTokens",
				newName: "AspNetPhoneNumberTokens");

			migrationBuilder.RenameTable(
				name: "ChangeEmailTokens",
				newName: "AspNetEmailTokens");

			migrationBuilder.RenameIndex(
				name: "IX_ChangePhoneNumberTokens_UserId",
				table: "AspNetPhoneNumberTokens",
				newName: "IX_AspNetPhoneNumberTokens_UserId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_AspNetPhoneNumberTokens",
				table: "AspNetPhoneNumberTokens",
				columns: new[] { "IdentityToken", "PhoneNumber" });

			migrationBuilder.AddPrimaryKey(
				name: "PK_AspNetEmailTokens",
				table: "AspNetEmailTokens",
				column: "IdentityToken");

			migrationBuilder.AddForeignKey(
				name: "FK_AspNetPhoneNumberTokens_AspNetUsers_UserId",
				table: "AspNetPhoneNumberTokens",
				column: "UserId",
				principalTable: "AspNetUsers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_AspNetPhoneNumberTokens_AspNetUsers_UserId",
				table: "AspNetPhoneNumberTokens");

			migrationBuilder.DropPrimaryKey(
				name: "PK_AspNetPhoneNumberTokens",
				table: "AspNetPhoneNumberTokens");

			migrationBuilder.DropPrimaryKey(
				name: "PK_AspNetEmailTokens",
				table: "AspNetEmailTokens");

			migrationBuilder.RenameTable(
				name: "AspNetPhoneNumberTokens",
				newName: "ChangePhoneNumberTokens");

			migrationBuilder.RenameTable(
				name: "AspNetEmailTokens",
				newName: "ChangeEmailTokens");

			migrationBuilder.RenameIndex(
				name: "IX_AspNetPhoneNumberTokens_UserId",
				table: "ChangePhoneNumberTokens",
				newName: "IX_ChangePhoneNumberTokens_UserId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_ChangePhoneNumberTokens",
				table: "ChangePhoneNumberTokens",
				columns: new[] { "IdentityToken", "PhoneNumber" });

			migrationBuilder.AddPrimaryKey(
				name: "PK_ChangeEmailTokens",
				table: "ChangeEmailTokens",
				column: "IdentityToken");

			migrationBuilder.AddForeignKey(
				name: "FK_ChangePhoneNumberTokens_AspNetUsers_UserId",
				table: "ChangePhoneNumberTokens",
				column: "UserId",
				principalTable: "AspNetUsers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
