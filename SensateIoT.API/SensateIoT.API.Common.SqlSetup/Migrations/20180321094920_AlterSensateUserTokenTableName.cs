using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class AlterSensateUserTokenTableName : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropPrimaryKey(
				name: "PK_AspNetUserTokens",
				table: "AspNetUserTokens");

			migrationBuilder.DropUniqueConstraint(
				name: "AK_AspNetUserTokens_UserId_LoginProvider_Name",
				table: "AspNetUserTokens");

			migrationBuilder.DropColumn(
				name: "Discriminator",
				table: "AspNetUserTokens");

			migrationBuilder.DropColumn(
				name: "CreatedAt",
				table: "AspNetUserTokens");

			migrationBuilder.DropColumn(
				name: "ExpiresAt",
				table: "AspNetUserTokens");

			migrationBuilder.DropColumn(
				name: "Valid",
				table: "AspNetUserTokens");

			migrationBuilder.AlterColumn<string>(
				name: "Value",
				table: "AspNetUserTokens",
				nullable: true,
				oldClrType: typeof(string));

			migrationBuilder.AddPrimaryKey(
				name: "PK_AspNetUserTokens",
				table: "AspNetUserTokens",
				columns: new[] { "UserId", "LoginProvider", "Name" });

			migrationBuilder.CreateTable(
				name: "AspNetAuthTokens",
				columns: table => new {
					UserId = table.Column<string>(nullable: false),
					Value = table.Column<string>(nullable: false),
					CreatedAt = table.Column<DateTime>(nullable: false),
					ExpiresAt = table.Column<DateTime>(nullable: false),
					LoginProvider = table.Column<string>(nullable: true),
					Valid = table.Column<bool>(nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_AspNetAuthTokens", x => new { x.UserId, x.Value });
					table.ForeignKey(
						name: "FK_AspNetAuthTokens_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AspNetAuthTokens");

			migrationBuilder.DropPrimaryKey(
				name: "PK_AspNetUserTokens",
				table: "AspNetUserTokens");

			migrationBuilder.AlterColumn<string>(
				name: "Value",
				table: "AspNetUserTokens",
				nullable: false,
				oldClrType: typeof(string),
				oldNullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Discriminator",
				table: "AspNetUserTokens",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<DateTime>(
				name: "CreatedAt",
				table: "AspNetUserTokens",
				nullable: true);

			migrationBuilder.AddColumn<DateTime>(
				name: "ExpiresAt",
				table: "AspNetUserTokens",
				nullable: true);

			migrationBuilder.AddColumn<bool>(
				name: "Valid",
				table: "AspNetUserTokens",
				nullable: true);

			migrationBuilder.AddPrimaryKey(
				name: "PK_AspNetUserTokens",
				table: "AspNetUserTokens",
				columns: new[] { "Value", "UserId" });

			migrationBuilder.AddUniqueConstraint(
				name: "AK_AspNetUserTokens_UserId_LoginProvider_Name",
				table: "AspNetUserTokens",
				columns: new[] { "UserId", "LoginProvider", "Name" });
		}
	}
}
