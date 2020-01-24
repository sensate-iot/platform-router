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

            migrationBuilder.CreateTable(
                name: "TriggerInvocations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TriggerId = table.Column<long>(nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(nullable: false),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
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
