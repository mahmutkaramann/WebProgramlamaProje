using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YeniSalon.Data;
using YeniSalon.Models;

namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MusaitlikSaatiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MusaitlikSaatiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MusaitlikSaati/Index/5 (Antrenör ID'ye göre)
        public async Task<IActionResult> Index(int? antrenorId)
        {
            if (antrenorId == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.AntrenorId == antrenorId);

            if (antrenor == null)
            {
                return NotFound();
            }

            var musaitlikSaatleri = await _context.MusaitlikSaatleri
                .Where(m => m.AntrenorId == antrenorId && m.AktifMi)
                .OrderBy(m => m.Gun)
                .ThenBy(m => m.BaslangicSaati)
                .ToListAsync();

            ViewBag.Antrenor = antrenor;
            return View(musaitlikSaatleri);
        }

        // GET: MusaitlikSaati/Create/5
        public async Task<IActionResult> Create(int? antrenorId)
        {
            if (antrenorId == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .FirstOrDefaultAsync(a => a.AntrenorId == antrenorId);

            if (antrenor == null)
            {
                return NotFound();
            }

            ViewBag.Antrenor = antrenor;
            ViewBag.Gunler = GetGunlerSelectList();
            return View();
        }

        // POST: MusaitlikSaati/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MusaitlikSaati musaitlikSaati)
        {
            if (ModelState.IsValid)
            {
                // Aynı gün ve saat aralığında kayıt var mı kontrol et
                var kayitVarMi = await _context.MusaitlikSaatleri
                    .AnyAsync(m => m.AntrenorId == musaitlikSaati.AntrenorId &&
                                  m.Gun == musaitlikSaati.Gun &&
                                  m.AktifMi &&
                                  ((musaitlikSaati.BaslangicSaati >= m.BaslangicSaati && musaitlikSaati.BaslangicSaati < m.BitisSaati) ||
                                   (musaitlikSaati.BitisSaati > m.BaslangicSaati && musaitlikSaati.BitisSaati <= m.BitisSaati) ||
                                   (musaitlikSaati.BaslangicSaati <= m.BaslangicSaati && musaitlikSaati.BitisSaati >= m.BitisSaati)));

                if (kayitVarMi)
                {
                    ModelState.AddModelError("", "Bu saat aralığında zaten müsaitlik saati tanımlanmış.");
                    var antrenor = await _context.Antrenorler.FindAsync(musaitlikSaati.AntrenorId);
                    ViewBag.Antrenor = antrenor;
                    ViewBag.Gunler = GetGunlerSelectList();
                    return View(musaitlikSaati);
                }

                musaitlikSaati.AktifMi = true;
                _context.Add(musaitlikSaati);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Müsaitlik saati başarıyla eklendi.";
                return RedirectToAction(nameof(Index), new { antrenorId = musaitlikSaati.AntrenorId });
            }

            var antrenor2 = await _context.Antrenorler.FindAsync(musaitlikSaati.AntrenorId);
            ViewBag.Antrenor = antrenor2;
            ViewBag.Gunler = GetGunlerSelectList();
            return View(musaitlikSaati);
        }

        // GET: MusaitlikSaati/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var musaitlikSaati = await _context.MusaitlikSaatleri
                .Include(m => m.Antrenor)
                .FirstOrDefaultAsync(m => m.MusaitlikId == id);

            if (musaitlikSaati == null)
            {
                return NotFound();
            }

            ViewBag.Gunler = GetGunlerSelectList(musaitlikSaati.Gun);
            return View(musaitlikSaati);
        }

        // POST: MusaitlikSaati/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MusaitlikSaati musaitlikSaati)
        {
            if (id != musaitlikSaati.MusaitlikId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Aynı gün ve saat aralığında başka kayıt var mı kontrol et (kendisi hariç)
                    var kayitVarMi = await _context.MusaitlikSaatleri
                        .AnyAsync(m => m.AntrenorId == musaitlikSaati.AntrenorId &&
                                      m.Gun == musaitlikSaati.Gun &&
                                      m.MusaitlikId != id &&
                                      m.AktifMi &&
                                      ((musaitlikSaati.BaslangicSaati >= m.BaslangicSaati && musaitlikSaati.BaslangicSaati < m.BitisSaati) ||
                                       (musaitlikSaati.BitisSaati > m.BaslangicSaati && musaitlikSaati.BitisSaati <= m.BitisSaati) ||
                                       (musaitlikSaati.BaslangicSaati <= m.BaslangicSaati && musaitlikSaati.BitisSaati >= m.BitisSaati)));

                    if (kayitVarMi)
                    {
                        ModelState.AddModelError("", "Bu saat aralığında zaten müsaitlik saati tanımlanmış.");
                        var antrenor = await _context.Antrenorler.FindAsync(musaitlikSaati.AntrenorId);
                        ViewBag.Gunler = GetGunlerSelectList(musaitlikSaati.Gun);
                        return View(musaitlikSaati);
                    }

                    _context.Update(musaitlikSaati);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Müsaitlik saati başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MusaitlikSaatiExists(musaitlikSaati.MusaitlikId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { antrenorId = musaitlikSaati.AntrenorId });
            }

            ViewBag.Gunler = GetGunlerSelectList(musaitlikSaati.Gun);
            return View(musaitlikSaati);
        }

        // GET: MusaitlikSaati/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var musaitlikSaati = await _context.MusaitlikSaatleri
                .Include(m => m.Antrenor)
                .FirstOrDefaultAsync(m => m.MusaitlikId == id);

            if (musaitlikSaati == null)
            {
                return NotFound();
            }

            return View(musaitlikSaati);
        }

        // POST: MusaitlikSaati/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var musaitlikSaati = await _context.MusaitlikSaatleri.FindAsync(id);
            if (musaitlikSaati != null)
            {
                // Aktif randevu kontrolü
                var aktifRandevuVarMi = await _context.Randevular
                    .AnyAsync(r => r.AntrenorId == musaitlikSaati.AntrenorId &&
                                  r.Durum != RandevuDurumu.IptalEdildi &&
                                  r.Durum != RandevuDurumu.Reddedildi &&
                                  r.Durum != RandevuDurumu.Gelmeyen &&
                                  r.RandevuTarihi.DayOfWeek == musaitlikSaati.Gun &&
                                  r.RandevuTarihi.TimeOfDay >= musaitlikSaati.BaslangicSaati &&
                                  r.RandevuTarihi.TimeOfDay < musaitlikSaati.BitisSaati);

                if (aktifRandevuVarMi)
                {
                    TempData["ErrorMessage"] = "Bu müsaitlik saatine ait aktif randevular olduğu için silinemez.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Soft delete
                musaitlikSaati.AktifMi = false;
                _context.Update(musaitlikSaati);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Müsaitlik saati başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index), new { antrenorId = musaitlikSaati?.AntrenorId });
        }

        // POST: MusaitlikSaati/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var musaitlikSaati = await _context.MusaitlikSaatleri.FindAsync(id);
            if (musaitlikSaati != null)
            {
                musaitlikSaati.AktifMi = false;
                _context.Update(musaitlikSaati);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Müsaitlik saati pasif hale getirildi.";
            }

            return RedirectToAction(nameof(Index), new { antrenorId = musaitlikSaati?.AntrenorId });
        }

        // POST: MusaitlikSaati/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var musaitlikSaati = await _context.MusaitlikSaatleri.FindAsync(id);
            if (musaitlikSaati != null)
            {
                musaitlikSaati.AktifMi = true;
                _context.Update(musaitlikSaati);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Müsaitlik saati aktif hale getirildi.";
            }

            return RedirectToAction(nameof(Index), new { antrenorId = musaitlikSaati?.AntrenorId });
        }

        private bool MusaitlikSaatiExists(int id)
        {
            return _context.MusaitlikSaatleri.Any(e => e.MusaitlikId == id);
        }

        private List<SelectListItem> GetGunlerSelectList(DayOfWeek? seciliGun = null)
        {
            var gunler = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Pazartesi", Selected = seciliGun == DayOfWeek.Monday },
                new SelectListItem { Value = "2", Text = "Salı", Selected = seciliGun == DayOfWeek.Tuesday },
                new SelectListItem { Value = "3", Text = "Çarşamba", Selected = seciliGun == DayOfWeek.Wednesday },
                new SelectListItem { Value = "4", Text = "Perşembe", Selected = seciliGun == DayOfWeek.Thursday },
                new SelectListItem { Value = "5", Text = "Cuma", Selected = seciliGun == DayOfWeek.Friday },
                new SelectListItem { Value = "6", Text = "Cumartesi", Selected = seciliGun == DayOfWeek.Saturday },
                new SelectListItem { Value = "0", Text = "Pazar", Selected = seciliGun == DayOfWeek.Sunday }
            };

            return gunler;
        }

        private string GetGunAdi(DayOfWeek gun)
        {
            return gun switch
            {
                DayOfWeek.Monday => "Pazartesi",
                DayOfWeek.Tuesday => "Salı",
                DayOfWeek.Wednesday => "Çarşamba",
                DayOfWeek.Thursday => "Perşembe",
                DayOfWeek.Friday => "Cuma",
                DayOfWeek.Saturday => "Cumartesi",
                DayOfWeek.Sunday => "Pazar",
                _ => gun.ToString()
            };
        }
    }
}