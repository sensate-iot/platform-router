using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
    public partial class AddForeignKeyToAuditLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AuthorId",
                table: "AuditLogs",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Method",
                table: "AuditLogs",
                column: "Method");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_AuthorId",
                table: "AuditLogs",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_AuthorId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_AuthorId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Method",
                table: "AuditLogs");
        }
    }
}
