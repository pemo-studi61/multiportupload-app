using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KeepBenchmarkrunIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'benchmarkrun_id'
                    ) THEN
                        ALTER TABLE benchmark_records ADD COLUMN benchmarkrun_id uuid NULL;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'benchmarkrun_id'
                    ) THEN
                        ALTER TABLE benchmark_records DROP COLUMN benchmarkrun_id;
                    END IF;
                END $$;
                """);
        }
    }
}
