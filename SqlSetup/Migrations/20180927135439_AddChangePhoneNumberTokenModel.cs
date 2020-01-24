using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
    public partial class AddChangePhoneNumberTokenModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangePhoneNumberTokens",
                columns: table => new
                {
                    IdentityToken = table.Column<string>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    UserToken = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangePhoneNumberTokens", x => x.IdentityToken);
                    table.UniqueConstraint("AlternateKey_UserToken", x => x.UserToken);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangePhoneNumberTokens");
        }
    }
}
