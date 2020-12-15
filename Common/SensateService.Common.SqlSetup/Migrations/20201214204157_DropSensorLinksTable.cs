using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
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
