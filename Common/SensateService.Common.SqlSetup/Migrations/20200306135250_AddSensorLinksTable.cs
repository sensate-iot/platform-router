using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class AddSensorLinksTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "SensorLinks",
				columns: table => new {
					SensorId = table.Column<string>(nullable: false),
					UserId = table.Column<string>(nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_SensorLinks", x => new { x.UserId, x.SensorId });
				});

			migrationBuilder.CreateIndex(
				name: "IX_SensorLinks_UserId",
				table: "SensorLinks",
				column: "UserId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "SensorLinks");
		}
	}
}
