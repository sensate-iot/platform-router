using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class CreateAuditLog : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "AuditLogs",
				columns: table => new {
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					AuthorId = table.Column<string>(nullable: true),
					Route = table.Column<string>(nullable: false),
					Timestamp = table.Column<DateTime>(nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_AuditLogs", x => x.Id);
					table.ForeignKey(
						name: "FK_AuditLogs_AspNetUsers_AuthorId",
						column: x => x.AuthorId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_AuditLogs_AuthorId",
				table: "AuditLogs",
				column: "AuthorId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AuditLogs");
		}
	}
}
