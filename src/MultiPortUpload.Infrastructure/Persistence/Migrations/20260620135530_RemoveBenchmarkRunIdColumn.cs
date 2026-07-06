using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBenchmarkRunIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'benchmark_run_id'
                    ) THEN
                        ALTER TABLE benchmark_records DROP COLUMN benchmark_run_id;
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
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'benchmark_run_id'
                    ) THEN
                        ALTER TABLE benchmark_records ADD COLUMN benchmark_run_id uuid NULL;
                    END IF;
                END $$;
                """);
        }
    }
}
