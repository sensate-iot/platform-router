using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class UpdateIndexes : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropUniqueConstraint(
				name: "AlternateKey_UserToken",
				table: "PhoneNumberTokens");

			migrationBuilder.AddUniqueConstraint(
				name: "AK_PhoneNumberTokens_UserToken",
				table: "PhoneNumberTokens",
				column: "UserToken");

			migrationBuilder.CreateIndex(
				name: "IX_PhoneNumberTokens_PhoneNumber",
				table: "PhoneNumberTokens",
				column: "PhoneNumber");

			migrationBuilder.CreateIndex(
				name: "IX_AuthTokens_UserId",
				table: "AuthTokens",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AuthTokens_Value",
				table: "AuthTokens",
				column: "Value");

			migrationBuilder.CreateIndex(
				name: "IX_ApiKeys_Type",
				table: "ApiKeys",
				column: "Type");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropUniqueConstraint(
				name: "AK_PhoneNumberTokens_UserToken",
				table: "PhoneNumberTokens");

			migrationBuilder.DropIndex(
				name: "IX_PhoneNumberTokens_PhoneNumber",
				table: "PhoneNumberTokens");

			migrationBuilder.DropIndex(
				name: "IX_AuthTokens_UserId",
				table: "AuthTokens");

			migrationBuilder.DropIndex(
				name: "IX_AuthTokens_Value",
				table: "AuthTokens");

			migrationBuilder.DropIndex(
				name: "IX_ApiKeys_Type",
				table: "ApiKeys");

			migrationBuilder.AddUniqueConstraint(
				name: "AlternateKey_UserToken",
				table: "PhoneNumberTokens",
				column: "UserToken");
		}
	}
}
