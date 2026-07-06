using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetStoredArtifactsIdDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Die Id der stored_artifacts wird künftig von der Datenbank erzeugt
            // (gen_random_uuid()); idempotent, falls der Default bereits gesetzt wurde.
            migrationBuilder.Sql(
                """
                ALTER TABLE public.stored_artifacts
                    ALTER COLUMN id SET DEFAULT gen_random_uuid();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public.stored_artifacts
                    ALTER COLUMN id DROP DEFAULT;
                """);
        }
    }
}
