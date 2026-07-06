using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameAdapterNameToUploadVariant : Migration
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
                        WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'adapter_name'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'upload_variant'
                    ) THEN
                        ALTER TABLE benchmark_records RENAME COLUMN adapter_name TO upload_variant;
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
                        WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'upload_variant'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'adapter_name'
                    ) THEN
                        ALTER TABLE benchmark_records RENAME COLUMN upload_variant TO adapter_name;
                    END IF;
                END $$;
                """);
        }
    }
}
