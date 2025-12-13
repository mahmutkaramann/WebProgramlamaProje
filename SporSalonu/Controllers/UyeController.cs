using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;     // Dinamik LINQ kullanımı için
using YeniSalon.Data;
using YeniSalon.Models;

// Filtreleme: Arama, cinsiyet, üyelik türü, durum
// Sıralama: Tüm sütunlarda sıralama
// Sayfalama: 10'ar 10'ar listeleme


namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UyeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UyeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Uye
        public async Task<IActionResult> Index(
            string search = "",
            string cinsiyet = "",
            string uyelikTuru = "",
            bool aktifMi = true,
            string sortBy = "Ad",
            string sortOrder = "asc",
            int page = 1,
            int pageSize = 10)
        {
            // Sorgu oluştur
            var query = _context.Users.AsQueryable();

            // Filtreleme
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Ad.Contains(search) ||
                    u.Soyad.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.TCKimlikNo.Contains(search) ||
                    u.Adres.Contains(search));
            }

            if (!string.IsNullOrEmpty(cinsiyet))
            {
                if (Enum.TryParse<Cinsiyet>(cinsiyet, out var cinsiyetEnum))
                {
                    query = query.Where(u => u.Cinsiyet == cinsiyetEnum);
                }
            }

            if (!string.IsNullOrEmpty(uyelikTuru))
            {
                if (Enum.TryParse<UyelikTuru>(uyelikTuru, out var uyelikTuruEnum))
                {
                    query = query.Where(u => u.UyelikTuru == uyelikTuruEnum);
                }
            }

            query = query.Where(u => u.AktifMi == aktifMi);

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
            var uyeler = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewBag'ler
            ViewBag.Search = search;
            ViewBag.Cinsiyet = cinsiyet;
            ViewBag.UyelikTuru = uyelikTuru;
            ViewBag.AktifMi = aktifMi;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;

            // Filtre seçenekleri
            ViewBag.CinsiyetListesi = new SelectList(Enum.GetValues(typeof(Cinsiyet)));
            ViewBag.UyelikTuruListesi = new SelectList(Enum.GetValues(typeof(UyelikTuru)));

            return View(uyeler);
        }

        // GET: Uye/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uye = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uye == null)
            {
                return NotFound();
            }

            // Üyenin randevularını getir
            var randevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.KullaniciId == id)
                .OrderByDescending(r => r.RandevuTarihi)
                .Take(10)
                .ToListAsync();

            // Üyenin AI önerilerini getir
            var aiOnerileri = await _context.AIEgzersizOnerileri
                .Where(a => a.KullaniciId == id)
                .OrderByDescending(a => a.OlusturulmaTarihi)
                .Take(5)
                .ToListAsync();

            ViewBag.Randevular = randevular;
            ViewBag.AIOnerileri = aiOnerileri;

            // Üyenin rolünü belirle
            var roles = await _userManager.GetRolesAsync(uye);
            ViewBag.Rol = roles.FirstOrDefault() ?? "Uye";

            return View(uye);
        }

        // GET: Uye/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Uye/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUyeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TC Kimlik No kontrolü
                var tcVarMi = await _context.Users
                    .AnyAsync(u => u.TCKimlikNo == model.TCKimlikNo);

                if (tcVarMi)
                {
                    ModelState.AddModelError("TCKimlikNo", "Bu TC Kimlik No ile kayıtlı üye bulunmaktadır.");
                    return View(model);
                }

                // Email kontrolü
                var emailVarMi = await _context.Users
                    .AnyAsync(u => u.Email == model.Email);

                if (emailVarMi)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi ile kayıtlı üye bulunmaktadır.");
                    return View(model);
                }

                var uye = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    TCKimlikNo = model.TCKimlikNo,
                    DogumTarihi = model.DogumTarihi,
                    Cinsiyet = model.Cinsiyet,
                    Adres = model.Adres,
                    Boy = model.Boy,
                    Kilo = model.Kilo,
                    UyelikBaslangic = model.UyelikBaslangic,
                    UyelikBitis = model.UyelikBitis,
                    UyelikTuru = model.UyelikTuru,
                    ProfilFotoUrl = model.ProfilFotoUrl,
                    AktifMi = true,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var result = await _userManager.CreateAsync(uye, model.Password);

                if (result.Succeeded)
                {
                    // Varsayılan olarak "Uye" rolünü ata
                    await _userManager.AddToRoleAsync(uye, "Uye");

                    TempData["SuccessMessage"] = "Üye başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Uye/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uye = await _context.Users.FindAsync(id);
            if (uye == null)
            {
                return NotFound();
            }

            var model = new EditUyeViewModel
            {
                Id = uye.Id,
                Email = uye.Email,
                Ad = uye.Ad,
                Soyad = uye.Soyad,
                TCKimlikNo = uye.TCKimlikNo,
                DogumTarihi = uye.DogumTarihi,
                Cinsiyet = uye.Cinsiyet,
                Adres = uye.Adres,
                Boy = uye.Boy,
                Kilo = uye.Kilo,
                UyelikBaslangic = uye.UyelikBaslangic,
                UyelikBitis = uye.UyelikBitis,
                UyelikTuru = uye.UyelikTuru,
                ProfilFotoUrl = uye.ProfilFotoUrl,
                AktifMi = uye.AktifMi
            };

            // Kullanıcının rolünü al
            var roles = await _userManager.GetRolesAsync(uye);
            model.Rol = roles.FirstOrDefault() ?? "Uye";

            ViewBag.Roller = new SelectList(new[] { "Uye", "Admin" }, model.Rol);

            return View(model);
        }

        // POST: Uye/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUyeViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var uye = await _context.Users.FindAsync(id);
                    if (uye == null)
                    {
                        return NotFound();
                    }

                    // TC Kimlik No kontrolü (başkasına ait olup olmadığı)
                    var tcVarMi = await _context.Users
                        .AnyAsync(u => u.TCKimlikNo == model.TCKimlikNo && u.Id != id);

                    if (tcVarMi)
                    {
                        ModelState.AddModelError("TCKimlikNo", "Bu TC Kimlik No ile kayıtlı başka bir üye bulunmaktadır.");
                        ViewBag.Roller = new SelectList(new[] { "Uye", "Admin" }, model.Rol);
                        return View(model);
                    }

                    // Email kontrolü (başkasına ait olup olmadığı)
                    var emailVarMi = await _context.Users
                        .AnyAsync(u => u.Email == model.Email && u.Id != id);

                    if (emailVarMi)
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi ile kayıtlı başka bir üye bulunmaktadır.");
                        ViewBag.Roller = new SelectList(new[] { "Uye", "Admin" }, model.Rol);
                        return View(model);
                    }

                    // Bilgileri güncelle
                    uye.Email = model.Email;
                    uye.UserName = model.Email;
                    uye.Ad = model.Ad;
                    uye.Soyad = model.Soyad;
                    uye.TCKimlikNo = model.TCKimlikNo;
                    uye.DogumTarihi = model.DogumTarihi;
                    uye.Cinsiyet = model.Cinsiyet;
                    uye.Adres = model.Adres;
                    uye.Boy = model.Boy;
                    uye.Kilo = model.Kilo;
                    uye.UyelikBaslangic = model.UyelikBaslangic;
                    uye.UyelikBitis = model.UyelikBitis;
                    uye.UyelikTuru = model.UyelikTuru;
                    uye.ProfilFotoUrl = model.ProfilFotoUrl;
                    uye.AktifMi = model.AktifMi;

                    // Şifre değişikliği
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(uye);
                        var result = await _userManager.ResetPasswordAsync(uye, token, model.NewPassword);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            ViewBag.Roller = new SelectList(new[] { "Uye", "Admin" }, model.Rol);
                            return View(model);
                        }
                    }

                    // Rol değişikliği
                    var currentRoles = await _userManager.GetRolesAsync(uye);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(uye, currentRoles);
                    }
                    await _userManager.AddToRoleAsync(uye, model.Rol);

                    _context.Update(uye);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Üye bilgileri başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UyeExists(id))
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

            ViewBag.Roller = new SelectList(new[] { "Uye", "Admin" }, model.Rol);
            return View(model);
        }

        // GET: Uye/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uye = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uye == null)
            {
                return NotFound();
            }

            return View(uye);
        }

        // POST: Uye/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var uye = await _context.Users.FindAsync(id);
            if (uye != null)
            {
                // Üyenin aktif randevuları var mı kontrol et
                var aktifRandevular = await _context.Randevular
                    .AnyAsync(r => r.KullaniciId == id &&
                                  (r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi));

                if (aktifRandevular)
                {
                    TempData["ErrorMessage"] = "Bu üyenin aktif randevuları olduğu için silinemez. Lütfen önce randevuları iptal edin.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // AI önerilerini sil
                var aiOnerileri = await _context.AIEgzersizOnerileri
                    .Where(a => a.KullaniciId == id)
                    .ToListAsync();

                if (aiOnerileri.Any())
                {
                    _context.AIEgzersizOnerileri.RemoveRange(aiOnerileri);
                }

                // Üyeyi sil
                _context.Users.Remove(uye);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Üye başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Uye/Deactivate/5 (Soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var uye = await _context.Users.FindAsync(id);
            if (uye != null)
            {
                uye.AktifMi = false;
                _context.Update(uye);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Üye pasif hale getirildi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Uye/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var uye = await _context.Users.FindAsync(id);
            if (uye != null)
            {
                uye.AktifMi = true;
                _context.Update(uye);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Üye aktif hale getirildi.";
            }

            return RedirectToAction(nameof(Index), new { aktifMi = false });
        }

        // GET: Uye/ResetPassword/5
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uye = await _context.Users.FindAsync(id);
            if (uye == null)
            {
                return NotFound();
            }

            ViewBag.UyeAdi = $"{uye.Ad} {uye.Soyad}";
            ViewBag.UyeId = id;

            return View();
        }

        // POST: Uye/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var uye = await _context.Users.FindAsync(id);
                if (uye == null)
                {
                    return NotFound();
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(uye);
                var result = await _userManager.ResetPasswordAsync(uye, token, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Şifre başarıyla sıfırlandı.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var uyeBilgi = await _context.Users.FindAsync(id);
            ViewBag.UyeAdi = $"{uyeBilgi?.Ad} {uyeBilgi?.Soyad}";
            ViewBag.UyeId = id;

            return View(model);
        }

        // GET: Uye/Statistics
        public async Task<IActionResult> Statistics()
        {
            var istatistikler = new UyeIstatistikleriViewModel
            {
                ToplamUye = await _context.Users.CountAsync(),
                AktifUye = await _context.Users.CountAsync(u => u.AktifMi),
                PasifUye = await _context.Users.CountAsync(u => !u.AktifMi),
                ErkekUye = await _context.Users.CountAsync(u => u.Cinsiyet == Cinsiyet.Erkek),
                KadinUye = await _context.Users.CountAsync(u => u.Cinsiyet == Cinsiyet.Kadın),
                YeniUyeler = await _context.Users
                    .Where(u => u.UyelikBaslangic >= DateTime.Now.AddDays(-30))
                    .CountAsync()
            };

            // Üyelik türlerine göre dağılım
            istatistikler.UyelikTuruDagilimi = await _context.Users
                .Where(u => u.UyelikTuru != null)
                .GroupBy(u => u.UyelikTuru)
                .Select(g => new UyelikTuruDagilimi
                {
                    UyelikTuru = g.Key!.Value,
                    Sayi = g.Count()
                })
                .ToListAsync();

            // Aylık üye artışı
            var son6Ay = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(d => new { Yil = d.Year, Ay = d.Month })
                .ToList();

            istatistikler.AylikUyeArtisi = new List<AylikUyeArtisi>();

            foreach (var ay in son6Ay)
            {
                var ayUye = await _context.Users
                    .CountAsync(u => u.UyelikBaslangic != null &&
                                   u.UyelikBaslangic.Value.Year == ay.Yil &&
                                   u.UyelikBaslangic.Value.Month == ay.Ay);

                istatistikler.AylikUyeArtisi.Add(new AylikUyeArtisi
                {
                    Ay = $"{ay.Yil}-{ay.Ay:D2}",
                    UyeSayisi = ayUye
                });
            }

            istatistikler.AylikUyeArtisi = istatistikler.AylikUyeArtisi
                .OrderBy(a => a.Ay)
                .ToList();

            return View(istatistikler);
        }

        // Export to Excel (Basit CSV)
        public async Task<IActionResult> ExportToCsv()
        {
            var uyeler = await _context.Users
                .Where(u => u.AktifMi)
                .Select(u => new
                {
                    u.Ad,
                    u.Soyad,
                    u.Email,
                    u.TCKimlikNo,
                    u.DogumTarihi,
                    Cinsiyet = u.Cinsiyet.ToString(),
                    u.Adres,
                    u.Boy,
                    u.Kilo,
                    UyelikTuru = u.UyelikTuru.HasValue ? u.UyelikTuru.Value.ToString() : "",
                    u.UyelikBaslangic,
                    u.UyelikBitis
                })
                .ToListAsync();

            var csv = "Ad,Soyad,Email,TC Kimlik No,Doğum Tarihi,Cinsiyet,Adres,Boy,Kilo,Üyelik Türü,Üyelik Başlangıç,Üyelik Bitiş\n";

            foreach (var uye in uyeler)
            {
                csv += $"\"{uye.Ad}\",\"{uye.Soyad}\",\"{uye.Email}\",\"{uye.TCKimlikNo}\",\"{uye.DogumTarihi:yyyy-MM-dd}\",\"{uye.Cinsiyet}\",\"{uye.Adres}\",{uye.Boy},{uye.Kilo},\"{uye.UyelikTuru}\",\"{uye.UyelikBaslangic:yyyy-MM-dd}\",\"{uye.UyelikBitis:yyyy-MM-dd}\"\n";
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"Uyeler_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private bool UyeExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }

    // ViewModel'ler
    public class CreateUyeViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [Display(Name = "TC Kimlik No")]
        public string TCKimlikNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi zorunludur")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime DogumTarihi { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur")]
        [Display(Name = "Cinsiyet")]
        public Cinsiyet Cinsiyet { get; set; }

        [Required(ErrorMessage = "Adres zorunludur")]
        [Display(Name = "Adres")]
        public string Adres { get; set; } = string.Empty;

        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int? Boy { get; set; }

        [Range(30, 200, ErrorMessage = "Kilo 30-200 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public decimal? Kilo { get; set; }

        [Display(Name = "Üyelik Başlangıç")]
        [DataType(DataType.Date)]
        public DateTime? UyelikBaslangic { get; set; }

        [Display(Name = "Üyelik Bitiş")]
        [DataType(DataType.Date)]
        public DateTime? UyelikBitis { get; set; }

        [Display(Name = "Üyelik Türü")]
        public UyelikTuru? UyelikTuru { get; set; }

        [Display(Name = "Profil Fotoğrafı URL")]
        public string? ProfilFotoUrl { get; set; }
    }

    public class EditUyeViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [Display(Name = "TC Kimlik No")]
        public string TCKimlikNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi zorunludur")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime DogumTarihi { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur")]
        [Display(Name = "Cinsiyet")]
        public Cinsiyet Cinsiyet { get; set; }

        [Required(ErrorMessage = "Adres zorunludur")]
        [Display(Name = "Adres")]
        public string Adres { get; set; } = string.Empty;

        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int? Boy { get; set; }

        [Range(30, 200, ErrorMessage = "Kilo 30-200 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public decimal? Kilo { get; set; }

        [Display(Name = "Üyelik Başlangıç")]
        [DataType(DataType.Date)]
        public DateTime? UyelikBaslangic { get; set; }

        [Display(Name = "Üyelik Bitiş")]
        [DataType(DataType.Date)]
        public DateTime? UyelikBitis { get; set; }

        [Display(Name = "Üyelik Türü")]
        public UyelikTuru? UyelikTuru { get; set; }

        [Display(Name = "Profil Fotoğrafı URL")]
        public string? ProfilFotoUrl { get; set; }

        [Display(Name = "Yeni Şifre")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string? ConfirmNewPassword { get; set; }

        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Uye";

        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; } = true;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Yeni şifre zorunludur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class UyeIstatistikleriViewModel
    {
        public int ToplamUye { get; set; }
        public int AktifUye { get; set; }
        public int PasifUye { get; set; }
        public int ErkekUye { get; set; }
        public int KadinUye { get; set; }
        public int YeniUyeler { get; set; }
        public List<UyelikTuruDagilimi> UyelikTuruDagilimi { get; set; } = new();
        public List<AylikUyeArtisi> AylikUyeArtisi { get; set; } = new();
    }

    public class UyelikTuruDagilimi
    {
        public UyelikTuru UyelikTuru { get; set; }
        public int Sayi { get; set; }
    }

    public class AylikUyeArtisi
    {
        public string Ay { get; set; } = string.Empty;
        public int UyeSayisi { get; set; }
    }
}