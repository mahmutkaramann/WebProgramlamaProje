using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YeniSalon.Data;
using YeniSalon.Models;

namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HizmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hizmet
        public async Task<IActionResult> Index(string kategori = null, bool aktifMi = true)
        {
            var hizmetler = _context.Hizmetler.Include(h => h.Salon).AsQueryable();

            // Filtreleme
            if (!string.IsNullOrEmpty(kategori))
            {
                hizmetler = hizmetler.Where(h => h.Kategori == kategori);
            }

            if (!aktifMi)
            {
                hizmetler = hizmetler.Where(h => !h.AktifMi);
            }
            else
            {
                hizmetler = hizmetler.Where(h => h.AktifMi);
            }

            var kategoriler = await _context.Hizmetler
                .Where(h => h.Kategori != null)
                .Select(h => h.Kategori!)
                .Distinct()
                .ToListAsync();

            ViewBag.Kategoriler = kategoriler;
            ViewBag.SeciliKategori = kategori;
            ViewBag.AktifMi = aktifMi;

            return View(await hizmetler.OrderBy(h => h.HizmetAdi).ToListAsync());
        }

        // GET: Hizmet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .Include(h => h.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Antrenor)
                .FirstOrDefaultAsync(m => m.HizmetId == id);

            if (hizmet == null)
            {
                return NotFound();
            }

            // Bu hizmeti veren antrenörleri al
            var antrenorler = await _context.AntrenorHizmetler
                .Where(ah => ah.HizmetId == id)
                .Include(ah => ah.Antrenor)
                .Select(ah => ah.Antrenor)
                .Where(a => a != null && a.AktifMi)
                .ToListAsync();

            ViewBag.Antrenorler = antrenorler;

            return View(hizmet);
        }

        // GET: Hizmet/Create
        public IActionResult Create()
        {
            var salonlar = _context.Salonlar.Where(s => true).ToList();
            ViewBag.Salonlar = new SelectList(salonlar, "SalonId", "SalonAdi");

            // Kategori önerileri
            var mevcutKategoriler = _context.Hizmetler
                .Where(h => h.Kategori != null)
                .Select(h => h.Kategori!)
                .Distinct()
                .ToList();

            ViewBag.KategoriListesi = mevcutKategoriler;

            return View();
        }

        // POST: Hizmet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hizmet hizmet)
        {
            if (ModelState.IsValid)
            {
                // Aynı isimde hizmet kontrolü
                var hizmetVarMi = await _context.Hizmetler
                    .AnyAsync(h => h.HizmetAdi.ToLower() == hizmet.HizmetAdi.ToLower());

                if (hizmetVarMi)
                {
                    ModelState.AddModelError("HizmetAdi", "Bu isimde bir hizmet zaten mevcut.");
                    ViewBag.Salonlar = new SelectList(_context.Salonlar, "SalonId", "SalonAdi", hizmet.SalonId);
                    return View(hizmet);
                }

                _context.Add(hizmet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hizmet başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Salonlar = new SelectList(_context.Salonlar, "SalonId", "SalonAdi", hizmet.SalonId);
            return View(hizmet);
        }

        // GET: Hizmet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null)
            {
                return NotFound();
            }

            var salonlar = _context.Salonlar.Where(s => true).ToList();
            ViewBag.Salonlar = new SelectList(salonlar, "SalonId", "SalonAdi", hizmet.SalonId);

            // Kategori önerileri
            var mevcutKategoriler = _context.Hizmetler
                .Where(h => h.Kategori != null && h.HizmetId != id)
                .Select(h => h.Kategori!)
                .Distinct()
                .ToList();

            ViewBag.KategoriListesi = mevcutKategoriler;

            return View(hizmet);
        }

        // POST: Hizmet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hizmet hizmet)
        {
            if (id != hizmet.HizmetId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Aynı isimde başka hizmet kontrolü (kendisi hariç)
                    var hizmetVarMi = await _context.Hizmetler
                        .AnyAsync(h => h.HizmetAdi.ToLower() == hizmet.HizmetAdi.ToLower() && h.HizmetId != id);

                    if (hizmetVarMi)
                    {
                        ModelState.AddModelError("HizmetAdi", "Bu isimde bir hizmet zaten mevcut.");
                        ViewBag.Salonlar = new SelectList(_context.Salonlar, "SalonId", "SalonAdi", hizmet.SalonId);
                        return View(hizmet);
                    }

                    _context.Update(hizmet);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Hizmet başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HizmetExists(hizmet.HizmetId))
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

            ViewBag.Salonlar = new SelectList(_context.Salonlar, "SalonId", "SalonAdi", hizmet.SalonId);
            return View(hizmet);
        }

        // GET: Hizmet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(m => m.HizmetId == id);

            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        // POST: Hizmet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                // Hizmete ait randevular var mı kontrol et
                var randevuVarMi = await _context.Randevular
                    .AnyAsync(r => r.HizmetId == id && (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));

                if (randevuVarMi)
                {
                    TempData["ErrorMessage"] = "Bu hizmete ait aktif randevular olduğu için silinemez. Lütfen önce randevuları iptal edin.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Antrenör-hizmet ilişkilerini sil
                var antrenorHizmetler = await _context.AntrenorHizmetler
                    .Where(ah => ah.HizmetId == id)
                    .ToListAsync();

                if (antrenorHizmetler.Any())
                {
                    _context.AntrenorHizmetler.RemoveRange(antrenorHizmetler);
                }

                // Hizmeti sil
                _context.Hizmetler.Remove(hizmet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hizmet başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Hizmet/Deactivate/5 (Soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                hizmet.AktifMi = false;
                _context.Update(hizmet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hizmet pasif hale getirildi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Hizmet/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                hizmet.AktifMi = true;
                _context.Update(hizmet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hizmet aktif hale getirildi.";
            }

            return RedirectToAction(nameof(Index), new { aktifMi = false });
        }

        // GET: Hizmet/AssignAntrenor/5 (Antrenör ata)
        public async Task<IActionResult> AssignAntrenor(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null)
            {
                return NotFound();
            }

            // Zaten atanmış antrenörler
            var atanmisAntrenorler = await _context.AntrenorHizmetler
                .Where(ah => ah.HizmetId == id)
                .Select(ah => ah.AntrenorId)
                .ToListAsync();

            // Aktif antrenörler (atanmamış olanlar)
            var antrenorler = await _context.Antrenorler
                .Where(a => a.AktifMi && !atanmisAntrenorler.Contains(a.AntrenorId))
                .Select(a => new SelectListItem
                {
                    Value = a.AntrenorId.ToString(),
                    Text = $"{a.Ad} {a.Soyad} - {a.UzmanlikAlanlari}"
                })
                .ToListAsync();

            ViewBag.HizmetAdi = hizmet.HizmetAdi;
            ViewBag.HizmetId = hizmet.HizmetId;
            ViewBag.Antrenorler = antrenorler;
            ViewBag.AtanmisAntrenorler = await _context.Antrenorler
                .Where(a => atanmisAntrenorler.Contains(a.AntrenorId))
                .ToListAsync();

            return View();
        }

        // POST: Hizmet/AssignAntrenor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignAntrenor(int hizmetId, int antrenorId)
        {
            var hizmetAntrenorVarMi = await _context.AntrenorHizmetler
                .AnyAsync(ah => ah.HizmetId == hizmetId && ah.AntrenorId == antrenorId);

            if (!hizmetAntrenorVarMi)
            {
                var antrenorHizmet = new AntrenorHizmet
                {
                    AntrenorId = antrenorId,
                    HizmetId = hizmetId
                };

                _context.Add(antrenorHizmet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Antrenör başarıyla hizmete atandı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bu antrenör zaten bu hizmete atanmış.";
            }

            return RedirectToAction(nameof(AssignAntrenor), new { id = hizmetId });
        }

        // POST: Hizmet/RemoveAntrenor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAntrenor(int hizmetId, int antrenorId)
        {
            var antrenorHizmet = await _context.AntrenorHizmetler
                .FirstOrDefaultAsync(ah => ah.HizmetId == hizmetId && ah.AntrenorId == antrenorId);

            if (antrenorHizmet != null)
            {
                // Bu hizmete ait aktif randevular var mı kontrol et
                var randevuVarMi = await _context.Randevular
                    .AnyAsync(r => r.HizmetId == hizmetId && r.AntrenorId == antrenorId &&
                                   (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));

                if (randevuVarMi)
                {
                    TempData["ErrorMessage"] = "Bu antrenöre ait aktif randevular olduğu için ilişki kaldırılamaz.";
                    return RedirectToAction(nameof(AssignAntrenor), new { id = hizmetId });
                }

                _context.AntrenorHizmetler.Remove(antrenorHizmet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Antrenör-hizmet ilişkisi başarıyla kaldırıldı.";
            }

            return RedirectToAction(nameof(AssignAntrenor), new { id = hizmetId });
        }

        private bool HizmetExists(int id)
        {
            return _context.Hizmetler.Any(e => e.HizmetId == id);
        }
    }
}