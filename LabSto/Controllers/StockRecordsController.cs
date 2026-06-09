using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabSto.Data;
using LabSto.Models.Entities;
using LabSto.Filters;

namespace LabSto.Controllers
{
    public class StockRecordsController(LabStoContext context, ILogger<StockRecordsController> logger) : Controller
    {
        private readonly LabStoContext _context = context;
        private readonly ILogger<StockRecordsController> _logger = logger;
        private static readonly string[] items = ["入库", "出库"];
        private static readonly string[] itemsArray = ["入库", "出库"];
        private static readonly string[] itemsArray0 = ["入库", "出库"];
        private static readonly string[] itemsArray1 = ["入库", "出库"];

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Index(
            string? searchString,
            string? typeFilter,
            DateTime? startDate,
            DateTime? endDate,
            int page = 1)
        {
            var query = _context.StockRecord
                .Include(s => s.Reagent)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.Reagent.Name.Contains(searchString) ||
                    s.OperatorName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                query = query.Where(s => s.Type == typeFilter);
            }

            if (startDate.HasValue)
            {
                var sd = startDate.Value;
                query = query.Where(s => s.CreateTime >= sd);
            }

            if (endDate.HasValue)
            {
                var ed = endDate.Value.AddDays(1);
                query = query.Where(s => s.CreateTime <= ed);
            }

            int pageSize = 15;
            var total = await query.CountAsync();
            var records = await query
                .OrderByDescending(s => s.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            ViewData["TypeFilter"] = typeFilter;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["PageNum"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);
            ViewData["TotalCount"] = total;

            return View(records);
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.StockRecord
                .Include(s => s.Reagent)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null) return NotFound();
            return View(record);
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Create()
        {
            ViewData["ReagentId"] = new SelectList(
                _context.Reagent.Where(r => r.Stock > 0),
                "Id", "Name");
            ViewData["TypeOptions"] = new SelectList(items);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Create(
            [Bind("ReagentId,Type,Quantity,Remark")] StockRecord record)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReagentId"] = new SelectList(
                    _context.Reagent.Where(r => r.Stock > 0), "Id", "Name", record.ReagentId);
                ViewData["TypeOptions"] = new SelectList(itemsArray);
                return View(record);
            }

            var reagent = await _context.Reagent.FindAsync(record.ReagentId);
            if (reagent == null)
            {
                ModelState.AddModelError("ReagentId", "试剂不存在");
                return View(record);
            }

            if (record.Type == "出库" && reagent.Stock < record.Quantity)
            {
                ModelState.AddModelError("Quantity", $"库存不足，当前库存：{reagent.Stock} {reagent.Unit}");
                ViewData["ReagentId"] = new SelectList(
                    _context.Reagent.Where(r => r.Stock > 0), "Id", "Name", record.ReagentId);
                return View(record);
            }

            record.OperatorName = User.Identity?.Name ?? "系统";
            record.CreateTime = DateTime.Now;

            _context.Add(record);

            if (record.Type == "入库")
                reagent.Stock += record.Quantity;
            else
                reagent.Stock -= record.Quantity;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"记录已创建，{reagent.Name} 库存更新为 {reagent.Stock} {reagent.Unit}";
            return RedirectToAction(nameof(Index));
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.StockRecord.FindAsync(id);
            if (record == null) return NotFound();

            ViewData["ReagentId"] = new SelectList(_context.Reagent, "Id", "Name", record.ReagentId);
            ViewData["TypeOptions"] = new SelectList(itemsArray0);
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,ReagentId,Type,Quantity,OperatorName,Remark,CreateTime")] StockRecord record)
        {
            if (id != record.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["ReagentId"] = new SelectList(_context.Reagent, "Id", "Name", record.ReagentId);
                ViewData["TypeOptions"] = new SelectList(itemsArray1);
                return View(record);
            }

            var original = await _context.StockRecord.AsNoTracking().FirstAsync(r => r.Id == id);
            var reagent = await _context.Reagent.FindAsync(record.ReagentId);
            if (reagent == null) return NotFound();

            if (record.Type == "出库")
            {
                double delta = original.Type == "入库"
                    ? original.Quantity + record.Quantity
                    : record.Quantity - original.Quantity;
                if (reagent.Stock < delta)
                {
                    ModelState.AddModelError("Quantity", $"库存不足，当前库存：{reagent.Stock} {reagent.Unit}");
                    ViewData["ReagentId"] = new SelectList(_context.Reagent, "Id", "Name", record.ReagentId);
                    return View(record);
                }
            }

            try
            {
                if (original.Type == "入库")
                    reagent.Stock -= original.Quantity;
                else
                    reagent.Stock += original.Quantity;

                if (record.Type == "入库")
                    reagent.Stock += record.Quantity;
                else
                    reagent.Stock -= record.Quantity;

                _context.Update(record);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockRecordExists(record.Id)) return NotFound();
                else throw;
            }

            TempData["Success"] = "记录已更新";
            return RedirectToAction(nameof(Index));
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.StockRecord
                .Include(s => s.Reagent)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null) return NotFound();
            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.StockRecord.FindAsync(id);
            if (record != null)
            {
                var reagent = await _context.Reagent.FindAsync(record.ReagentId);
                if (reagent != null)
                {
                    if (record.Type == "入库")
                        reagent.Stock -= record.Quantity;
                    else
                        reagent.Stock += record.Quantity;
                }

                _context.StockRecord.Remove(record);
                await _context.SaveChangesAsync();
                TempData["Success"] = "记录已删除，库存已回滚";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StockRecordExists(int id)
        {
            return _context.StockRecord.Any(e => e.Id == id);
        }
    }
}
