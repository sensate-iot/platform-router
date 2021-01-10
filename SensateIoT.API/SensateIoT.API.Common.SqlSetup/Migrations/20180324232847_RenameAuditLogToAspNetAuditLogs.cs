using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class RenameAuditLogToAspNetAuditLogs : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_AuditLogs_AspNetUsers_AuthorId",
				table: "AuditLogs");

			migrationBuilder.DropPrimaryKey(
				name: "PK_AuditLogs",
				table: "AuditLogs");

			migrationBuilder.RenameTable(
				name: "AuditLogs",
				newName: "AspNetAuditLogs");

			migrationBuilder.RenameIndex(
				name: "IX_AuditLogs_AuthorId",
				table: "AspNetAuditLogs",
				newName: "IX_AspNetAuditLogs_AuthorId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_AspNetAuditLogs",
				table: "AspNetAuditLogs",
				column: "Id");

			migrationBuilder.AddForeignKey(
				name: "FK_AspNetAuditLogs_AspNetUsers_AuthorId",
				table: "AspNetAuditLogs",
				column: "AuthorId",
				principalTable: "AspNetUsers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_AspNetAuditLogs_AspNetUsers_AuthorId",
				table: "AspNetAuditLogs");

			migrationBuilder.DropPrimaryKey(
				name: "PK_AspNetAuditLogs",
				table: "AspNetAuditLogs");

			migrationBuilder.RenameTable(
				name: "AspNetAuditLogs",
				newName: "AuditLogs");

			migrationBuilder.RenameIndex(
				name: "IX_AspNetAuditLogs_AuthorId",
				table: "AuditLogs",
				newName: "IX_AuditLogs_AuthorId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_AuditLogs",
				table: "AuditLogs",
				column: "Id");

			migrationBuilder.AddForeignKey(
				name: "FK_AuditLogs_AspNetUsers_AuthorId",
				table: "AuditLogs",
				column: "AuthorId",
				principalTable: "AspNetUsers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
