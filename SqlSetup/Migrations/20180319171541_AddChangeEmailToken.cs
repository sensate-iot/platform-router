using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SensateService.SqlSetup.Migrations
{
    public partial class AddChangeEmailToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeEmailTokens",
                columns: table => new
                {
                    IdentityToken = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    UserToken = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeEmailTokens", x => x.IdentityToken);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeEmailTokens");
        }
    }
}
