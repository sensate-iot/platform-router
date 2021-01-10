using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class AddRemoteAddressToAuditLog : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<IPAddress>(
				name: "Address",
				table: "AspNetAuditLogs",
				nullable: false);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Address",
				table: "AspNetAuditLogs");
		}
	}
}
