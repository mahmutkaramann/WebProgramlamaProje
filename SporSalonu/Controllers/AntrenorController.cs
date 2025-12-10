using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonu.Data;
using SporSalonu.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SporSalonu.Controllers
{
    [Authorize] // Tüm action'lara erişim için giriş gerekli
    public class AntrenorController : Controller
    {
        private readonly AppDbContext _context;

        public AntrenorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Antrenor
        // Üyeler ve Admin görüntüleyebilir
        public async Task<IActionResult> Index()
        {
            return View(await _context.Antrenorler.ToListAsync());
        }

        // GET: Antrenor/Details/5
        // Sadece görüntüleme
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .FirstOrDefaultAsync(m => m.AntId == id);
            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        // GET: Antrenor/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Antrenor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("AntId,AntAd,AntSoyad,AntUzmanlik")] Antrenorler antrenor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(antrenor);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = antrenor.AntAd + " " + antrenor.AntSoyad + " adlı antrenör başarıyla eklendi";
                return RedirectToAction(nameof(Index));
            }
            return View(antrenor);
        }

        // GET: Antrenor/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null)
            {
                return NotFound();
            }
            return View(antrenor);
        }

        // POST: Antrenor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("AntId,AntAd,AntSoyad,AntUzmanlik")] Antrenorler antrenor)
        {
            if (id != antrenor.AntId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();
                    TempData["Mesaj"] = antrenor.AntAd + " " + antrenor.AntSoyad + " adlı antrenör başarıyla güncellendi";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.AntId))
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
            return View(antrenor);
        }

        // GET: Antrenor/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .FirstOrDefaultAsync(m => m.AntId == id);
            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        // POST: Antrenor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            _context.Antrenorler.Remove(antrenor);
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = antrenor.AntAd + " " + antrenor.AntSoyad + " adlı antrenör başarıyla silindi";
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntId == id);
        }
    }
}