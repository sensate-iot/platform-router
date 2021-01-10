using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class CreateTriggerActionsTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "UpperEdige",
				table: "Triggers");

			migrationBuilder.AddColumn<decimal>(
				name: "UpperEdge",
				table: "Triggers",
				nullable: true);

			migrationBuilder.CreateTable(
				name: "TriggerActions",
				columns: table => new {
					TriggerId = table.Column<long>(nullable: false),
					Channel = table.Column<int>(nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_TriggerActions", x => new { x.TriggerId, x.Channel });
					table.ForeignKey(
						name: "FK_TriggerActions_Triggers_TriggerId",
						column: x => x.TriggerId,
						principalTable: "Triggers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "TriggerActions");

			migrationBuilder.DropColumn(
				name: "UpperEdge",
				table: "Triggers");

			migrationBuilder.AddColumn<decimal>(
				name: "UpperEdige",
				table: "Triggers",
				type: "numeric",
				nullable: true);
		}
	}
}
