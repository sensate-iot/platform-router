using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SensateIoT.API.SqlSetup.Migrations
{
	public partial class CreateBlobsTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Blobs",
				columns: table => new {
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					SensorId = table.Column<string>(maxLength: 24, nullable: false),
					FileName = table.Column<string>(nullable: false),
					Path = table.Column<string>(nullable: false),
					StorageType = table.Column<int>(nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_Blobs", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Blobs_SensorId",
				table: "Blobs",
				column: "SensorId");

			migrationBuilder.CreateIndex(
				name: "IX_Blobs_SensorId_FileName",
				table: "Blobs",
				columns: new[] { "SensorId", "FileName" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Blobs");
		}
	}
}
