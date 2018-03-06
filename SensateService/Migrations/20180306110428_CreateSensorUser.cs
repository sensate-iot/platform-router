using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SensateService.Migrations
{
    public partial class CreateSensorUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSensors",
                columns: table => new
                {
                    SensorId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    Owner = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSensors", x => new { x.SensorId, x.UserId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSensors");
        }
    }
}
