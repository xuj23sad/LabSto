using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabSto.Data;
using LabSto.Models.Entities;
using Newtonsoft.Json;
using LabSto.Filters;

namespace LabSto.Controllers
{
    public class CabinetsController(LabStoContext context, ILogger<CabinetsController> logger) : Controller
    {
        private readonly LabStoContext _context = context;
        private readonly ILogger<CabinetsController> _logger = logger;

        // GET: Cabinets
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cabinet.ToListAsync());
        }

        // GET: Cabinets/Details/5
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cabinet = await _context.Cabinet
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cabinet == null)
            {
                return NotFound();
            }

            return View(cabinet);
        }

        // GET: Cabinets/Create
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cabinets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Create([Bind("Id,CabinetNo,Location,Capacity,StorageType,Status")] Cabinet cabinet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cabinet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cabinet);
        }

        // GET: Cabinets/Edit/5
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cabinet = await _context.Cabinet.FindAsync(id);
            if (cabinet == null)
            {
                return NotFound();
            }
            return View(cabinet);
        }

        // POST: Cabinets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CabinetNo,Location,Capacity,StorageType,Status")] Cabinet cabinet)
        {
            if (id != cabinet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cabinet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CabinetExists(cabinet.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cabinet);
        }

        // GET: Cabinets/Delete/5
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cabinet = await _context.Cabinet
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cabinet == null)
            {
                return NotFound();
            }

            return View(cabinet);
        }

        // POST: Cabinets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cabinet = await _context.Cabinet.FindAsync(id);
            if (cabinet != null)
            {
                _context.Cabinet.Remove(cabinet);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CabinetExists(int id)
        {
            return _context.Cabinet.Any(e => e.Id == id);
        }
    }
}
