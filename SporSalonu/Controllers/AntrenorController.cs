// Controllers/AntrenorController.cs (tamamlanmış hali)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YeniSalon.Data;
using YeniSalon.Models;
using System.Linq.Dynamic.Core;

namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AntrenorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Antrenor
        public async Task<IActionResult> Index(
            string search = "",
            string cinsiyet = "",
            bool aktifMi = true,
            string sortBy = "Ad",
            string sortOrder = "asc",
            int page = 1,
            int pageSize = 10)
        {
            // Sorgu oluştur
            var query = _context.Antrenorler
                .Include(a => a.Salon)
                .AsQueryable();

            // Filtreleme
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.Ad.Contains(search) ||
                    a.Soyad.Contains(search) ||
                    a.TCKimlikNo.Contains(search) ||
                    a.Adres.Contains(search) ||
                    a.UzmanlikAlanlari.Contains(search));
            }

            if (!string.IsNullOrEmpty(cinsiyet))
            {
                if (Enum.TryParse<Cinsiyet>(cinsiyet, out var cinsiyetEnum))
                {
                    query = query.Where(a => a.Cinsiyet == cinsiyetEnum);
                }
            }

            query = query.Where(a => a.AktifMi == aktifMi);

            // Sıralama
            if (!string.IsNullOrEmpty(sortBy))
            {
                string sortDirection = sortOrder.ToLower() == "desc" ? "descending" : "ascending";
                query = query.OrderBy($"{sortBy} {sortDirection}");
            }

            // Toplam kayıt sayısı
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Sayfalama
            var antrenorler = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewBag'ler
            ViewBag.Search = search;
            ViewBag.Cinsiyet = cinsiyet;
            ViewBag.AktifMi = aktifMi;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;

            // Filtre seçenekleri
            ViewBag.CinsiyetListesi = new SelectList(Enum.GetValues(typeof(Cinsiyet)));

            return View(antrenorler);
        }

        // GET: Antrenor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.MusaitlikSaatleri)
                .Include(a => a.Randevular)
                    .ThenInclude(r => r.Kullanici)
                .FirstOrDefaultAsync(m => m.AntrenorId == id);

            if (antrenor == null)
            {
                return NotFound();
            }

            // İstatistikler
            ViewBag.ToplamRandevu = await _context.Randevular
                .CountAsync(r => r.AntrenorId == id);
            ViewBag.AktifRandevu = await _context.Randevular
                .CountAsync(r => r.AntrenorId == id &&
                               (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));
            ViewBag.TamamlananRandevu = await _context.Randevular
                .CountAsync(r => r.AntrenorId == id && r.Durum == RandevuDurumu.Tamamlandi);

            // Haftalık randevular
            var baslangic = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
            var bitis = baslangic.AddDays(7);
            ViewBag.HaftalikRandevu = await _context.Randevular
                .CountAsync(r => r.AntrenorId == id &&
                               r.RandevuTarihi >= baslangic && r.RandevuTarihi < bitis);

            return View(antrenor);
        }

        // GET: Antrenor/Create
        public IActionResult Create()
        {
            ViewBag.Salonlar = _context.Salonlar
                .Where(s => true) // Tüm salonlar
                .Select(s => new SelectListItem
                {
                    Value = s.SalonId.ToString(),
                    Text = s.SalonAdi
                })
                .ToList();

            return View();
        }

        // POST: Antrenor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Antrenor antrenor)
        {
            if (ModelState.IsValid)
            {
                // TC Kimlik No kontrolü
                var tcVarMi = await _context.Antrenorler
                    .AnyAsync(a => a.TCKimlikNo == antrenor.TCKimlikNo);

                if (tcVarMi)
                {
                    ModelState.AddModelError("TCKimlikNo", "Bu TC Kimlik No ile kayıtlı antrenör bulunmaktadır.");
                    await PopulateSalonlar();
                    return View(antrenor);
                }

                antrenor.AktifMi = true;
                _context.Add(antrenor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Antrenör başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateSalonlar();
            return View(antrenor);
        }

        // GET: Antrenor/Edit/5
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

            await PopulateSalonlar(antrenor.SalonId);
            return View(antrenor);
        }

        // POST: Antrenor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Antrenor antrenor)
        {
            if (id != antrenor.AntrenorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // TC Kimlik No kontrolü (kendisi hariç)
                    var tcVarMi = await _context.Antrenorler
                        .AnyAsync(a => a.TCKimlikNo == antrenor.TCKimlikNo && a.AntrenorId != id);

                    if (tcVarMi)
                    {
                        ModelState.AddModelError("TCKimlikNo", "Bu TC Kimlik No ile kayıtlı antrenör bulunmaktadır.");
                        await PopulateSalonlar(antrenor.SalonId);
                        return View(antrenor);
                    }

                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Antrenör bilgileri başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.AntrenorId))
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

            await PopulateSalonlar(antrenor.SalonId);
            return View(antrenor);
        }

        // GET: Antrenor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(m => m.AntrenorId == id);

            if (antrenor == null)
            {
                return NotFound();
            }

            // Aktif randevu kontrolü
            var aktifRandevuVarMi = await _context.Randevular
                .AnyAsync(r => r.AntrenorId == id &&
                              (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));

            ViewBag.AktifRandevuVarMi = aktifRandevuVarMi;
            ViewBag.AktifRandevuSayisi = await _context.Randevular
                .CountAsync(r => r.AntrenorId == id &&
                               (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));

            return View(antrenor);
        }

        // POST: Antrenor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                // Aktif randevu kontrolü
                var aktifRandevuVarMi = await _context.Randevular
                    .AnyAsync(r => r.AntrenorId == id &&
                                  (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));

                if (aktifRandevuVarMi)
                {
                    TempData["ErrorMessage"] = "Bu antrenöre ait aktif randevular olduğu için silinemez. Lütfen önce randevuları iptal edin.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Soft delete
                antrenor.AktifMi = false;
                _context.Update(antrenor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Antrenör başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Antrenor/Deactivate/5 (Soft delete alternatifi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                antrenor.AktifMi = false;
                _context.Update(antrenor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Antrenör pasif hale getirildi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Antrenor/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                antrenor.AktifMi = true;
                _context.Update(antrenor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Antrenör aktif hale getirildi.";
            }

            return RedirectToAction(nameof(Index), new { aktifMi = false });
        }

        // GET: Antrenor/AssignServices/5
        public async Task<IActionResult> AssignServices(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .FirstOrDefaultAsync(a => a.AntrenorId == id);

            if (antrenor == null)
            {
                return NotFound();
            }

            // Zaten atanmış hizmetler
            var atanmisHizmetler = antrenor.AntrenorHizmetler
                .Select(ah => ah.HizmetId)
                .ToList();

            // Tüm hizmetler
            var tumHizmetler = await _context.Hizmetler
                .Where(h => h.AktifMi)
                .Select(h => new SelectListItem
                {
                    Value = h.HizmetId.ToString(),
                    Text = $"{h.HizmetAdi} - {h.Ucret:C2} - {h.SureDakika} dk",
                    Selected = atanmisHizmetler.Contains(h.HizmetId)
                })
                .ToListAsync();

            ViewBag.Antrenor = antrenor;
            ViewBag.Hizmetler = tumHizmetler;
            ViewBag.AtanmisHizmetler = antrenor.AntrenorHizmetler
                .Select(ah => ah.Hizmet)
                .Where(h => h != null)
                .ToList();

            return View();
        }

        // POST: Antrenor/AssignServices
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignServices(int antrenorId, List<int> hizmetIds)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorHizmetler)
                .FirstOrDefaultAsync(a => a.AntrenorId == antrenorId);

            if (antrenor == null)
            {
                return NotFound();
            }

            // Mevcut ilişkileri temizle
            if (antrenor.AntrenorHizmetler != null)
            {
                _context.AntrenorHizmetler.RemoveRange(antrenor.AntrenorHizmetler);
            }

            // Yeni ilişkileri ekle
            foreach (var hizmetId in hizmetIds)
            {
                var antrenorHizmet = new AntrenorHizmet
                {
                    AntrenorId = antrenorId,
                    HizmetId = hizmetId
                };
                _context.AntrenorHizmetler.Add(antrenorHizmet);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hizmet atamaları başarıyla güncellendi.";
            return RedirectToAction(nameof(Details), new { id = antrenorId });
        }

        #region Yardımcı Metotlar
        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntrenorId == id);
        }

        private async Task PopulateSalonlar(int? selectedSalonId = null)
        {
            var salonlar = await _context.Salonlar
                .Select(s => new SelectListItem
                {
                    Value = s.SalonId.ToString(),
                    Text = s.SalonAdi,
                    Selected = s.SalonId == selectedSalonId
                })
                .ToListAsync();

            ViewBag.Salonlar = salonlar;
        }

        private async Task<decimal> CalculateAylikGelir(int antrenorId)
        {
            var baslangic = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var bitis = baslangic.AddMonths(1);

            var gelir = await _context.Randevular
                .Where(r => r.AntrenorId == antrenorId &&
                           r.RandevuTarihi >= baslangic &&
                           r.RandevuTarihi < bitis &&
                           r.Durum == RandevuDurumu.Tamamlandi)
                .Include(r => r.Hizmet)
                .SumAsync(r => r.Hizmet.Ucret);

            return gelir;
        }
        #endregion
    }

    #region ViewModel'ler
    public class AntrenorIstatistikViewModel
    {
        public Antrenor Antrenor { get; set; } = null!;
        public List<AylikRandevuDagilimi> AylikRandevular { get; set; } = new();
        public List<HizmetRandevuSayisi> HizmetRandevulari { get; set; } = new();
        public List<DurumDagilimi> DurumDagilimi { get; set; } = new();
        public decimal AylikGelir { get; set; }
    }
    public class DurumDagilimi
    {
        public RandevuDurumu Durum { get; set; }
        public int Sayi { get; set; }
    }
    #endregion
}