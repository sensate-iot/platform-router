using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class MoveMessageFromTriggersToTriggerActions : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Message",
				table: "Triggers");

			migrationBuilder.AddColumn<string>(
				name: "Message",
				table: "TriggerActions",
				maxLength: 255,
				nullable: false,
				defaultValue: "");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Message",
				table: "TriggerActions");

			migrationBuilder.AddColumn<string>(
				name: "Message",
				table: "Triggers",
				type: "character varying(300)",
				maxLength: 300,
				nullable: false,
				defaultValue: "");
		}
	}
}
