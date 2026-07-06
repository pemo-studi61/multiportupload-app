using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialBenchmarkSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "benchmark_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    artifact_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    upload_variant = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_benchmark_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_benchmark_records_started_at_utc",
                table: "benchmark_records",
                column: "started_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_benchmark_records_upload_variant",
                table: "benchmark_records",
                column: "upload_variant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "benchmark_records");
        }
    }
}
