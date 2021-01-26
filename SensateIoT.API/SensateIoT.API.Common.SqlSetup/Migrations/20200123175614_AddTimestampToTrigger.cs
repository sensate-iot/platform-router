using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class AddTimestampToTrigger : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTime>(
				name: "LastTriggered",
				table: "Triggers",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

			migrationBuilder.CreateIndex(
				name: "IX_Triggers_LastTriggered",
				table: "Triggers",
				column: "LastTriggered");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Triggers_LastTriggered",
				table: "Triggers");

			migrationBuilder.DropColumn(
				name: "LastTriggered",
				table: "Triggers");
		}
	}
}
