using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabSto.Data;
using LabSto.Filters;

namespace LabSto.Controllers
{
    public class StatisticsController(LabStoContext context, ILogger<StatisticsController> logger) : Controller
    {
        private readonly LabStoContext _context = context;
        private readonly ILogger<StatisticsController> _logger = logger;

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);

            var vm = new StatisticsVM
            {
                TotalReagents = await _context.Reagent.CountAsync(),
                TotalCabinets = await _context.Cabinet.CountAsync(),
                TotalStockRecords = await _context.StockRecord.CountAsync(),
                LowStockCount = await _context.Reagent.CountAsync(r => r.Stock <= r.MinStock && r.MinStock > 0),
                LowStockReagents = await _context.Reagent
                    .Where(r => r.Stock <= r.MinStock && r.MinStock > 0)
                    .Select(r => new LowStockReagentItem { Name = r.Name, Stock = r.Stock, MinStock = r.MinStock, Unit = r.Unit })
                    .ToListAsync(),

                ThisMonthRecords = await _context.StockRecord
                    .CountAsync(r => r.CreateTime >= startOfMonth),

                ThisWeekRecords = await _context.StockRecord
                    .CountAsync(r => r.CreateTime >= startOfWeek),

                TotalStockValue = await _context.StockRecord
                    .Where(r => r.Type == "入库")
                    .SumAsync(r => r.Quantity)
            };

            var monthRecords = await _context.StockRecord
                .Where(r => r.CreateTime >= startOfMonth)
                .GroupBy(r => r.Type)
                .Select(g => new { Type = g.Key, Count = g.Count(), TotalQty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            vm.MonthInCount = monthRecords.FirstOrDefault(r => r.Type == "入库")?.Count ?? 0;
            vm.MonthOutCount = monthRecords.FirstOrDefault(r => r.Type == "出库")?.Count ?? 0;

            var in30 = now.AddDays(-30);
            vm.StockIn30Days = await _context.StockRecord
                .Where(r => r.CreateTime >= in30 && r.Type == "入库")
                .CountAsync();
            vm.StockOut30Days = await _context.StockRecord
                .Where(r => r.CreateTime >= in30 && r.Type == "出库")
                .CountAsync();

            vm.CategoryStats = await _context.Reagent
                .GroupBy(r => r.Category)
                .Where(g => g.Key != "")
                .Select(g => new CategoryStatItem
                {
                    Category = g.Key,
                    Count = g.Count(),
                    TotalStock = g.Sum(x => x.Stock)
                })
                .OrderByDescending(c => c.Count)
                .ToListAsync();

            vm.TopReagents = await _context.StockRecord
                .Where(r => r.CreateTime >= startOfMonth)
                .GroupBy(r => r.Reagent.Name)
                .Select(g => new TopReagentItem
                {
                    Name = g.Key,
                    InCount = g.Count(r => r.Type == "入库"),
                    OutCount = g.Count(r => r.Type == "出库"),
                    TotalQty = g.Sum(r => r.Quantity)
                })
                .OrderByDescending(r => r.OutCount)
                .Take(5)
                .ToListAsync();

            vm.RecentRecords = await _context.StockRecord
                .Include(r => r.Reagent)
                .OrderByDescending(r => r.CreateTime)
                .Take(8)
                .Select(r => new RecentRecordItem
                {
                    ReagentName = r.Reagent.Name,
                    Type = r.Type,
                    Quantity = r.Quantity,
                    Operator = r.OperatorName,
                    CreateTime = r.CreateTime
                })
                .ToListAsync();

            vm.StockTrend = await _context.StockRecord
                .Where(r => r.CreateTime >= in30)
                .GroupBy(r => r.CreateTime.Date)
                .Select(g => new DailyStockItem
                {
                    Date = g.Key,
                    InCount = g.Count(r => r.Type == "入库"),
                    OutCount = g.Count(r => r.Type == "出库")
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            vm.CabinetUtilization = await _context.Cabinet
                .Select(c => new CabinetUtilItem
                {
                    CabinetNo = c.CabinetNo,
                    Location = c.Location,
                    StorageType = c.StorageType,
                    Capacity = c.Capacity,
                    UsedCount = c.Slots.Count(s => s.Status == "占用")
                })
                .OrderBy(c => c.CabinetNo)
                .ToListAsync();

            return View(vm);
        }
    }

    public class StatisticsVM
    {
        public int TotalReagents { get; set; }
        public int TotalCabinets { get; set; }
        public int TotalStockRecords { get; set; }
        public int LowStockCount { get; set; }
        public int ThisMonthRecords { get; set; }
        public int ThisWeekRecords { get; set; }
        public double TotalStockValue { get; set; }
        public int MonthInCount { get; set; }
        public int MonthOutCount { get; set; }
        public int StockIn30Days { get; set; }
        public int StockOut30Days { get; set; }
        public List<LowStockReagentItem> LowStockReagents { get; set; } = [];
        public List<CategoryStatItem> CategoryStats { get; set; } = [];
        public List<TopReagentItem> TopReagents { get; set; } = [];
        public List<RecentRecordItem> RecentRecords { get; set; } = [];
        public List<DailyStockItem> StockTrend { get; set; } = [];
        public List<CabinetUtilItem> CabinetUtilization { get; set; } = [];
    }

    public class CategoryStatItem
    {
        public string Category { get; set; } = "";
        public int Count { get; set; }
        public double TotalStock { get; set; }
    }

    public class TopReagentItem
    {
        public string Name { get; set; } = "";
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public double TotalQty { get; set; }
    }

    public class RecentRecordItem
    {
        public string ReagentName { get; set; } = "";
        public string Type { get; set; } = "";
        public double Quantity { get; set; }
        public string Operator { get; set; } = "";
        public DateTime CreateTime { get; set; }
    }

    public class DailyStockItem
    {
        public DateTime Date { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
    }

    public class CabinetUtilItem
    {
        public string CabinetNo { get; set; } = "";
        public string Location { get; set; } = "";
        public string StorageType { get; set; } = "";
        public int Capacity { get; set; }
        public int UsedCount { get; set; }
        public double UtilizationRate => Capacity > 0 ? UsedCount * 100.0 / Capacity : 0;
    }

    public class LowStockReagentItem
    {
        public string Name { get; set; } = "";
        public double Stock { get; set; }
        public double MinStock { get; set; }
        public string Unit { get; set; } = "";
    }
}
