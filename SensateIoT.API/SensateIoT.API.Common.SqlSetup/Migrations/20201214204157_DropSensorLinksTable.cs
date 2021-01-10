using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class DropSensorLinksTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable("SensorLinks");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
