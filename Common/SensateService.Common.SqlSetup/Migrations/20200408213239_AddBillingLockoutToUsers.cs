using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
	public partial class AddBillingLockoutToUsers : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "BillingLockout",
				table: "Users",
				nullable: false,
				defaultValue: false);

			migrationBuilder.CreateIndex(
				name: "IX_Users_BillingLockout",
				table: "Users",
				column: "BillingLockout");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Users_BillingLockout",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "BillingLockout",
				table: "Users");
		}
	}
}
