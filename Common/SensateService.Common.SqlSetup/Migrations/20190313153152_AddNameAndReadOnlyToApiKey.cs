using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class AddNameAndReadOnlyToApiKey : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "Name",
				table: "AspNetApiKeys",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<bool>(
				name: "ReadOnly",
				table: "AspNetApiKeys",
				nullable: false,
				defaultValue: false);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Name",
				table: "AspNetApiKeys");

			migrationBuilder.DropColumn(
				name: "ReadOnly",
				table: "AspNetApiKeys");
		}
	}
}
