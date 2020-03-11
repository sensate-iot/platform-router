using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SensateService.SqlSetup.Migrations
{
	public partial class FirstLastNameNotNullable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "LastName",
				table: "AspNetUsers",
				nullable: false,
				oldClrType: typeof(string),
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "FirstName",
				table: "AspNetUsers",
				nullable: false,
				oldClrType: typeof(string),
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "LastName",
				table: "AspNetUsers",
				nullable: true,
				oldClrType: typeof(string));

			migrationBuilder.AlterColumn<string>(
				name: "FirstName",
				table: "AspNetUsers",
				nullable: true,
				oldClrType: typeof(string));
		}
	}
}
