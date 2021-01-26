using Microsoft.EntityFrameworkCore.Migrations;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class AddAlternateKeyToTriggerInvocations : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "MeasurementBucketId",
				table: "TriggerInvocations",
				maxLength: 24,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(24)",
				oldMaxLength: 24,
				oldNullable: true);

			migrationBuilder.AddUniqueConstraint(
				name: "AK_TriggerInvocations_MeasurementBucketId_MeasurementId_Trigge~",
				table: "TriggerInvocations",
				columns: new[] { "MeasurementBucketId", "MeasurementId", "TriggerId" });
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropUniqueConstraint(
				name: "AK_TriggerInvocations_MeasurementBucketId_MeasurementId_Trigge~",
				table: "TriggerInvocations");

			migrationBuilder.AlterColumn<string>(
				name: "MeasurementBucketId",
				table: "TriggerInvocations",
				type: "character varying(24)",
				maxLength: 24,
				nullable: true,
				oldClrType: typeof(string),
				oldMaxLength: 24);
		}
	}
}
