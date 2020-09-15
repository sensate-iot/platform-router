using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class AddFileSizeToBlobsTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<long>(
				name: "FileSize",
				table: "Blobs",
				nullable: false,
				defaultValue: 0L);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "FileSize",
				table: "Blobs");
		}
	}
}
