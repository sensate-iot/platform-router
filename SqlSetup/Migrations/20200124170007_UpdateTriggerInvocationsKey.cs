using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SensateService.SqlSetup.Migrations
{
    public partial class UpdateTriggerInvocationsKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TriggerInvocations",
                table: "TriggerInvocations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TriggerInvocations");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TriggerInvocations");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Triggers",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MeasurementBucketId",
                table: "TriggerInvocations",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MeasurementId",
                table: "TriggerInvocations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TriggerInvocations",
                table: "TriggerInvocations",
                columns: new[] { "MeasurementBucketId", "MeasurementId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TriggerInvocations",
                table: "TriggerInvocations");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Triggers");

            migrationBuilder.DropColumn(
                name: "MeasurementBucketId",
                table: "TriggerInvocations");

            migrationBuilder.DropColumn(
                name: "MeasurementId",
                table: "TriggerInvocations");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "TriggerInvocations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TriggerInvocations",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TriggerInvocations",
                table: "TriggerInvocations",
                column: "Id");
        }
    }
}
