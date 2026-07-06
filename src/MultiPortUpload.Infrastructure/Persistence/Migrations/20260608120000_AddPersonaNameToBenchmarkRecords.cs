using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonaNameToBenchmarkRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Die Spalte wurde produktiv ggf. bereits manuell angelegt. Damit das
            // Anwenden dieser Migration auf einer solchen Datenbank nicht
            // fehlschlägt, wird die Spalte idempotent hinzugefügt.
            migrationBuilder.Sql(
                "ALTER TABLE benchmark_records ADD COLUMN IF NOT EXISTS persona_name character varying(128);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "persona_name",
                table: "benchmark_records");
        }
    }
}
