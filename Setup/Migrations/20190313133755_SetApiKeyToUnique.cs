using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.Setup.Migrations
{
    public partial class SetApiKeyToUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetApiKeys_ApiKey",
                table: "AspNetApiKeys");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetApiKeys_ApiKey",
                table: "AspNetApiKeys",
                column: "ApiKey",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetApiKeys_ApiKey",
                table: "AspNetApiKeys");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetApiKeys_ApiKey",
                table: "AspNetApiKeys",
                column: "ApiKey");
        }
    }
}
