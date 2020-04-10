using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class AddRequestCountToApiKeyTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<long>(
				name: "RequestCount",
				table: "ApiKeys",
				nullable: false,
				defaultValue: 0L);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "RequestCount",
				table: "ApiKeys");
		}
	}
}
