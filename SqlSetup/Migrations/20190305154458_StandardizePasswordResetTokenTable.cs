using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class StandardizePasswordResetTokenTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropPrimaryKey(
				name: "PK_PasswordResetTokens",
				table: "PasswordResetTokens");

			migrationBuilder.RenameTable(
				name: "PasswordResetTokens",
				newName: "AspNetPasswordResetTokens");

			migrationBuilder.AddPrimaryKey(
				name: "PK_AspNetPasswordResetTokens",
				table: "AspNetPasswordResetTokens",
				column: "UserToken");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropPrimaryKey(
				name: "PK_AspNetPasswordResetTokens",
				table: "AspNetPasswordResetTokens");

			migrationBuilder.RenameTable(
				name: "AspNetPasswordResetTokens",
				newName: "PasswordResetTokens");

			migrationBuilder.AddPrimaryKey(
				name: "PK_PasswordResetTokens",
				table: "PasswordResetTokens",
				column: "UserToken");
		}
	}
}
