using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBenchmarkRunIdColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BenchmarkRunId",
                table: "benchmark_records",
                newName: "benchmark_run_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "benchmark_run_id",
                table: "benchmark_records",
                newName: "BenchmarkRunId");
        }
    }
}
