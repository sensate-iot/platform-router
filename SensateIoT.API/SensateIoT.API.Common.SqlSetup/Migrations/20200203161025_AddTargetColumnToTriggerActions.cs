using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class AddTargetColumnToTriggerActions : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "Target",
				table: "TriggerActions",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Target",
				table: "TriggerActions");
		}
	}
}
