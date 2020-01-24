using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
    public partial class AddUnconfirmedPhoneNumberToSensateUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnconfirmedPhoneNumber",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnconfirmedPhoneNumber",
                table: "AspNetUsers");
        }
    }
}
