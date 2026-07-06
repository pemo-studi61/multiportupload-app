using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationAndCommentToBenchmarkRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent, falls die Spalten ggf. außerhalb der Migrationen ergänzt wurden.
            migrationBuilder.Sql(
                """
                ALTER TABLE public.benchmark_runs ADD COLUMN IF NOT EXISTS location text NULL;
                ALTER TABLE public.benchmark_runs ADD COLUMN IF NOT EXISTS comment  text NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public.benchmark_runs DROP COLUMN IF EXISTS comment;
                ALTER TABLE public.benchmark_runs DROP COLUMN IF EXISTS location;
                """);
        }
    }
}
