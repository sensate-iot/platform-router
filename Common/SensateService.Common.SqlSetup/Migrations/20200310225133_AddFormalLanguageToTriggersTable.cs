using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class AddFormalLanguageToTriggersTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "FormalLanguage",
				table: "Triggers",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "Type",
				table: "Triggers",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "IX_Triggers_Type",
				table: "Triggers",
				column: "Type");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Triggers_Type",
				table: "Triggers");

			migrationBuilder.DropColumn(
				name: "FormalLanguage",
				table: "Triggers");

			migrationBuilder.DropColumn(
				name: "Type",
				table: "Triggers");
		}
	}
}
