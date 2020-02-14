using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateService.SqlSetup.Migrations
{
    public partial class RemoveMeasurementIndexFromTriggerInvocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_TriggerInvocations_MeasurementBucketId_MeasurementId_Trigge~",
                table: "TriggerInvocations");

            migrationBuilder.DropColumn(
                name: "MeasurementBucketId",
                table: "TriggerInvocations");

            migrationBuilder.DropColumn(
                name: "MeasurementId",
                table: "TriggerInvocations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeasurementBucketId",
                table: "TriggerInvocations",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MeasurementId",
                table: "TriggerInvocations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_TriggerInvocations_MeasurementBucketId_MeasurementId_Trigge~",
                table: "TriggerInvocations",
                columns: new[] { "MeasurementBucketId", "MeasurementId", "TriggerId" });
        }
    }
}
