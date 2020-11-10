using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
    public partial class DropTriggerTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.DropTable("TriggerInvocations");
	        migrationBuilder.DropTable("TriggerActions");
	        migrationBuilder.DropTable("Triggers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
