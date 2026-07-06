using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using IoFile = System.IO.File;

namespace MultiPortUpload.Api.Controllers
{

    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        private readonly string _logDirectory = "logs";

        [HttpGet]
        public IActionResult GetLogs([FromQuery] string? date)
        {
            if (!Directory.Exists(_logDirectory))
            {
                return Ok(Array.Empty<object>());
            }
            DateTime? filterDate = null;
            if (!string.IsNullOrEmpty(date))
            {
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    return BadRequest("Invalid date format. Use 'yyyy-MM-dd'.");
                }
                filterDate = parsedDate;
            }

            var logFiles = Directory.GetFiles(_logDirectory, "*.log")
                .Where(file => filterDate == null ||
                    IoFile.GetLastWriteTime(file).Date == filterDate.Value.Date)
                .Select(file =>
                {
                    var info = new FileInfo(file);

                    return new
                    {
                        FileName = info.Name,
                        LastModified = info.LastWriteTime,
                        SizeBytes = info.Length
                    };
                })
                .OrderByDescending(file => file.LastModified)
                .ToList();
            return Ok(logFiles);
        }

        [HttpGet("{fileName}")]
        public IActionResult GetLogFileContent(string fileName, [FromQuery] int tail = 200)
        {
            var safeFileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(_logDirectory, safeFileName);

            if (!IoFile.Exists(filePath))
            {
                return NotFound("Log file not found.");
            }

            var lines = IoFile.ReadLines(filePath);

            if (tail > 0)
            {
                lines = lines.TakeLast(tail);
            }

            return Ok(new
            {
                FileName = safeFileName,
                Tail = tail,
                Content = lines.ToList()
            });
        }

        [HttpGet("{fileName}/download")]
        public IActionResult DownloadLogFile(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(_logDirectory, safeFileName);

            if (!IoFile.Exists(filePath))
            {
                return NotFound("Log file not found.");
            }

            var bytes = IoFile.ReadAllBytes(filePath);

            return File(bytes, "text/plain", safeFileName);
        }
    }
}