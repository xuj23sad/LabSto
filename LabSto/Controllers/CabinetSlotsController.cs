using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabSto.Data;
using LabSto.Models.Entities;
using LabSto.Filters;

namespace LabSto.Controllers
{
    public class CabinetSlotsController : Controller
    {
        private readonly LabStoContext _context;
        private readonly ILogger<CabinetSlotsController> _logger;

        public CabinetSlotsController(LabStoContext context, ILogger<CabinetSlotsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Index(int? cabinetId)
        {
            if (cabinetId.HasValue)
            {
                var cabinet = await _context.Cabinet
                    .Include(c => c.Slots)
                        .ThenInclude(s => s.Reagent)
                    .FirstOrDefaultAsync(c => c.Id == cabinetId.Value);

                if (cabinet == null) return NotFound();
                ViewData["CabinetName"] = cabinet.CabinetNo;
                ViewData["CabinetId"] = cabinet.Id;
                return View(cabinet.Slots.OrderBy(s => s.SlotNo).ToList());
            }

            var allSlots = await _context.CabinetSlot
                .Include(s => s.Cabinet)
                .Include(s => s.Reagent)
                .OrderBy(s => s.CabinetId)
                .ThenBy(s => s.SlotNo)
                .ToListAsync();

            return View(allSlots);
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Manage(int cabinetId)
        {
            var cabinet = await _context.Cabinet
                .Include(c => c.Slots)
                    .ThenInclude(s => s.Reagent)
                .FirstOrDefaultAsync(c => c.Id == cabinetId);

            if (cabinet == null) return NotFound();

            ViewData["CabinetId"] = cabinet.Id;
            ViewData["CabinetNo"] = cabinet.CabinetNo;
            ViewData["Reagents"] = await _context.Reagent.OrderBy(r => r.Name).ToListAsync();

            return View(cabinet);
        }

        [HttpPost]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> AssignReagent(int slotId, int? reagentId)
        {
            var slot = await _context.CabinetSlot.FindAsync(slotId);
            if (slot == null) return NotFound();

            if (reagentId.HasValue)
            {
                slot.ReagentId = reagentId;
                slot.Status = "占用";
            }
            else
            {
                slot.ReagentId = null;
                slot.Status = "空置";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = reagentId.HasValue ? "试剂已放置到该格位" : "格位已清空";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> InitializeSlots(int cabinetId)
        {
            var cabinet = await _context.Cabinet.FindAsync(cabinetId);
            if (cabinet == null) return NotFound();

            var existing = await _context.CabinetSlot.CountAsync(s => s.CabinetId == cabinetId);
            if (existing > 0)
            {
                TempData["Error"] = "该试剂柜已有格位，无需初始化";
                return RedirectToAction("Index", "Cabinets");
            }

            for (int i = 1; i <= cabinet.Capacity; i++)
            {
                var slot = new CabinetSlot
                {
                    CabinetId = cabinetId,
                    SlotNo = i.ToString(),
                    Status = "空置"
                };
                _context.CabinetSlot.Add(slot);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"已为 {cabinet.CabinetNo} 初始化 {cabinet.Capacity} 个格位";
            return RedirectToAction("Manage", new { cabinetId });
        }
    }
}
