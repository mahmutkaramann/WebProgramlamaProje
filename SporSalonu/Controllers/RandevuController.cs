using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using YeniSalon.Data;
using YeniSalon.Models;

namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Randevu
        public async Task<IActionResult> Index(
            string? kullaniciId = null,
            int? antrenorId = null,
            int? hizmetId = null,
            string? durum = null,
            DateTime? baslangicTarihi = null,
            DateTime? bitisTarihi = null,
            string sortBy = "RandevuTarihi",
            string sortOrder = "desc",
            int page = 1,
            int pageSize = 15)
        {
            // Sorgu oluştur
            var query = _context.Randevular
                .Include(r => r.Kullanici)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .AsQueryable();

            // Filtreleme
            if (!string.IsNullOrEmpty(kullaniciId))
            {
                query = query.Where(r => r.KullaniciId == kullaniciId);
            }

            if (antrenorId.HasValue)
            {
                query = query.Where(r => r.AntrenorId == antrenorId.Value);
            }

            if (hizmetId.HasValue)
            {
                query = query.Where(r => r.HizmetId == hizmetId.Value);
            }

            if (!string.IsNullOrEmpty(durum))
            {
                if (Enum.TryParse<RandevuDurumu>(durum, out var durumEnum))
                {
                    query = query.Where(r => r.Durum == durumEnum);
                }
            }

            if (baslangicTarihi.HasValue)
            {
                query = query.Where(r => r.RandevuTarihi >= baslangicTarihi.Value);
            }

            if (bitisTarihi.HasValue)
            {
                var bitisTarihiGunSonu = bitisTarihi.Value.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.RandevuTarihi <= bitisTarihiGunSonu);
            }

            // Sıralama
            if (!string.IsNullOrEmpty(sortBy))
            {
                string sortDirection = sortOrder.ToLower() == "desc" ? "descending" : "ascending";

                // Özel sıralama için
                if (sortBy == "KullaniciAdi")
                {
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.Kullanici.Ad)
                        : query.OrderBy(r => r.Kullanici.Ad);
                }
                else if (sortBy == "AntrenorAdi")
                {
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.Antrenor.Ad)
                        : query.OrderBy(r => r.Antrenor.Ad);
                }
                else if (sortBy == "HizmetAdi")
                {
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.Hizmet.HizmetAdi)
                        : query.OrderBy(r => r.Hizmet.HizmetAdi);
                }
                else
                {
                    query = query.OrderBy($"{sortBy} {sortDirection}");
                }
            }

            // Toplam kayıt sayısı
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Sayfalama
            var randevular = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewBag'ler
            ViewBag.KullaniciId = kullaniciId;
            ViewBag.AntrenorId = antrenorId;
            ViewBag.HizmetId = hizmetId;
            ViewBag.Durum = durum;
            ViewBag.BaslangicTarihi = baslangicTarihi?.ToString("yyyy-MM-dd");
            ViewBag.BitisTarihi = bitisTarihi?.ToString("yyyy-MM-dd");
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;

            // Filtre seçenekleri
            ViewBag.Kullanicilar = await _context.Users
                .Where(u => u.AktifMi)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.Ad} {u.Soyad} - {u.Email}"
                })
                .ToListAsync();

            ViewBag.Antrenorler = await _context.Antrenorler
                .Where(a => a.AktifMi)
                .Select(a => new SelectListItem
                {
                    Value = a.AntrenorId.ToString(),
                    Text = $"{a.Ad} {a.Soyad}"
                })
                .ToListAsync();

            ViewBag.Hizmetler = await _context.Hizmetler
                .Where(h => h.AktifMi)
                .Select(h => new SelectListItem
                {
                    Value = h.HizmetId.ToString(),
                    Text = h.HizmetAdi
                })
                .ToListAsync();

            // DÜZELTME: List<SelectListItem> olarak ayarla
            ViewBag.Durumlar = Enum.GetValues(typeof(RandevuDurumu))
                .Cast<RandevuDurumu>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                })
                .ToList();

            // İstatistikler
            ViewBag.ToplamRandevu = totalRecords;
            ViewBag.BekleyenRandevu = await _context.Randevular
                .CountAsync(r => r.Durum == RandevuDurumu.Beklemede);
            ViewBag.OnaylananRandevu = await _context.Randevular
                .CountAsync(r => r.Durum == RandevuDurumu.Onaylandi);
            ViewBag.TamamlananRandevu = await _context.Randevular
                .CountAsync(r => r.Durum == RandevuDurumu.Tamamlandi);

            return View(randevular);
        }

        // GET: Randevu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Kullanici)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .FirstOrDefaultAsync(m => m.RandevuId == id);

            if (randevu == null)
            {
                return NotFound();
            }

            // Antrenörün müsaitlik saatlerini kontrol et
            var musaitlikSaatleri = await _context.MusaitlikSaatleri
                .Where(m => m.AntrenorId == randevu.AntrenorId && m.AktifMi)
                .ToListAsync();

            ViewBag.MusaitlikSaatleri = musaitlikSaatleri;

            return View(randevu);
        }

        // GET: Randevu/OnayBekleyen
        public async Task<IActionResult> OnayBekleyen()
        {
            var onayBekleyenRandevular = await _context.Randevular
                .Include(r => r.Kullanici)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.Durum == RandevuDurumu.Beklemede)
                .OrderBy(r => r.RandevuTarihi)
                .ToListAsync();

            return View(onayBekleyenRandevular);
        }

        // GET: Randevu/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCreateViewBags();
            return View();
        }

        // POST: Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            if (ModelState.IsValid)
            {
                // Çakışma kontrolü
                var cakismaVarMi = await _context.Randevular
                    .AnyAsync(r => r.AntrenorId == randevu.AntrenorId &&
                                  r.Durum != RandevuDurumu.IptalEdildi &&
                                  r.Durum != RandevuDurumu.Reddedildi &&
                                  r.Durum != RandevuDurumu.Gelmeyen &&
                                  ((randevu.RandevuTarihi >= r.RandevuTarihi && randevu.RandevuTarihi < r.BitisTarihi) ||
                                   (randevu.BitisTarihi > r.RandevuTarihi && randevu.BitisTarihi <= r.BitisTarihi) ||
                                   (randevu.RandevuTarihi <= r.RandevuTarihi && randevu.BitisTarihi >= r.BitisTarihi)));

                if (cakismaVarMi)
                {
                    ModelState.AddModelError("RandevuTarihi", "Bu saatte antrenörün başka bir randevusu bulunmaktadır.");
                    await PopulateCreateViewBags();
                    return View(randevu);
                }

                // Hizmet süresini hesapla ve bitiş tarihini ayarla
                var hizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
                if (hizmet != null)
                {
                    randevu.BitisTarihi = randevu.RandevuTarihi.AddMinutes(hizmet.SureDakika);
                }

                randevu.OlusturulmaTarihi = DateTime.Now;

                _context.Add(randevu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevu başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCreateViewBags();
            return View(randevu);
        }

        // GET: Randevu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            await PopulateEditViewBags(randevu);
            return View(randevu);
        }

        // POST: Randevu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Randevu randevu)
        {
            if (id != randevu.RandevuId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Çakışma kontrolü (kendisi hariç)
                    var cakismaVarMi = await _context.Randevular
                        .AnyAsync(r => r.AntrenorId == randevu.AntrenorId &&
                                      r.RandevuId != id &&
                                      r.Durum != RandevuDurumu.IptalEdildi &&
                                      r.Durum != RandevuDurumu.Reddedildi &&
                                      r.Durum != RandevuDurumu.Gelmeyen &&
                                      ((randevu.RandevuTarihi >= r.RandevuTarihi && randevu.RandevuTarihi < r.BitisTarihi) ||
                                       (randevu.BitisTarihi > r.RandevuTarihi && randevu.BitisTarihi <= r.BitisTarihi) ||
                                       (randevu.RandevuTarihi <= r.RandevuTarihi && randevu.BitisTarihi >= r.BitisTarihi)));

                    if (cakismaVarMi)
                    {
                        ModelState.AddModelError("RandevuTarihi", "Bu saatte antrenörün başka bir randevusu bulunmaktadır.");
                        await PopulateEditViewBags(randevu);
                        return View(randevu);
                    }

                    // Hizmet süresini hesapla ve bitiş tarihini ayarla
                    var hizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
                    if (hizmet != null)
                    {
                        randevu.BitisTarihi = randevu.RandevuTarihi.AddMinutes(hizmet.SureDakika);
                    }

                    _context.Update(randevu);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Randevu başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RandevuExists(randevu.RandevuId))
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

            await PopulateEditViewBags(randevu);
            return View(randevu);
        }

        // GET: Randevu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Kullanici)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .FirstOrDefaultAsync(m => m.RandevuId == id);

            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // POST: Randevu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                // Onaylanmış veya beklemede olan randevular için kontrol
                if (randevu.Durum == RandevuDurumu.Onaylandi || randevu.Durum == RandevuDurumu.Beklemede)
                {
                    TempData["ErrorMessage"] = "Onaylanmış veya beklemede olan randevular silinemez. Lütfen önce iptal edin.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevu başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Randevu/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                if (randevu.Durum != RandevuDurumu.Beklemede)
                {
                    TempData["ErrorMessage"] = "Sadece beklemede durumundaki randevular onaylanabilir.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Çakışma kontrolü
                var cakismaVarMi = await _context.Randevular
                    .AnyAsync(r => r.AntrenorId == randevu.AntrenorId &&
                                  r.RandevuId != id &&
                                  r.Durum == RandevuDurumu.Onaylandi &&
                                  ((randevu.RandevuTarihi >= r.RandevuTarihi && randevu.RandevuTarihi < r.BitisTarihi) ||
                                   (randevu.BitisTarihi > r.RandevuTarihi && randevu.BitisTarihi <= r.BitisTarihi)));

                if (cakismaVarMi)
                {
                    TempData["ErrorMessage"] = "Bu saatte antrenörün onaylanmış başka bir randevusu bulunmaktadır.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                randevu.Durum = RandevuDurumu.Onaylandi;

                _context.Update(randevu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevu başarıyla onaylandı.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Randevu/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason = null)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                if (randevu.Durum == RandevuDurumu.Tamamlandi)
                {
                    TempData["ErrorMessage"] = "Tamamlanmış randevular reddedilemez.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                randevu.Durum = RandevuDurumu.Reddedildi;

                if (!string.IsNullOrEmpty(reason))
                {
                    randevu.Not = $"Reddedilme nedeni: {reason}";
                }

                _context.Update(randevu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevu başarıyla reddedildi.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Randevu/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason = null)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                if (randevu.Durum == RandevuDurumu.Tamamlandi)
                {
                    TempData["ErrorMessage"] = "Tamamlanmış randevular iptal edilemez.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                randevu.Durum = RandevuDurumu.IptalEdildi;

                if (!string.IsNullOrEmpty(reason))
                {
                    randevu.Not = $"İptal nedeni: {reason}";
                }

                _context.Update(randevu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevu başarıyla iptal edildi.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Randevu/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, int? degerlendirme = null, string? degerlendirmeNotu = null)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                if (randevu.Durum != RandevuDurumu.Onaylandi)
                {
                    TempData["ErrorMessage"] = "Sadece onaylanmış randevular tamamlanabilir.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                randevu.Durum = RandevuDurumu.Tamamlandi;

                _context.Update(randevu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevu başarıyla tamamlandı olarak işaretlendi.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Randevu/MarkAsNoShow/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsNoShow(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                if (randevu.Durum != RandevuDurumu.Onaylandi)
                {
                    TempData["ErrorMessage"] = "Sadece onaylanmış randevular için gelmeyen işaretlenebilir.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                randevu.Durum = RandevuDurumu.Gelmeyen;

                _context.Update(randevu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevu gelmeyen olarak işaretlendi.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Randevu/Calendar
        public async Task<IActionResult> Calendar(int? antrenorId = null, DateTime? tarih = null)
        {
            var gosterilecekTarih = tarih ?? DateTime.Today;
            var baslangicTarihi = new DateTime(gosterilecekTarih.Year, gosterilecekTarih.Month, 1);
            var bitisTarihi = baslangicTarihi.AddMonths(1).AddDays(-1);

            var query = _context.Randevular
                .Include(r => r.Kullanici)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.RandevuTarihi >= baslangicTarihi && r.RandevuTarihi <= bitisTarihi &&
                           (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));

            if (antrenorId.HasValue)
            {
                query = query.Where(r => r.AntrenorId == antrenorId.Value);
            }

            var randevular = await query
                .OrderBy(r => r.RandevuTarihi)
                .ToListAsync();

            // Takvim verilerini hazırla
            var takvimVerileri = new List<TakvimRandevuViewModel>();

            foreach (var randevu in randevular)
            {
                takvimVerileri.Add(new TakvimRandevuViewModel
                {
                    Id = randevu.RandevuId,
                    Title = $"{randevu.Antrenor?.Ad} - {randevu.Hizmet?.HizmetAdi}",
                    Start = randevu.RandevuTarihi,
                    End = randevu.BitisTarihi,
                    KullaniciAdi = $"{randevu.Kullanici?.Ad} {randevu.Kullanici?.Soyad}",
                    AntrenorAdi = $"{randevu.Antrenor?.Ad} {randevu.Antrenor?.Soyad}",
                    HizmetAdi = randevu.Hizmet?.HizmetAdi,
                    Durum = randevu.Durum,
                    Color = GetRandevuColor(randevu.Durum)
                });
            }

            ViewBag.Antrenorler = await _context.Antrenorler
                .Where(a => a.AktifMi)
                .Select(a => new SelectListItem
                {
                    Value = a.AntrenorId.ToString(),
                    Text = $"{a.Ad} {a.Soyad}"
                })
                .ToListAsync();

            ViewBag.SecilenAntrenorId = antrenorId;
            ViewBag.SecilenTarih = gosterilecekTarih.ToString("yyyy-MM");
            ViewBag.TakvimVerileri = takvimVerileri;

            return View();
        }

        // GET: Randevu/Statistics
        public async Task<IActionResult> Statistics()
        {
            var son30Gun = DateTime.Now.AddDays(-30);

            var istatistikler = new RandevuIstatistikleriViewModel
            {
                // Genel istatistikler
                ToplamRandevu = await _context.Randevular.CountAsync(),
                Son30GunRandevu = await _context.Randevular
                    .CountAsync(r => r.RandevuTarihi >= son30Gun),

                // Durum istatistikleri
                BekleyenRandevu = await _context.Randevular
                    .CountAsync(r => r.Durum == RandevuDurumu.Beklemede),
                OnaylananRandevu = await _context.Randevular
                    .CountAsync(r => r.Durum == RandevuDurumu.Onaylandi),
                TamamlananRandevu = await _context.Randevular
                    .CountAsync(r => r.Durum == RandevuDurumu.Tamamlandi),
                IptalEdilenRandevu = await _context.Randevular
                    .CountAsync(r => r.Durum == RandevuDurumu.IptalEdildi),

                // Aylık dağılım
                AylikRandevular = await GetAylikRandevuDagilimi(),

                // Antrenör başına randevu sayıları
                AntrenorRandevulari = await _context.Randevular
                    .Include(r => r.Antrenor)
                    .GroupBy(r => new { r.AntrenorId, r.Antrenor.Ad, r.Antrenor.Soyad })
                    .Select(g => new AntrenorRandevuSayisi
                    {
                        AntrenorId = g.Key.AntrenorId,
                        AntrenorAdi = $"{g.Key.Ad} {g.Key.Soyad}",
                        RandevuSayisi = g.Count()
                    })
                    .OrderByDescending(a => a.RandevuSayisi)
                    .Take(10)
                    .ToListAsync(),

                // Hizmet başına randevu sayıları
                HizmetRandevulari = await _context.Randevular
                    .Include(r => r.Hizmet)
                    .GroupBy(r => new { r.HizmetId, r.Hizmet.HizmetAdi })
                    .Select(g => new HizmetRandevuSayisi
                    {
                        HizmetId = g.Key.HizmetId,
                        HizmetAdi = g.Key.HizmetAdi,
                        RandevuSayisi = g.Count()
                    })
                    .OrderByDescending(h => h.RandevuSayisi)
                    .Take(10)
                    .ToListAsync()
            };

            return View(istatistikler);
        }

        // GET: Randevu/CheckAvailability
        public async Task<IActionResult> CheckAvailability(int antrenorId, DateTime randevuTarihi, int sureDakika)
        {
            var baslangic = randevuTarihi;
            var bitis = randevuTarihi.AddMinutes(sureDakika);

            var musaitRandevular = await _context.Randevular
                .Where(r => r.AntrenorId == antrenorId &&
                           r.Durum != RandevuDurumu.IptalEdildi &&
                           r.Durum != RandevuDurumu.Reddedildi &&
                           r.Durum != RandevuDurumu.Gelmeyen &&
                           ((baslangic >= r.RandevuTarihi && baslangic < r.BitisTarihi) ||
                            (bitis > r.RandevuTarihi && bitis <= r.BitisTarihi) ||
                            (baslangic <= r.RandevuTarihi && bitis >= r.BitisTarihi)))
                .Include(r => r.Kullanici)
                .Include(r => r.Hizmet)
                .ToListAsync();

            var antrenor = await _context.Antrenorler.FindAsync(antrenorId);

            return Json(new
            {
                Musait = !musaitRandevular.Any(),
                CakisanRandevular = musaitRandevular.Select(r => new
                {
                    Kullanici = $"{r.Kullanici?.Ad} {r.Kullanici?.Soyad}",
                    Hizmet = r.Hizmet?.HizmetAdi,
                    Baslangic = r.RandevuTarihi.ToString("dd.MM.yyyy HH:mm"),
                    Bitis = r.BitisTarihi.ToString("dd.MM.yyyy HH:mm"),
                    Durum = r.Durum.ToString()
                }),
                Antrenor = antrenor != null ? $"{antrenor.Ad} {antrenor.Soyad}" : "Bilinmiyor"
            });
        }

        private async Task<List<AylikRandevuDagilimi>> GetAylikRandevuDagilimi()
        {
            var son6Ay = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(d => new { Yil = d.Year, Ay = d.Month })
                .OrderBy(x => x.Yil).ThenBy(x => x.Ay)
                .ToList();

            var dagilim = new List<AylikRandevuDagilimi>();

            foreach (var ay in son6Ay)
            {
                var ayRandevu = await _context.Randevular
                    .CountAsync(r => r.RandevuTarihi.Year == ay.Yil && r.RandevuTarihi.Month == ay.Ay);

                dagilim.Add(new AylikRandevuDagilimi
                {
                    Ay = $"{ay.Yil}-{ay.Ay:D2}",
                    RandevuSayisi = ayRandevu
                });
            }

            return dagilim;
        }

        private string GetRandevuColor(RandevuDurumu durum)
        {
            return durum switch
            {
                RandevuDurumu.Beklemede => "#ffc107", // Sarı
                RandevuDurumu.Onaylandi => "#198754", // Yeşil
                RandevuDurumu.Tamamlandi => "#0d6efd", // Mavi
                RandevuDurumu.IptalEdildi => "#dc3545", // Kırmızı
                RandevuDurumu.Reddedildi => "#6c757d", // Gri
                RandevuDurumu.Gelmeyen => "#6610f2", // Mor
                _ => "#6c757d"
            };
        }

        private async Task PopulateCreateViewBags()
        {
            ViewBag.Kullanicilar = await _context.Users
                .Where(u => u.AktifMi)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.Ad} {u.Soyad} - {u.Email}"
                })
                .ToListAsync();

            ViewBag.Antrenorler = await _context.Antrenorler
                .Where(a => a.AktifMi)
                .Select(a => new SelectListItem
                {
                    Value = a.AntrenorId.ToString(),
                    Text = $"{a.Ad} {a.Soyad} - {a.UzmanlikAlanlari}"
                })
                .ToListAsync();

            ViewBag.Hizmetler = await _context.Hizmetler
                .Where(h => h.AktifMi)
                .Select(h => new SelectListItem
                {
                    Value = h.HizmetId.ToString(),
                    Text = $"{h.HizmetAdi} - {h.Ucret:C2} - {h.SureDakika} dk"
                })
                .ToListAsync();

            // DÜZELTME: List<SelectListItem> olarak ayarla
            ViewBag.Durumlar = Enum.GetValues(typeof(RandevuDurumu))
                .Cast<RandevuDurumu>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                })
                .ToList();
        }

        private async Task PopulateEditViewBags(Randevu randevu)
        {
            ViewBag.Kullanicilar = await _context.Users
                .Where(u => u.AktifMi)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.Ad} {u.Soyad} - {u.Email}",
                    Selected = u.Id == randevu.KullaniciId
                })
                .ToListAsync();

            ViewBag.Antrenorler = await _context.Antrenorler
                .Where(a => a.AktifMi)
                .Select(a => new SelectListItem
                {
                    Value = a.AntrenorId.ToString(),
                    Text = $"{a.Ad} {a.Soyad} - {a.UzmanlikAlanlari}",
                    Selected = a.AntrenorId == randevu.AntrenorId
                })
                .ToListAsync();

            ViewBag.Hizmetler = await _context.Hizmetler
                .Where(h => h.AktifMi)
                .Select(h => new SelectListItem
                {
                    Value = h.HizmetId.ToString(),
                    Text = $"{h.HizmetAdi} - {h.Ucret:C2} - {h.SureDakika} dk",
                    Selected = h.HizmetId == randevu.HizmetId
                })
                .ToListAsync();

            // DÜZELTME: List<SelectListItem> olarak ayarla ve seçili durumu işaretle
            ViewBag.Durumlar = Enum.GetValues(typeof(RandevuDurumu))
                .Cast<RandevuDurumu>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString(),
                    Selected = e == randevu.Durum
                })
                .ToList();
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.RandevuId == id);
        }
    }

    // ViewModel'ler
    public class TakvimRandevuViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public string AntrenorAdi { get; set; } = string.Empty;
        public string HizmetAdi { get; set; } = string.Empty;
        public RandevuDurumu Durum { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class RandevuIstatistikleriViewModel
    {
        public int ToplamRandevu { get; set; }
        public int Son30GunRandevu { get; set; }
        public int BekleyenRandevu { get; set; }
        public int OnaylananRandevu { get; set; }
        public int TamamlananRandevu { get; set; }
        public int IptalEdilenRandevu { get; set; }
        public List<AylikRandevuDagilimi> AylikRandevular { get; set; } = new();
        public List<AntrenorRandevuSayisi> AntrenorRandevulari { get; set; } = new();
        public List<HizmetRandevuSayisi> HizmetRandevulari { get; set; } = new();
    }

    public class AylikRandevuDagilimi
    {
        public string Ay { get; set; } = string.Empty;
        public int RandevuSayisi { get; set; }
    }

    public class AntrenorRandevuSayisi
    {
        public int AntrenorId { get; set; }
        public string AntrenorAdi { get; set; } = string.Empty;
        public int RandevuSayisi { get; set; }
    }

    public class HizmetRandevuSayisi
    {
        public int HizmetId { get; set; }
        public string HizmetAdi { get; set; } = string.Empty;
        public int RandevuSayisi { get; set; }
    }


}