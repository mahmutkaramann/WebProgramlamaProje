using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using YeniSalon.Data;
using YeniSalon.Models;

namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SalonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Salon/Index
        public IActionResult Index()
        {
            // API endpoint'ini ViewBag'e ekleyelim
            ViewBag.ApiEndpoint = "/api/SalonApi";
            return View();
        }

        // GET: Salon/List - API ile çalışan yeni liste sayfası
        public IActionResult List()
        {
            return View();
        }

        // GET: Salon/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var salon = await _context.Salonlar
                .Include(s => s.Antrenorler)
                .Include(s => s.HizmetTurleri)
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon == null)
            {
                return NotFound();
            }

            ViewBag.ApiEndpoint = $"/api/SalonApi/{id}";
            return View(salon);
        }

        // GET: Salon/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Salon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Salon salon)
        {
            if (ModelState.IsValid)
            {
                // Aynı isimde salon var mı kontrol et
                var salonAdiVarMi = await _context.Salonlar
                    .AnyAsync(s => s.SalonAdi.ToLower() == salon.SalonAdi.ToLower());

                if (salonAdiVarMi)
                {
                    ModelState.AddModelError("SalonAdi", "Bu isimde bir salon zaten mevcut.");
                    return View(salon);
                }

                _context.Add(salon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Salon başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            return View(salon);
        }

        // GET: Salon/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var salon = await _context.Salonlar.FindAsync(id);

            if (salon == null)
            {
                return NotFound();
            }

            ViewBag.ApiEndpoint = $"/api/SalonApi/{id}";
            return View(salon);
        }

        // POST: Salon/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Salon salon)
        {
            if (id != salon.SalonId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Aynı isimde başka salon var mı kontrol et (kendisi hariç)
                    var salonAdiVarMi = await _context.Salonlar
                        .AnyAsync(s => s.SalonAdi.ToLower() == salon.SalonAdi.ToLower() && s.SalonId != id);

                    if (salonAdiVarMi)
                    {
                        ModelState.AddModelError("SalonAdi", "Bu isimde başka bir salon zaten mevcut.");
                        return View(salon);
                    }

                    _context.Update(salon);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Salon bilgileri başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalonExists(salon.SalonId))
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
            return View(salon);
        }

        // GET: Salon/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var salon = await _context.Salonlar
                .Include(s => s.Antrenorler)
                .Include(s => s.HizmetTurleri)
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon == null)
            {
                return NotFound();
            }

            ViewBag.AntrenorSayisi = salon.Antrenorler?.Count ?? 0;
            ViewBag.HizmetSayisi = salon.HizmetTurleri?.Count ?? 0;

            return View(salon);
        }

        // POST: Salon/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salon = await _context.Salonlar
                .Include(s => s.Antrenorler)
                .Include(s => s.HizmetTurleri)
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon != null)
            {
                // Salonun antrenörleri var mı kontrol et
                if (salon.Antrenorler != null && salon.Antrenorler.Any())
                {
                    TempData["ErrorMessage"] = "Bu salona kayıtlı antrenörler bulunmaktadır. Önce antrenörleri başka bir salona taşıyın.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Salonun hizmetleri var mı kontrol et
                if (salon.HizmetTurleri != null && salon.HizmetTurleri.Any())
                {
                    TempData["ErrorMessage"] = "Bu salona kayıtlı hizmetler bulunmaktadır. Önce hizmetleri başka bir salona taşıyın.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Salonlar.Remove(salon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Salon başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Salon/Statistics/{id}
        public async Task<IActionResult> Statistics(int id)
        {
            var salon = await _context.Salonlar
                .Include(s => s.Antrenorler)
                .Include(s => s.HizmetTurleri)
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon == null)
            {
                return NotFound();
            }

            // İstatistikleri hesapla
            ViewBag.AktifAntrenorSayisi = salon.Antrenorler?.Count(a => a.AktifMi) ?? 0;
            ViewBag.ToplamAntrenorSayisi = salon.Antrenorler?.Count ?? 0;
            ViewBag.AktifHizmetSayisi = salon.HizmetTurleri?.Count(h => h.AktifMi) ?? 0;
            ViewBag.ToplamHizmetSayisi = salon.HizmetTurleri?.Count ?? 0;

            // Bu salondaki toplam randevu sayısı
            ViewBag.ToplamRandevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .CountAsync(r => r.Antrenor != null && r.Antrenor.SalonId == id);

            // Son 30 günkü randevu sayısı
            var son30Gun = DateTime.Now.AddDays(-30);
            ViewBag.Son30GunRandevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .CountAsync(r => r.Antrenor != null && r.Antrenor.SalonId == id && r.RandevuTarihi >= son30Gun);

            ViewBag.ApiEndpoint = $"/api/SalonApi/GetSalonStatistics/{id}";
            return View(salon);
        }

        private bool SalonExists(int id)
        {
            return _context.Salonlar.Any(e => e.SalonId == id);
        }
    }
}