using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using YeniSalon.Data;
using YeniSalon.Models;
using System.Linq.Dynamic.Core;

namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Uye")]
    public class UyeRandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UyeRandevuController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UyeRandevu - Kullanıcının kendi randevularını listele
        public async Task<IActionResult> Index(
            int? antrenorId = null,
            int? hizmetId = null,
            string? durum = null,
            DateTime? baslangicTarihi = null,
            DateTime? bitisTarihi = null,
            string sortBy = "RandevuTarihi",
            string sortOrder = "desc",
            int page = 1,
            int pageSize = 10)
        {
            // Kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Kullanıcının sadece kendi randevularını getir
            var query = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.KullaniciId == user.Id)
                .AsQueryable();

            // Filtreleme
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
                if (sortBy == "AntrenorAdi")
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
                    Text = h.HizmetAdi
                })
                .ToListAsync();

            // Kullanıcı için sadece belirli durumlar
            var durumListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Tüm Durumlar" },
                new SelectListItem { Value = RandevuDurumu.Beklemede.ToString(), Text = "Beklemede" },
                new SelectListItem { Value = RandevuDurumu.Onaylandi.ToString(), Text = "Onaylandı" },
                new SelectListItem { Value = RandevuDurumu.Tamamlandi.ToString(), Text = "Tamamlandı" },
                new SelectListItem { Value = RandevuDurumu.IptalEdildi.ToString(), Text = "İptal Edildi" }
            };
            ViewBag.Durumlar = durumListesi;

            // İstatistikler
            ViewBag.ToplamRandevu = totalRecords;
            ViewBag.BekleyenRandevu = await _context.Randevular
                .CountAsync(r => r.KullaniciId == user.Id && r.Durum == RandevuDurumu.Beklemede);
            ViewBag.OnaylananRandevu = await _context.Randevular
                .CountAsync(r => r.KullaniciId == user.Id && r.Durum == RandevuDurumu.Onaylandi);
            ViewBag.TamamlananRandevu = await _context.Randevular
                .CountAsync(r => r.KullaniciId == user.Id && r.Durum == RandevuDurumu.Tamamlandi);

            return View(randevular);
        }

        // GET: UyeRandevu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .FirstOrDefaultAsync(m => m.RandevuId == id && m.KullaniciId == user.Id);

            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // GET: UyeRandevu/Create - Antrenör ve Hizmet Seçimi
        public async Task<IActionResult> Create()
        {
            await PopulateCreateViewBags();
            return View();
        }

        // POST: UyeRandevu/Create - İlk Adım: Antrenör ve Hizmet Seçimi
        // POST: UyeRandevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {

            //randevu.Durum = RandevuDurumu.Beklemede;
            //randevu.OlusturulmaTarihi = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    // Hizmet süresini hesapla ve bitiş tarihini ayarla
                    var hizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
                    if (hizmet != null)
                    {
                        randevu.BitisTarihi = randevu.RandevuTarihi.AddMinutes(hizmet.SureDakika);
                    }
                    else
                    {
                        // Varsayılan 60 dakika
                        randevu.BitisTarihi = randevu.RandevuTarihi.AddMinutes(60);
                    }

                    // Çakışma kontrolü
                    var isAvailable = await IsTimeSlotAvailable(randevu.AntrenorId, randevu.RandevuTarihi, randevu.BitisTarihi);

                    if (!isAvailable)
                    {
                        TempData["ErrorMessage"] = "Seçtiğiniz saat artık müsait değil. Lütfen başka bir saat seçin.";
                        await PopulateCreateViewBags();
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Add(randevu);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Randevu talebiniz başarıyla oluşturuldu. Antrenör onayı bekleniyor.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Randevu oluşturulurken bir hata oluştu: {ex.Message}";
                    await PopulateCreateViewBags();
                    return View(randevu);
                }
            }

            await PopulateCreateViewBags();
            return View(randevu);
        }

        // GET: UyeRandevu/SelectDate - İkinci Adım: Tarih ve Saat Seçimi
        public async Task<IActionResult> SelectDate(int antrenorId, int hizmetId)
        {
            var antrenor = await _context.Antrenorler.FindAsync(antrenorId);
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            if (antrenor == null || hizmet == null)
            {
                return NotFound();
            }

            // Antrenörün müsaitlik saatlerini al
            var musaitlikSaatleri = await _context.MusaitlikSaatleri
                .Where(m => m.AntrenorId == antrenorId && m.AktifMi)
                .ToListAsync();

            // Önümüzdeki 14 gün için müsait saatleri hesapla
            var availableDates = new List<AvailableDateViewModel>();
            var today = DateTime.Today;

            for (int i = 0; i < 14; i++)
            {
                var date = today.AddDays(i);
                var dayOfWeek = date.DayOfWeek;

                // Bu gün için müsaitlik saatlerini bul
                var dayMusaitlik = musaitlikSaatleri
                    .Where(m => m.Gun == dayOfWeek)
                    .ToList();

                if (dayMusaitlik.Any())
                {
                    var availableDate = new AvailableDateViewModel
                    {
                        Date = date,
                        DayName = GetDayNameTurkish(dayOfWeek),
                        AvailableSlots = new List<TimeSlotViewModel>()
                    };

                    // Her müsaitlik aralığı için slotlar oluştur
                    foreach (var musaitlik in dayMusaitlik)
                    {
                        // 30 dakikalık slotlar oluştur
                        var startTime = musaitlik.BaslangicSaati;
                        var endTime = musaitlik.BitisSaati;

                        while (startTime.Add(TimeSpan.FromMinutes(hizmet.SureDakika)) <= endTime)
                        {
                            var slotEnd = startTime.Add(TimeSpan.FromMinutes(hizmet.SureDakika));

                            // Bu slot için çakışma kontrolü
                            var slotStartDateTime = date.Add(startTime);
                            var slotEndDateTime = date.Add(slotEnd);

                            var isAvailable = await IsTimeSlotAvailable(antrenorId, slotStartDateTime, slotEndDateTime);

                            if (isAvailable)
                            {
                                availableDate.AvailableSlots.Add(new TimeSlotViewModel
                                {
                                    StartTime = startTime,
                                    EndTime = slotEnd,
                                    IsAvailable = true
                                });
                            }

                            // 30 dakika ilerle
                            startTime = startTime.Add(TimeSpan.FromMinutes(30));
                        }
                    }

                    if (availableDate.AvailableSlots.Any())
                    {
                        availableDates.Add(availableDate);
                    }
                }
            }

            ViewBag.Antrenor = antrenor;
            ViewBag.Hizmet = hizmet;
            ViewBag.AntrenorId = antrenorId;
            ViewBag.HizmetId = hizmetId;
            ViewBag.AvailableDates = availableDates;

            return View();
        }

        // POST: UyeRandevu/CreateAppointment - Son Adım: Randevu Oluşturma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment(CreateAppointmentViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Müsaitlik kontrolü
                    var isAvailable = await IsTimeSlotAvailable(model.AntrenorId, model.RandevuTarihi,
                        model.RandevuTarihi.AddMinutes(model.DurationMinutes));

                    if (!isAvailable)
                    {
                        TempData["ErrorMessage"] = "Seçtiğiniz saat artık müsait değil. Lütfen başka bir saat seçin.";
                        return RedirectToAction("SelectDate", new
                        {
                            antrenorId = model.AntrenorId,
                            hizmetId = model.HizmetId
                        });
                    }

                    // 2. Bitiş tarihini doğru hesapla
                    var hizmet = await _context.Hizmetler.FindAsync(model.HizmetId);
                    if (hizmet == null)
                    {
                        TempData["ErrorMessage"] = "Seçilen hizmet bulunamadı.";
                        return RedirectToAction("SelectDate", new
                        {
                            antrenorId = model.AntrenorId,
                            hizmetId = model.HizmetId
                        });
                    }

                    var bitisTarihi = model.RandevuTarihi.AddMinutes(hizmet.SureDakika);

                    var randevu = new Randevu
                    {
                        KullaniciId = user.Id,
                        AntrenorId = model.AntrenorId,
                        HizmetId = model.HizmetId,
                        RandevuTarihi = model.RandevuTarihi,
                        BitisTarihi = bitisTarihi, // Bitiş tarihini ekle
                        Durum = RandevuDurumu.Beklemede,
                        OlusturulmaTarihi = DateTime.Now,
                        Not = model.Not
                    };

                    _context.Add(randevu);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Randevu talebiniz başarıyla oluşturuldu. Antrenör onayı bekleniyor.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Randevu oluşturulurken bir hata oluştu: {ex.Message}";
                    return RedirectToAction("SelectDate", new
                    {
                        antrenorId = model.AntrenorId,
                        hizmetId = model.HizmetId
                    });
                }
            }

            TempData["ErrorMessage"] = "Randevu oluşturulurken bir hata oluştu.";
            return RedirectToAction("SelectDate", new
            {
                antrenorId = model.AntrenorId,
                hizmetId = model.HizmetId
            });
        }

        // GET: UyeRandevu/Edit/5 - Sadece beklemede durumundaki randevular düzenlenebilir
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var randevu = await _context.Randevular
                .FirstOrDefaultAsync(r => r.RandevuId == id &&
                                        r.KullaniciId == user.Id &&
                                        r.Durum == RandevuDurumu.Beklemede);

            if (randevu == null)
            {
                TempData["ErrorMessage"] = "Sadece beklemede durumundaki randevularınızı düzenleyebilirsiniz.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateEditViewBags(randevu);
            return View(randevu);
        }

        // POST: UyeRandevu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Randevu randevu)
        {
            if (id != randevu.RandevuId)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Kullanıcı ID'sini ve durumu koru
            randevu.KullaniciId = user.Id;
            randevu.Durum = RandevuDurumu.Beklemede;

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

                    TempData["SuccessMessage"] = "Randevunuz başarıyla güncellendi.";
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

        // POST: UyeRandevu/Cancel/5 - Randevu iptal etme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var randevu = await _context.Randevular
                .FirstOrDefaultAsync(r => r.RandevuId == id && r.KullaniciId == user.Id);

            if (randevu == null)
            {
                TempData["ErrorMessage"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Sadece beklemede veya onaylanmış randevular iptal edilebilir
            if (randevu.Durum != RandevuDurumu.Beklemede && randevu.Durum != RandevuDurumu.Onaylandi)
            {
                TempData["ErrorMessage"] = "Sadece beklemede veya onaylanmış randevularınızı iptal edebilirsiniz.";
                return RedirectToAction(nameof(Details), new { id });
            }

            randevu.Durum = RandevuDurumu.IptalEdildi;

            if (!string.IsNullOrEmpty(reason))
            {
                randevu.Not = $"Üye iptal nedeni: {reason}";
            }

            _context.Update(randevu);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Randevunuz başarıyla iptal edildi.";
            return RedirectToAction(nameof(Index));
        }


        // Müsaitlik Saati Kontrolü
        // UyeRandevuController.cs içinde

        // GET: UyeRandevu/CheckAvailability
        public async Task<IActionResult> CheckAvailability(int antrenorId, DateTime randevuTarihi, int sureDakika)
        {
            try
            {
                Console.WriteLine($"CheckAvailability çağrıldı: AntrenorId={antrenorId}, Tarih={randevuTarihi}, Süre={sureDakika}");

                var baslangic = randevuTarihi;
                var bitis = randevuTarihi.AddMinutes(sureDakika);

                Console.WriteLine($"Başlangıç: {baslangic}, Bitiş: {bitis}");

                // 1. ÖNCE MÜSAİTLİK SAATLERİNİ KONTROL ET
                var gun = randevuTarihi.DayOfWeek;
                var saat = randevuTarihi.TimeOfDay;

                Console.WriteLine($"Gün: {gun}, Saat: {saat}");

                var musaitlikVarMi = await _context.MusaitlikSaatleri
                    .AnyAsync(m => m.AntrenorId == antrenorId &&
                                  m.Gun == gun &&
                                  m.AktifMi &&
                                  m.BaslangicSaati <= saat &&
                                  m.BitisSaati >= bitis.TimeOfDay);

                Console.WriteLine($"Müsaitlik var mı: {musaitlikVarMi}");

                if (!musaitlikVarMi)
                {
                    return Json(new
                    {
                        Musait = false,
                        CakisanRandevular = new List<object>(),
                        Mesaj = "Antrenör bu gün ve saatte müsait değil (müsaitlik saati yok)."
                    });
                }

                // 2. ÇAKIŞAN RANDEVULARI KONTROL ET
                // TÜM AKTİF RANDEVULARI AL (sadece Onaylandı ve Beklemede değil, ama IptalEdildi, Reddedildi, Gelmeyen hariç)
                var tumAktifRandevular = await _context.Randevular
                    .Where(r => r.AntrenorId == antrenorId &&
                               r.Durum != RandevuDurumu.IptalEdildi &&
                               r.Durum != RandevuDurumu.Reddedildi &&
                               r.Durum != RandevuDurumu.Gelmeyen)
                    .Include(r => r.Kullanici)
                    .Include(r => r.Hizmet)
                    .ToListAsync();

                Console.WriteLine($"Toplam aktif randevu sayısı: {tumAktifRandevular.Count}");

                var gercekCakisma = new List<Randevu>();

                foreach (var randevu in tumAktifRandevular)
                {
                    // Bitiş tarihi kontrolü - null veya default ise hizmet süresine göre hesapla
                    var randevuBitis = randevu.BitisTarihi;

                    if (randevuBitis == default(DateTime) || randevuBitis == DateTime.MinValue)
                    {
                        // Hizmet süresini bul
                        var hizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
                        randevuBitis = hizmet != null
                            ? randevu.RandevuTarihi.AddMinutes(hizmet.SureDakika)
                            : randevu.RandevuTarihi.AddHours(1);
                    }

                    Console.WriteLine($"Randevu kontrol: {randevu.RandevuId}, Başlangıç: {randevu.RandevuTarihi}, Bitiş: {randevuBitis}, Durum: {randevu.Durum}");

                    // Çakışma kontrolü
                    var cakismaVarMi = (baslangic >= randevu.RandevuTarihi && baslangic < randevuBitis) ||
                                      (bitis > randevu.RandevuTarihi && bitis <= randevuBitis) ||
                                      (baslangic <= randevu.RandevuTarihi && bitis >= randevuBitis);

                    if (cakismaVarMi)
                    {
                        Console.WriteLine($"ÇAKIŞMA BULUNDU: Randevu ID: {randevu.RandevuId}");
                        gercekCakisma.Add(randevu);
                    }
                }

                Console.WriteLine($"Toplam çakışma sayısı: {gercekCakisma.Count}");

                var antrenor = await _context.Antrenorler.FindAsync(antrenorId);

                if (gercekCakisma.Any())
                {
                    return Json(new
                    {
                        Musait = false,
                        CakisanRandevular = gercekCakisma.Select(r => new
                        {
                            Kullanici = r.Kullanici != null ? $"{r.Kullanici.Ad} {r.Kullanici.Soyad}" : "Bilinmeyen Üye",
                            Hizmet = r.Hizmet?.HizmetAdi ?? "Bilinmeyen Hizmet",
                            Baslangic = r.RandevuTarihi.ToString("dd.MM.yyyy HH:mm"),
                            Bitis = (r.BitisTarihi != default && r.BitisTarihi != DateTime.MinValue
                                    ? r.BitisTarihi
                                    : r.RandevuTarihi.AddHours(1)).ToString("dd.MM.yyyy HH:mm"),
                            Durum = r.Durum.ToString()
                        }),
                        Antrenor = antrenor != null ? $"{antrenor.Ad} {antrenor.Soyad}" : "Bilinmiyor",
                        Mesaj = "Çakışan randevu bulundu."
                    });
                }
                else
                {
                    return Json(new
                    {
                        Musait = true,
                        CakisanRandevular = new List<object>(),
                        Antrenor = antrenor != null ? $"{antrenor.Ad} {antrenor.Soyad}" : "Bilinmiyor",
                        Mesaj = "Antrenör müsait."
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CheckAvailability hatası: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return Json(new
                {
                    Musait = false,
                    CakisanRandevular = new List<object>(),
                    Mesaj = $"Müsaitlik kontrolü sırasında bir hata oluştu: {ex.Message}"
                });
            }
        }

        // Geçici olarak bir action oluşturabilirsiniz (sadece debug için):
        [HttpGet]
        public async Task<IActionResult> FixBitisTarihleri()
        {
            try
            {
                var randevular = await _context.Randevular
                    .Where(r => r.BitisTarihi == null || r.BitisTarihi == DateTime.MinValue)
                    .Include(r => r.Hizmet)
                    .ToListAsync();

                int fixedCount = 0;
                foreach (var randevu in randevular)
                {
                    if (randevu.Hizmet != null)
                    {
                        randevu.BitisTarihi = randevu.RandevuTarihi.AddMinutes(randevu.Hizmet.SureDakika);
                        fixedCount++;
                    }
                    else
                    {
                        // Hizmet bulunamazsa varsayılan 60 dakika
                        randevu.BitisTarihi = randevu.RandevuTarihi.AddMinutes(60);
                        fixedCount++;
                    }
                }

                if (fixedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    return Content($"{fixedCount} randevunun bitiş tarihi düzeltildi.");
                }
                else
                {
                    return Content("Düzeltilecek randevu bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                return Content($"Hata: {ex.Message}");
            }
        }


        // GET: UyeRandevu/Calendar - Takvim görünümü
        public async Task<IActionResult> Calendar(int? antrenorId = null, DateTime? tarih = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var gosterilecekTarih = tarih ?? DateTime.Today;
            var baslangicTarihi = new DateTime(gosterilecekTarih.Year, gosterilecekTarih.Month, 1);
            var bitisTarihi = baslangicTarihi.AddMonths(1).AddDays(-1);

            var query = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.KullaniciId == user.Id &&
                           r.RandevuTarihi >= baslangicTarihi &&
                           r.RandevuTarihi <= bitisTarihi &&
                           (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));

            if (antrenorId.HasValue)
            {
                query = query.Where(r => r.AntrenorId == antrenorId.Value);
            }

            var randevular = await query
                .OrderBy(r => r.RandevuTarihi)
                .ToListAsync();

            // Takvim verilerini hazırla
            var takvimVerileri = new List<UyeTakvimRandevuViewModel>();

            foreach (var randevu in randevular)
            {
                takvimVerileri.Add(new UyeTakvimRandevuViewModel
                {
                    Id = randevu.RandevuId,
                    Title = $"{randevu.Antrenor?.Ad} - {randevu.Hizmet?.HizmetAdi}",
                    Start = randevu.RandevuTarihi,
                    End = randevu.BitisTarihi,
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

        // GET: UyeRandevu/AntrenorMusaitlik - Antrenör müsaitlik saatlerini görüntüle
        public async Task<IActionResult> AntrenorMusaitlik(int antrenorId)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.MusaitlikSaatleri)
                .FirstOrDefaultAsync(a => a.AntrenorId == antrenorId && a.AktifMi);

            if (antrenor == null)
            {
                return NotFound();
            }

            // Müsaitlik saatlerini günlere göre grupla
            var musaitlikGruplari = antrenor.MusaitlikSaatleri
                .Where(m => m.AktifMi)
                .GroupBy(m => m.Gun)
                .Select(g => new AntrenorMusaitlikViewModel
                {
                    Gun = g.Key,
                    GunAdi = GetDayNameTurkish(g.Key),
                    Saatler = g.Select(m => new MusaitlikSaatiViewModel
                    {
                        Baslangic = m.BaslangicSaati,
                        Bitis = m.BitisSaati
                    }).ToList()
                })
                .OrderBy(g => (int)g.Gun)
                .ToList();

            ViewBag.Antrenor = antrenor;
            ViewBag.MusaitlikGruplari = musaitlikGruplari;

            return View();
        }

        // Yardımcı Metotlar
        // UyeRandevuController.cs içine yeni action'lar ekleyin

        public async Task<IActionResult> GetAvailableDates(int antrenorId, int hizmetId)
        {
            try
            {
                var antrenor = await _context.Antrenorler.FindAsync(antrenorId);
                var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

                if (antrenor == null || hizmet == null)
                {
                    return Json(new { success = false, message = "Antrenör veya hizmet bulunamadı." });
                }

                var musaitlikSaatleri = await _context.MusaitlikSaatleri
                    .Where(m => m.AntrenorId == antrenorId && m.AktifMi)
                    .ToListAsync();

                var availableDates = new List<dynamic>();
                var today = DateTime.Today;
                var endDate = today.AddDays(30); // 30 gün ilerisine bak

                for (var date = today.AddDays(1); date <= endDate; date = date.AddDays(1))
                {
                    var dayOfWeek = date.DayOfWeek;

                    // Bugün ve geçmiş tarihleri atla
                    if (date.Date <= today.Date)
                        continue;

                    // Bu gün için müsaitlik var mı kontrol et
                    var dayMusaitlik = musaitlikSaatleri
                        .Where(m => m.Gun == dayOfWeek)
                        .ToList();

                    if (dayMusaitlik.Any())
                    {
                        // Bu tarihte en az bir müsait saat var mı kontrol et
                        var hasAvailableSlot = false;

                        foreach (var musaitlik in dayMusaitlik)
                        {
                            // Hizmet süresine göre slotlar oluştur
                            var startTime = musaitlik.BaslangicSaati;
                            var endTime = musaitlik.BitisSaati;

                            while (startTime.Add(TimeSpan.FromMinutes(hizmet.SureDakika)) <= endTime)
                            {
                                var slotEnd = startTime.Add(TimeSpan.FromMinutes(hizmet.SureDakika));
                                var slotStartDateTime = date.Add(startTime);
                                var slotEndDateTime = date.Add(slotEnd);

                                // Çakışma kontrolü
                                var isAvailable = await IsTimeSlotAvailable(antrenorId, slotStartDateTime, slotEndDateTime);

                                if (isAvailable)
                                {
                                    hasAvailableSlot = true;
                                    break;
                                }

                                startTime = startTime.Add(TimeSpan.FromMinutes(30));
                            }

                            if (hasAvailableSlot) break;
                        }

                        if (hasAvailableSlot)
                        {
                            availableDates.Add(new
                            {
                                Date = date.ToString("yyyy-MM-dd"),
                                DisplayText = $"{GetDayNameTurkish(dayOfWeek)}, {date:dd MMMM yyyy}",
                                DayName = GetDayNameTurkish(dayOfWeek)
                            });
                        }
                    }
                }

                return Json(new { success = true, dates = availableDates });
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapılabilir
                return Json(new { success = false, message = $"Bir hata oluştu: {ex.Message}" });
            }
        }

        // GET: Müsait Saatleri Getir
        public async Task<IActionResult> GetAvailableTimes(int antrenorId, int hizmetId, string date)
        {
            var parsedDate = DateTime.Parse(date);
            var antrenor = await _context.Antrenorler.FindAsync(antrenorId);
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            if (antrenor == null || hizmet == null)
            {
                return Json(new { success = false, message = "Antrenör veya hizmet bulunamadı." });
            }

            var dayOfWeek = parsedDate.DayOfWeek;
            var musaitlikSaatleri = await _context.MusaitlikSaatleri
                .Where(m => m.AntrenorId == antrenorId && m.Gun == dayOfWeek && m.AktifMi)
                .ToListAsync();

            var availableTimes = new List<dynamic>();

            foreach (var musaitlik in musaitlikSaatleri)
            {
                var startTime = musaitlik.BaslangicSaati;
                var endTime = musaitlik.BitisSaati;

                // 30 dakikalık slotlar oluştur
                while (startTime.Add(TimeSpan.FromMinutes(hizmet.SureDakika)) <= endTime)
                {
                    var slotEnd = startTime.Add(TimeSpan.FromMinutes(hizmet.SureDakika));
                    var slotStartDateTime = parsedDate.Add(startTime);
                    var slotEndDateTime = parsedDate.Add(slotEnd);

                    // Çakışma kontrolü
                    var isAvailable = await IsTimeSlotAvailable(antrenorId, slotStartDateTime, slotEndDateTime);

                    if (isAvailable)
                    {
                        var startTimeString = startTime.ToString(@"hh\:mm");
                        var endTimeString = slotEnd.ToString(@"hh\:mm");

                        availableTimes.Add(new
                        {
                            Time = startTimeString,
                            DisplayText = $"{startTimeString} - {endTimeString}",
                            DateTime = slotStartDateTime.ToString("yyyy-MM-ddTHH:mm")
                        });
                    }

                    startTime = startTime.Add(TimeSpan.FromMinutes(30));
                }
            }

            // Saatlere göre sırala
            availableTimes = availableTimes.OrderBy(t => t.Time).ToList();

            return Json(new { success = true, times = availableTimes });
        }

        // Çakışma kontrolü için yardımcı metod
        private async Task<bool> IsTimeSlotAvailable(int antrenorId, DateTime startTime, DateTime endTime)
        {
            try
            {
                // TÜM AKTİF RANDEVULARI AL
                var tumAktifRandevular = await _context.Randevular
                    .Where(r => r.AntrenorId == antrenorId &&
                               r.Durum != RandevuDurumu.IptalEdildi &&
                               r.Durum != RandevuDurumu.Reddedildi &&
                               r.Durum != RandevuDurumu.Gelmeyen)
                    .ToListAsync();

                foreach (var randevu in tumAktifRandevular)
                {
                    // Bitiş tarihi kontrolü
                    var randevuBitis = randevu.BitisTarihi;

                    if (randevuBitis == default(DateTime) || randevuBitis == DateTime.MinValue)
                    {
                        var hizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
                        randevuBitis = hizmet != null
                            ? randevu.RandevuTarihi.AddMinutes(hizmet.SureDakika)
                            : randevu.RandevuTarihi.AddHours(1);
                    }

                    // Çakışma kontrolü
                    var cakismaVarMi = (startTime >= randevu.RandevuTarihi && startTime < randevuBitis) ||
                                      (endTime > randevu.RandevuTarihi && endTime <= randevuBitis) ||
                                      (startTime <= randevu.RandevuTarihi && endTime >= randevuBitis);

                    if (cakismaVarMi)
                    {
                        return false; // Çakışma var
                    }
                }

                return true; // Çakışma yok
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsTimeSlotAvailable hatası: {ex.Message}");
                return false;
            }
        }

        private string GetDayNameTurkish(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Pazartesi",
                DayOfWeek.Tuesday => "Salı",
                DayOfWeek.Wednesday => "Çarşamba",
                DayOfWeek.Thursday => "Perşembe",
                DayOfWeek.Friday => "Cuma",
                DayOfWeek.Saturday => "Cumartesi",
                DayOfWeek.Sunday => "Pazar",
                _ => day.ToString()
            };
        }

        private async Task PopulateCreateViewBags()
        {
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

            var user = await _userManager.GetUserAsync(User);
            if(user != null)
                ViewBag.userId = user.Id;
        }

        private async Task PopulateEditViewBags(Randevu randevu)
        {
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
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.RandevuId == id);
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
    }

    // ViewModel'ler
    public class CreateRandevuViewModel
    {
        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Hizmet seçimi zorunludur")]
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }
    }

    public class CreateAppointmentViewModel
    {
        [Required]
        public int AntrenorId { get; set; }

        [Required]
        public int HizmetId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi seçimi zorunludur")]
        [Display(Name = "Randevu Tarihi")]
        public DateTime RandevuTarihi { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Display(Name = "Not")]
        [MaxLength(500)]
        public string? Not { get; set; }
    }

    public class AvailableDateViewModel
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public List<TimeSlotViewModel> AvailableSlots { get; set; } = new();
    }

    public class TimeSlotViewModel
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class AntrenorMusaitlikViewModel
    {
        public DayOfWeek Gun { get; set; }
        public string GunAdi { get; set; } = string.Empty;
        public List<MusaitlikSaatiViewModel> Saatler { get; set; } = new();
    }

    public class MusaitlikSaatiViewModel
    {
        public TimeSpan Baslangic { get; set; }
        public TimeSpan Bitis { get; set; }
    }

    public class UyeTakvimRandevuViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string AntrenorAdi { get; set; } = string.Empty;
        public string HizmetAdi { get; set; } = string.Empty;
        public RandevuDurumu Durum { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}