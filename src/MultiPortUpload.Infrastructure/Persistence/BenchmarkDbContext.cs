// file: src/MultiPortUpload.Infrastructure/Persistence/BenchmarkDbContext.cs

using Microsoft.EntityFrameworkCore;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Infrastructure.Persistence;

/// <summary>
/// EF-Core-DbContext für die Benchmark-Persistenz in PostgreSQL.
/// </summary>
public sealed class BenchmarkDbContext : DbContext
{
    public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options)
        : base(options)
    {
    }

    public DbSet<BenchmarkRecord> BenchmarkRecords => Set<BenchmarkRecord>();

    public DbSet<BenchmarkRun> BenchmarkRuns => Set<BenchmarkRun>();

    public DbSet<StoredArtifact> StoredArtifacts => Set<StoredArtifact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StoredArtifact>(entity =>
        {
            entity.ToTable("stored_artifacts");

            entity.HasKey(e => e.Id);

            // Die Id wird von der Datenbank generiert (gen_random_uuid()), damit der
            // erzeugte Wert anschließend als artifact_id im Benchmark-Datensatz dient.
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.OriginalFileName)
                .HasColumnName("original_file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.StoredFileName)
                .HasColumnName("stored_file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.MimeType)
                .HasColumnName("mime_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.FileExtension)
                .HasColumnName("file_extension")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.SizeInBytes)
                .HasColumnName("size_in_bytes");

            entity.Property(e => e.Sha256)
                .HasColumnName("sha256")
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.StoragePath)
                .HasColumnName("storage_path")
                .IsRequired();

            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<BenchmarkRun>(entity =>
        {
            entity.ToTable("benchmark_runs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.RunnerName)
                .HasColumnName("runner_name");

            entity.Property(e => e.RunningTime)
                .HasColumnName("running_time");

            entity.Property(e => e.Location)
                .HasColumnName("location");

            entity.Property(e => e.Comment)
                .HasColumnName("comment");

            entity.Property(e => e.RunUuid)
                .HasColumnName("run_uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<BenchmarkRecord>(entity =>
        {
            entity.ToTable("benchmark_records");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.BenchmarkRunId)
                .HasColumnName("benchmarkrun_id");

            entity.Property(e => e.ArtifactId)
                .HasColumnName("artifact_id")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(e => e.UploadVariant)
                .HasColumnName("upload_variant")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.PersonaName)
                .HasColumnName("persona_name")
                .HasMaxLength(128);

            entity.Property(e => e.OriginalFileName)
                .HasColumnName("original_file_name")
                .HasMaxLength(1024)
                .IsRequired();

            entity.Property(e => e.SizeInBytes)
                .HasColumnName("size_in_bytes");

            entity.Property(e => e.StartedAtUtc)
                .HasColumnName("started_at_utc");

            entity.Property(e => e.FinishedAtUtc)
                .HasColumnName("finished_at_utc");

            entity.Property(e => e.DurationInMilliseconds)
                .HasColumnName("duration_ms");

            // Berechnete Eigenschaft – nicht persistiert.
            entity.Ignore(e => e.ThroughputMbPerSecond);

            entity.HasIndex(e => e.UploadVariant)
                .HasDatabaseName("ix_benchmark_records_upload_variant");

            entity.HasIndex(e => e.StartedAtUtc)
                .HasDatabaseName("ix_benchmark_records_started_at_utc");
        });
    }
}
