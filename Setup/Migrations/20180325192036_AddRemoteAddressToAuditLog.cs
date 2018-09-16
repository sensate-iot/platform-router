using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Net;

namespace SensateService.Setup.Migrations
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
