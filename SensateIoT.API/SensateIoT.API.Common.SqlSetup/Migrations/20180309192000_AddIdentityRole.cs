using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class AddIdentityRole : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Discriminator",
				table: "AspNetUsers");

			migrationBuilder.AddColumn<string>(
				name: "Description",
				table: "AspNetRoles",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Description",
				table: "AspNetRoles");

			migrationBuilder.AddColumn<string>(
				name: "Discriminator",
				table: "AspNetUsers",
				nullable: false,
				defaultValue: "");
		}
	}
}
