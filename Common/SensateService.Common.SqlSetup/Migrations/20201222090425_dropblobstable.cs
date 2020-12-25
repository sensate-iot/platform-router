using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class dropblobstable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable("Blobs");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
