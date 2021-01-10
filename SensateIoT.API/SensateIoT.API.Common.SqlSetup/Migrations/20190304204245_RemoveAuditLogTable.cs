using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class RemoveAuditLogTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AspNetAuditLogs");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "AspNetAuditLogs",
				columns: table => new {
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Address = table.Column<IPAddress>(nullable: false),
					AuthorId = table.Column<string>(nullable: true),
					Method = table.Column<int>(nullable: false),
					Route = table.Column<string>(nullable: false),
					Timestamp = table.Column<DateTime>(nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_AspNetAuditLogs", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetAuditLogs_AspNetUsers_AuthorId",
						column: x => x.AuthorId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_AspNetAuditLogs_AuthorId",
				table: "AspNetAuditLogs",
				column: "AuthorId");
		}
	}
}
