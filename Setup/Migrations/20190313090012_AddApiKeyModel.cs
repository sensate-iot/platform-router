using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.Setup.Migrations
{
    public partial class AddApiKeyModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetApiKeys",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    ApiKey = table.Column<string>(nullable: false),
                    Revoked = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetApiKeys", x => x.Id);
                    table.UniqueConstraint("AK_AspNetApiKeys_ApiKey", x => x.ApiKey);
                    table.ForeignKey(
                        name: "FK_AspNetApiKeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetApiKeys_UserId",
                table: "AspNetApiKeys",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetApiKeys");
        }
    }
}
