using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SensateService.SqlSetup.Migrations
{
	public partial class AddTriggerInvocationsTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Triggers_LastTriggered",
				table: "Triggers");

			migrationBuilder.DropColumn(
				name: "LastTriggered",
				table: "Triggers");

			migrationBuilder.AddColumn<string>(
				name: "Message",
				table: "Triggers",
				maxLength: 300,
				nullable: false,
				defaultValue: "");

			migrationBuilder.CreateTable(
				name: "TriggerInvocations",
				columns: table => new {
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					MeasurementBucketId = table.Column<string>(maxLength: 24, nullable: true),
					MeasurementId = table.Column<int>(nullable: false),
					TriggerId = table.Column<long>(nullable: false),
					Timestamp = table.Column<DateTimeOffset>(nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_TriggerInvocations", x => x.Id);
					table.ForeignKey(
						name: "FK_TriggerInvocations_Triggers_TriggerId",
						column: x => x.TriggerId,
						principalTable: "Triggers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_TriggerInvocations_TriggerId",
				table: "TriggerInvocations",
				column: "TriggerId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "TriggerInvocations");

			migrationBuilder.DropColumn(
				name: "Message",
				table: "Triggers");

			migrationBuilder.AddColumn<DateTime>(
				name: "LastTriggered",
				table: "Triggers",
				type: "timestamp without time zone",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

			migrationBuilder.CreateIndex(
				name: "IX_Triggers_LastTriggered",
				table: "Triggers",
				column: "LastTriggered");
		}
	}
}
