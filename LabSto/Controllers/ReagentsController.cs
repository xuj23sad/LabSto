using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabSto.Data;
using LabSto.Models.Entities;
using LabSto.Filters;

namespace LabSto.Controllers
{
    public class ReagentsController(LabStoContext context, ILogger<ReagentsController> logger) : Controller
    {
        private readonly LabStoContext _context = context;
        private readonly ILogger<ReagentsController> _logger = logger;

        // GET: Reagents
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Reagent.ToListAsync());
        }

        // GET: Reagents/Details/5
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reagent = await _context.Reagent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reagent == null)
            {
                return NotFound();
            }

            return View(reagent);
        }

        // GET: Reagents/Create
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reagents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Create([Bind("Id,Name,CASNo,Category,DangerLevel,Unit,Stock,MinStock,Remark,CreateTime")] Reagent reagent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reagent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(reagent);
        }

        // GET: Reagents/Edit/5
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reagent = await _context.Reagent.FindAsync(id);
            if (reagent == null)
            {
                return NotFound();
            }
            return View(reagent);
        }

        // POST: Reagents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CASNo,Category,DangerLevel,Unit,Stock,MinStock,Remark,CreateTime")] Reagent reagent)
        {
            if (id != reagent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reagent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReagentExists(reagent.Id))
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
            return View(reagent);
        }

        // GET: Reagents/Delete/5
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reagent = await _context.Reagent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reagent == null)
            {
                return NotFound();
            }

            return View(reagent);
        }

        // POST: Reagents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(CustomActionFilterAttribute))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reagent = await _context.Reagent.FindAsync(id);
            if (reagent != null)
            {
                _context.Reagent.Remove(reagent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReagentExists(int id)
        {
            return _context.Reagent.Any(e => e.Id == id);
        }
    }
}
