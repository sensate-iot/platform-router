using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SensateService.SqlSetup.Migrations
{
	public partial class AddSensateUserToken : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
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
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
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
		}
	}
}
