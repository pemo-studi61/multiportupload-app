using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPortUpload.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredArtifactsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Die Tabelle wird ggf. außerhalb der Migrationen (manuell) angelegt; daher
            // idempotent per IF NOT EXISTS, damit die Migration in jedem Fall sauber
            // durchläuft und nur im __EFMigrationsHistory vermerkt wird.
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS public.stored_artifacts (
                    id                  uuid not null,
                    original_file_name  varchar(255) not null,
                    stored_file_name    varchar(255) not null,
                    mime_type           varchar(100) not null,
                    file_extension      varchar(20)  not null,
                    size_in_bytes       bigint not null check (size_in_bytes >= 0),
                    sha256              varchar(64)  not null,
                    storage_path        text not null,
                    created_at_utc      timestamp with time zone not null default now(),
                    constraint stored_artifacts_pkey primary key (id)
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.stored_artifacts;");
        }
    }
}
