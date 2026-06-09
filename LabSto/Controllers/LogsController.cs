using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LabSto.Controllers;

public class LogsController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LogsController> _logger;

    public LogsController(IWebHostEnvironment env, ILogger<LogsController> logger)
    {
        _env = env;
        _logger = logger;
    }

    [TypeFilter(typeof(Filters.CustomActionFilterAttribute))]
    public IActionResult Index(string? level, string? date, int page = 1)
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LabSto", "logs");

        if (!Directory.Exists(logDir))
        {
            return View(new LogsVM
            {
                LogFiles = [],
                SelectedLevel = level ?? "All",
                SelectedDate = date ?? "",
                CurrentPage = 1,
                TotalPages = 1,
                LogEntries = []
            });
        }

        var logFiles = Directory.GetFiles(logDir, "*.log")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(f => f != null)
            .Select(f => f!)
            .OrderByDescending(f => f)
            .ToList();

        var targetDate = string.IsNullOrEmpty(date)
            ? DateTime.Today.ToString("yyyy-MM-dd")
            : date;
        var logFilePath = Path.Combine(logDir, $"{targetDate}.log");

        var entries = new List<LogEntry>();

        if (System.IO.File.Exists(logFilePath))
        {
            var lines = System.IO.File.ReadAllLines(logFilePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var entry = ParseLine(line);
                if (entry == null) continue;

                if (!string.IsNullOrEmpty(level) && level != "All")
                {
                    if (!entry.Level.Equals(level, StringComparison.OrdinalIgnoreCase)) continue;
                }

                entries.Add(entry);
            }
        }

        var pageSize = 50;
        var totalPages = Math.Max(1, (int)Math.Ceiling(entries.Count / (double)pageSize));
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        entries = entries
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var vm = new LogsVM
        {
            LogFiles = logFiles,
            SelectedDate = targetDate,
            SelectedLevel = level ?? "All",
            CurrentPage = page,
            TotalPages = totalPages,
            LogEntries = entries
        };

        return View(vm);
    }

    public IActionResult Download(string date)
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LabSto", "logs");

        var logFilePath = Path.Combine(logDir, $"{date}.log");
        if (!System.IO.File.Exists(logFilePath))
            return NotFound();

        var bytes = System.IO.File.ReadAllBytes(logFilePath);
        return File(bytes, "text/plain", $"nlog-{date}.log");
    }

    private LogEntry? ParseLine(string line)
    {
        // Format: 2026-06-09 11:30:45.1234|INFO|LoggerName|Message
        var parts = line.Split('|');
        if (parts.Length < 3) return null;

        var entry = new LogEntry { RawLine = line };

        if (DateTime.TryParse(parts[0], out var dt))
            entry.Timestamp = dt;

        entry.Level = parts[1].Trim().ToUpper();
        entry.Logger = parts.Length > 2 ? parts[2].Trim() : "";

        if (parts.Length > 3)
            entry.Message = string.Join("|", parts.Skip(3));

        return entry;
    }

    public class LogsVM
    {
        public List<string> LogFiles { get; set; } = [];
        public List<LogEntry> LogEntries { get; set; } = [];
        public string SelectedDate { get; set; } = "";
        public string SelectedLevel { get; set; } = "All";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = "";
        public string Logger { get; set; } = "";
        public string Message { get; set; } = "";
        public string RawLine { get; set; } = "";
    }
}
