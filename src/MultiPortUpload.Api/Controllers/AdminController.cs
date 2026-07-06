// file: src/MultiPortUpload.Api/Controllers/AdminController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiPortUpload.Infrastructure.Persistence;

namespace MultiPortUpload.Api.Controllers;

public class AdminSummaryDto
{
    public int BenchmarkRuns { get; set; }
    public int UploadAdapters { get; set; }
    public int LogFiles { get; set; }
    public int ErrorsToday { get; set; }
}

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly BenchmarkDbContext _dbContext;
    private readonly ILogger<AdminController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AdminController(
        BenchmarkDbContext dbContext,
        ILogger<AdminController> logger,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var benchmarkRuns = 0;
        try
        {
            benchmarkRuns = await _dbContext.BenchmarkRecords.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Benchmark summary count could not be loaded. Returning 0 as fallback.");
        }

        var logFiles = 0;
        try
        {
            var logDirectory = ResolveLogDirectory();
            if (Directory.Exists(logDirectory))
            {
                logFiles = Directory.EnumerateFiles(logDirectory, "*.log", SearchOption.TopDirectoryOnly).Count();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Log file count could not be loaded. Returning 0 as fallback.");
        }

        var summary = new AdminSummaryDto
        {
            BenchmarkRuns = benchmarkRuns,
            UploadAdapters = 6,
            LogFiles = logFiles,
            ErrorsToday = 0
        };

    return Ok(summary);
    }

    private string ResolveLogDirectory()
    {
        var configuredRoot = Environment.GetEnvironmentVariable("MPU_LOG_ROOT");
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            return Path.Combine(configuredRoot, "logs");
        }

        var contentRootLogs = Path.Combine(_environment.ContentRootPath, "logs");
        if (Directory.Exists(contentRootLogs))
        {
            return contentRootLogs;
        }

        // Local fallback for NLog default: ${currentdir}/../.. + /logs
        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "logs"));
    }

    [HttpGet("benchmarks")]
    public IActionResult GetBenchmarks()
    {
        try
        {
            var benchmarks = _dbContext.BenchmarkRecords
                .OrderByDescending(x => x.StartedAtUtc)
                .Take(50)
                .Select(x => new
                {
                    x.Id,
                    x.ArtifactId,
                    x.StartedAtUtc,
                    x.FinishedAtUtc,
                    x.UploadVariant,
                    x.PersonaName,
                    x.OriginalFileName,
                    x.SizeInBytes,
                    x.DurationInMilliseconds
                })
                .ToList();

            return Ok(benchmarks);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Benchmark list could not be loaded. Returning empty list as fallback.");
            return Ok(Array.Empty<object>());
        }
    }


    [HttpGet("benchmarks/{id}")]
    public async Task<IActionResult> GetBenchmark(Guid id)
    {
        var benchmark = await _dbContext.BenchmarkRecords
            .FirstOrDefaultAsync(x => x.Id == id);

        if (benchmark == null)
        {
            return NotFound();
        }

        return Ok(benchmark);
    }

}

