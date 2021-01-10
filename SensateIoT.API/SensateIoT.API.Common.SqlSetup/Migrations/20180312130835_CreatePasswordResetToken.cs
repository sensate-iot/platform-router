using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class CreatePasswordResetToken : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "PasswordResetTokens",
				columns: table => new {
					UserToken = table.Column<string>(nullable: false),
					IdentityToken = table.Column<string>(nullable: true)
				},
				constraints: table => {
					table.PrimaryKey("PK_PasswordResetTokens", x => x.UserToken);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PasswordResetTokens");
		}
	}
}
