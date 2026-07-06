using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class NormalizeLegacyBenchmarkColumnNames : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.tables
                    WHERE table_schema = 'public' AND table_name = 'BenchmarkRecords'
                ) THEN
                    ALTER TABLE "BenchmarkRecords" RENAME TO benchmark_records;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'UploadVariant'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'upload_variant'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "UploadVariant" TO upload_variant;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'ArtifactId'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'artifact_id'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "ArtifactId" TO artifact_id;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'OriginalFileName'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'original_file_name'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "OriginalFileName" TO original_file_name;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'SizeInBytes'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'size_in_bytes'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "SizeInBytes" TO size_in_bytes;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'StartedAtUtc'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'started_at_utc'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "StartedAtUtc" TO started_at_utc;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'FinishedAtUtc'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'finished_at_utc'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "FinishedAtUtc" TO finished_at_utc;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'DurationInMilliseconds'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'duration_ms'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "DurationInMilliseconds" TO duration_ms;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'PersonaName'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'persona_name'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "PersonaName" TO persona_name;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'BenchmarkRunId'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'benchmark_run_id'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN "BenchmarkRunId" TO benchmark_run_id;
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
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'benchmark_run_id'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'BenchmarkRunId'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN benchmark_run_id TO "BenchmarkRunId";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'persona_name'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'PersonaName'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN persona_name TO "PersonaName";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'duration_ms'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'DurationInMilliseconds'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN duration_ms TO "DurationInMilliseconds";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'finished_at_utc'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'FinishedAtUtc'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN finished_at_utc TO "FinishedAtUtc";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'started_at_utc'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'StartedAtUtc'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN started_at_utc TO "StartedAtUtc";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'size_in_bytes'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'SizeInBytes'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN size_in_bytes TO "SizeInBytes";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'original_file_name'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'OriginalFileName'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN original_file_name TO "OriginalFileName";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'artifact_id'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'ArtifactId'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN artifact_id TO "ArtifactId";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'upload_variant'
                ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'benchmark_records' AND column_name = 'UploadVariant'
                ) THEN
                    ALTER TABLE benchmark_records RENAME COLUMN upload_variant TO "UploadVariant";
                END IF;
            END $$;
            """);
    }
}
