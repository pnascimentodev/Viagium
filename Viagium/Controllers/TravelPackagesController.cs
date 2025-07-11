using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;

namespace Viagium.Controllers
{
    public class TravelPackagesController : Controller
    {
        private readonly ViagiumContext _context;

        public TravelPackagesController(ViagiumContext context)
        {
            _context = context;
        }

        // GET: TravelPackages
        public async Task<IActionResult> Index()
        {
            return View(await _context.TravelPackages.ToListAsync());
        }

        // GET: TravelPackages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelPackages = await _context.TravelPackages
                .FirstOrDefaultAsync(m => m.TravelPackagesId == id);
            if (travelPackages == null)
            {
                return NotFound();
            }

            return View(travelPackages);
        }

        // GET: TravelPackages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TravelPackages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TravelPackagesId,Title,ImageUrl,Description,Destination,Duration,Price,CreatedAt,UpdatedAt,IsActived,DeletedAt,CreatedByEmployeeId")] TravelPackages travelPackages)
        {
            if (ModelState.IsValid)
            {
                _context.Add(travelPackages);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(travelPackages);
        }

        // GET: TravelPackages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelPackages = await _context.TravelPackages.FindAsync(id);
            if (travelPackages == null)
            {
                return NotFound();
            }
            return View(travelPackages);
        }

        // POST: TravelPackages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TravelPackagesId,Title,ImageUrl,Description,Destination,Duration,Price,CreatedAt,UpdatedAt,IsActived,DeletedAt,CreatedByEmployeeId")] TravelPackages travelPackages)
        {
            if (id != travelPackages.TravelPackagesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(travelPackages);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TravelPackagesExists(travelPackages.TravelPackagesId))
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
            return View(travelPackages);
        }

        // GET: TravelPackages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelPackages = await _context.TravelPackages
                .FirstOrDefaultAsync(m => m.TravelPackagesId == id);
            if (travelPackages == null)
            {
                return NotFound();
            }

            return View(travelPackages);
        }

        // POST: TravelPackages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var travelPackages = await _context.TravelPackages.FindAsync(id);
            if (travelPackages != null)
            {
                _context.TravelPackages.Remove(travelPackages);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TravelPackagesExists(int id)
        {
            return _context.TravelPackages.Any(e => e.TravelPackagesId == id);
        }
    }
}
