using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class CreateAuditLogModel : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateSequence(
				name: "Id_sequence");

			migrationBuilder.CreateTable(
				name: "AuditLogs",
				columns: table => new {
					Id = table.Column<long>(nullable: false, defaultValueSql: "nextval('\"Id_sequence\"')"),
					Route = table.Column<string>(nullable: false),
					Method = table.Column<int>(nullable: false),
					Address = table.Column<IPAddress>(nullable: false),
					AuthorId = table.Column<string>(nullable: true),
					Timestamp = table.Column<DateTime>(nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_AuditLogs", x => x.Id);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AuditLogs");

			migrationBuilder.DropSequence(
				name: "Id_sequence");
		}
	}
}
