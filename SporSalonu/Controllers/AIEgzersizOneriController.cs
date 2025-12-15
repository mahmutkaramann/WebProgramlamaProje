using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YeniSalon.Data;
using YeniSalon.Models;
using YeniSalon.Services;
using System.ComponentModel.DataAnnotations;

namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Uye")]
    public class AIEgzersizOneriController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOpenAIService _openAIService;
        private readonly IWebHostEnvironment _environment;

        public AIEgzersizOneriController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOpenAIService openAIService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _openAIService = openAIService;
            _environment = environment;
        }

        // GET: AIEgzersizOneri
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var query = _context.AIEgzersizOnerileri
                .Where(o => o.KullaniciId == user.Id)
                .OrderByDescending(o => o.OlusturulmaTarihi);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var oneriler = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;

            return View(oneriler);
        }

        // GET: AIEgzersizOneri/Create
        public IActionResult Create()
        {
            return View(new AIEgzersizOneriViewModel());
        }

        // POST: AIEgzersizOneri/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AIEgzersizOneriViewModel model)
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
                    string aiResponse = string.Empty;
                    string? imageUrl = null;
                    string? aiImageUrl = null;

                    // Dosya yükleme
                    if (model.Foto != null && model.Foto.Length > 0)
                    {
                        // Dosya boyutu kontrolü (max 5MB)
                        if (model.Foto.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("Foto", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                            return View(model);
                        }

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ai-requests");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Foto.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Foto.CopyToAsync(stream);
                        }

                        imageUrl = $"/uploads/ai-requests/{fileName}";
                    }

                    // Yapay zekadan cevap al
                    switch (model.IstekTipi)
                    {
                        case IstekTipi.EgzersizOnerisi:
                            aiResponse = await _openAIService.GetExerciseRecommendationAsync(
                                model.GirilenBilgi,
                                model.Yas,
                                model.Cinsiyet?.ToString(),
                                model.Boy,
                                model.Kilo,
                                model.Hedef
                            );
                            break;

                        case IstekTipi.DiyetOnerisi:
                            aiResponse = await _openAIService.GetDietRecommendationAsync(
                                model.GirilenBilgi,
                                model.Yas,
                                model.Cinsiyet?.ToString(),
                                model.Boy,
                                model.Kilo,
                                model.Hedef
                            );
                            break;

                        case IstekTipi.GorselSimulasyon:
                            if (!string.IsNullOrEmpty(model.GirilenBilgi))
                            {
                                try
                                {
                                    aiImageUrl = await _openAIService.GenerateVisualSimulationAsync(model.GirilenBilgi);
                                    aiResponse = "Görsel simülasyon başarıyla oluşturuldu. Aşağıda oluşturulan görseli görebilirsiniz.";
                                }
                                catch (Exception ex)
                                {
                                    aiResponse = $"Görsel oluşturulurken hata: {ex.Message}";
                                }
                            }
                            break;

                        case IstekTipi.VucutAnalizi:
                            if (model.Foto != null)
                            {
                                try
                                {
                                    using var memoryStream = new MemoryStream();
                                    await model.Foto.CopyToAsync(memoryStream);
                                    var imageBytes = memoryStream.ToArray();
                                    var base64Image = Convert.ToBase64String(imageBytes);

                                    aiResponse = await _openAIService.GetBodyAnalysisAsync(
                                        base64Image,
                                        model.GirilenBilgi
                                    );
                                }
                                catch (Exception ex)
                                {
                                    aiResponse = $"Vücut analizi sırasında hata: {ex.Message}";
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("Foto", "Vücut analizi için fotoğraf yüklemelisiniz.");
                                return View(model);
                            }
                            break;
                    }

                    // Veritabanına kaydet
                    var oneri = new AIEgzersizOneri
                    {
                        KullaniciId = user.Id,
                        IstekTipi = model.IstekTipi,
                        GirilenBilgi = model.GirilenBilgi,
                        Boy = model.Boy,
                        Kilo = model.Kilo,
                        Yas = model.Yas,
                        Cinsiyet = model.Cinsiyet,
                        Hedef = model.Hedef,
                        FotoUrl = imageUrl,
                        AIYaniti = aiResponse,
                        AIGorselUrl = aiImageUrl,
                        OlusturulmaTarihi = DateTime.Now
                    };

                    _context.AIEgzersizOnerileri.Add(oneri);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Yapay zeka öneriniz başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(Details), new { id = oneri.OneriId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
                    ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: AIEgzersizOneri/Details/5
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

            var oneri = await _context.AIEgzersizOnerileri
                .FirstOrDefaultAsync(o => o.OneriId == id && o.KullaniciId == user.Id);

            if (oneri == null)
            {
                return NotFound();
            }

            // Kullanıcı bilgilerini de ekleyelim
            oneri.Kullanici = user;

            return View(oneri);
        }

        // POST: AIEgzersizOneri/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var oneri = await _context.AIEgzersizOnerileri
                .FirstOrDefaultAsync(o => o.OneriId == id && o.KullaniciId == user.Id);

            if (oneri == null)
            {
                return NotFound();
            }

            _context.AIEgzersizOnerileri.Remove(oneri);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Öneri başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Hızlı Egzersiz Önerisi
        [HttpPost]
        public async Task<IActionResult> QuickExerciseSuggestion([FromBody] QuickSuggestionRequest request)
        {
            try
            {
                var response = await _openAIService.GetExerciseRecommendationAsync(
                    request.Goal,
                    request.Age,
                    request.Gender,
                    request.Height,
                    request.Weight,
                    request.Goal
                );

                return Json(new { success = true, response });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }

    // ViewModel'ler
    public class AIEgzersizOneriViewModel
    {
        [Required(ErrorMessage = "İstek tipi seçmelisiniz")]
        [Display(Name = "İstek Tipi")]
        public IstekTipi IstekTipi { get; set; }

        [Display(Name = "Açıklama / Soru")]
        [MaxLength(2000, ErrorMessage = "Maksimum 2000 karakter")]
        public string GirilenBilgi { get; set; } = string.Empty;

        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalı")]
        public int? Boy { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 200, ErrorMessage = "Kilo 30-200 kg arasında olmalı")]
        public decimal? Kilo { get; set; }

        [Display(Name = "Yaş")]
        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalı")]
        public int? Yas { get; set; }

        [Display(Name = "Cinsiyet")]
        public Cinsiyet? Cinsiyet { get; set; }

        [Display(Name = "Hedef")]
        [MaxLength(500)]
        public string? Hedef { get; set; }

        [Display(Name = "Fotoğraf (Vücut Analizi için)")]
        public IFormFile? Foto { get; set; }

        [Display(Name = "Mevcut Fotoğraf URL")]
        public string? MevcutFotoUrl { get; set; }
    }

    public class QuickSuggestionRequest
    {
        public string Goal { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public int? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}