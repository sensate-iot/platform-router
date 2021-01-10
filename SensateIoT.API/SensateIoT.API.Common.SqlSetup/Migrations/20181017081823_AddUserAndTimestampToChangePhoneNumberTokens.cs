using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class AddUserAndTimestampToChangePhoneNumberTokens : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropPrimaryKey(
				name: "PK_ChangePhoneNumberTokens",
				table: "ChangePhoneNumberTokens");

			migrationBuilder.AlterColumn<string>(
				name: "PhoneNumber",
				table: "ChangePhoneNumberTokens",
				nullable: false,
				oldClrType: typeof(string),
				oldNullable: true);

			migrationBuilder.AddColumn<DateTime>(
				name: "Timestamp",
				table: "ChangePhoneNumberTokens",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

			migrationBuilder.AddColumn<string>(
				name: "UserId",
				table: "ChangePhoneNumberTokens",
				nullable: true);

			migrationBuilder.AddPrimaryKey(
				name: "PK_ChangePhoneNumberTokens",
				table: "ChangePhoneNumberTokens",
				columns: new[] { "IdentityToken", "PhoneNumber" });

			migrationBuilder.CreateIndex(
				name: "IX_ChangePhoneNumberTokens_UserId",
				table: "ChangePhoneNumberTokens",
				column: "UserId");

			migrationBuilder.AddForeignKey(
				name: "FK_ChangePhoneNumberTokens_AspNetUsers_UserId",
				table: "ChangePhoneNumberTokens",
				column: "UserId",
				principalTable: "AspNetUsers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_ChangePhoneNumberTokens_AspNetUsers_UserId",
				table: "ChangePhoneNumberTokens");

			migrationBuilder.DropPrimaryKey(
				name: "PK_ChangePhoneNumberTokens",
				table: "ChangePhoneNumberTokens");

			migrationBuilder.DropIndex(
				name: "IX_ChangePhoneNumberTokens_UserId",
				table: "ChangePhoneNumberTokens");

			migrationBuilder.DropColumn(
				name: "Timestamp",
				table: "ChangePhoneNumberTokens");

			migrationBuilder.DropColumn(
				name: "UserId",
				table: "ChangePhoneNumberTokens");

			migrationBuilder.AlterColumn<string>(
				name: "PhoneNumber",
				table: "ChangePhoneNumberTokens",
				nullable: true,
				oldClrType: typeof(string));

			migrationBuilder.AddPrimaryKey(
				name: "PK_ChangePhoneNumberTokens",
				table: "ChangePhoneNumberTokens",
				column: "IdentityToken");
		}
	}
}
