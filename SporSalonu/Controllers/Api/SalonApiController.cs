using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using YeniSalon.Data;
using YeniSalon.Models;

namespace YeniSalon.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SalonApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SalonApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SalonApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Salon>>> GetSalonlar(
            string? search = null,
            string? sortBy = "SalonAdi",
            string? sortOrder = "asc",
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Salonlar.AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.SalonAdi.Contains(search) ||
                    s.Adres.Contains(search) ||
                    s.Telefon.Contains(search) ||
                    s.Email.Contains(search));
            }

            // Toplam kayıt sayısı
            int totalRecords = await query.CountAsync();

            // Sıralama
            if (!string.IsNullOrEmpty(sortBy))
            {
                string sortDirection = sortOrder?.ToLower() == "desc" ? "descending" : "ascending";
                query = query.OrderBy($"{sortBy} {sortDirection}");
            }

            // Sayfalama
            var salonlar = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.SalonId,
                    s.SalonAdi,
                    s.Adres,
                    s.Telefon,
                    s.Email,
                    s.AcilisSaati,
                    s.KapanisSaati,
                    AntrenorSayisi = s.Antrenorler.Count,
                    HizmetSayisi = s.HizmetTurleri.Count
                })
                .ToListAsync();

            var result = new
            {
                Data = salonlar,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize
            };

            return Ok(result);
        }

        // GET: api/SalonApi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Salon>> GetSalon(int id)
        {
            var salon = await _context.Salonlar
                .Include(s => s.Antrenorler)
                .Include(s => s.HizmetTurleri)
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon == null)
            {
                return NotFound(new { message = "Salon bulunamadı." });
            }

            return Ok(new
            {
                salon.SalonId,
                salon.SalonAdi,
                salon.Adres,
                salon.Telefon,
                salon.Email,
                salon.AcilisSaati,
                salon.KapanisSaati,
                Antrenorler = salon.Antrenorler.Select(a => new
                {
                    a.AntrenorId,
                    a.Ad,
                    a.Soyad,
                    a.UzmanlikAlanlari,
                    a.AktifMi
                }),
                Hizmetler = salon.HizmetTurleri.Select(h => new
                {
                    h.HizmetId,
                    h.HizmetAdi,
                    h.Kategori,
                    h.Ucret,
                    h.SureDakika,
                    h.AktifMi
                })
            });
        }

        // PUT: api/SalonApi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSalon(int id, Salon salon)
        {
            if (id != salon.SalonId)
            {
                return BadRequest(new { message = "ID uyuşmazlığı." });
            }

            var existingSalon = await _context.Salonlar.FindAsync(id);
            if (existingSalon == null)
            {
                return NotFound(new { message = "Salon bulunamadı." });
            }

            // Aynı isimde başka salon var mı kontrol et (kendisi hariç)
            var salonAdiVarMi = await _context.Salonlar
                .AnyAsync(s => s.SalonAdi.ToLower() == salon.SalonAdi.ToLower() && s.SalonId != id);

            if (salonAdiVarMi)
            {
                return BadRequest(new { message = "Bu isimde başka bir salon zaten mevcut." });
            }

            // Güncelleme
            existingSalon.SalonAdi = salon.SalonAdi;
            existingSalon.Adres = salon.Adres;
            existingSalon.Telefon = salon.Telefon;
            existingSalon.Email = salon.Email;
            existingSalon.AcilisSaati = salon.AcilisSaati;
            existingSalon.KapanisSaati = salon.KapanisSaati;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Salon başarıyla güncellendi.", salon = existingSalon });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalonExists(id))
                {
                    return NotFound(new { message = "Salon bulunamadı." });
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/SalonApi
        [HttpPost]
        public async Task<ActionResult<Salon>> PostSalon(Salon salon)
        {
            // Aynı isimde salon var mı kontrol et
            var salonAdiVarMi = await _context.Salonlar
                .AnyAsync(s => s.SalonAdi.ToLower() == salon.SalonAdi.ToLower());

            if (salonAdiVarMi)
            {
                return BadRequest(new { message = "Bu isimde bir salon zaten mevcut." });
            }

            _context.Salonlar.Add(salon);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSalon", new { id = salon.SalonId },
                new { message = "Salon başarıyla oluşturuldu.", salon });
        }

        // DELETE: api/SalonApi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalon(int id)
        {
            var salon = await _context.Salonlar
                .Include(s => s.Antrenorler)
                .Include(s => s.HizmetTurleri)
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon == null)
            {
                return NotFound(new { message = "Salon bulunamadı." });
            }

            // Salonun antrenörleri var mı kontrol et
            if (salon.Antrenorler != null && salon.Antrenorler.Any(a => a.AktifMi))
            {
                return BadRequest(new
                {
                    message = "Bu salona kayıtlı aktif antrenörler bulunmaktadır. Önce antrenörleri başka bir salona taşıyın veya pasif hale getirin."
                });
            }

            // Salonun hizmetleri var mı kontrol et
            if (salon.HizmetTurleri != null && salon.HizmetTurleri.Any(h => h.AktifMi))
            {
                return BadRequest(new
                {
                    message = "Bu salona kayıtlı aktif hizmetler bulunmaktadır. Önce hizmetleri başka bir salona taşıyın veya pasif hale getirin."
                });
            }

            _context.Salonlar.Remove(salon);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Salon başarıyla silindi." });
        }

        // GET: api/SalonApi/GetSalonStatistics/{id}
        [HttpGet("GetSalonStatistics/{id}")]
        public async Task<ActionResult> GetSalonStatistics(int id)
        {
            var salon = await _context.Salonlar
                .Include(s => s.Antrenorler)
                .Include(s => s.HizmetTurleri)
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon == null)
            {
                return NotFound(new { message = "Salon bulunamadı." });
            }

            // İstatistikleri hesapla
            var aktifAntrenorSayisi = salon.Antrenorler?.Count(a => a.AktifMi) ?? 0;
            var toplamAntrenorSayisi = salon.Antrenorler?.Count ?? 0;
            var aktifHizmetSayisi = salon.HizmetTurleri?.Count(h => h.AktifMi) ?? 0;
            var toplamHizmetSayisi = salon.HizmetTurleri?.Count ?? 0;

            // Bu salondaki toplam randevu sayısı
            var toplamRandevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .CountAsync(r => r.Antrenor != null && r.Antrenor.SalonId == id);

            // Son 30 günkü randevu sayısı
            var son30Gun = DateTime.Now.AddDays(-30);
            var son30GunRandevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .CountAsync(r => r.Antrenor != null && r.Antrenor.SalonId == id && r.RandevuTarihi >= son30Gun);

            var istatistikler = new
            {
                SalonAdi = salon.SalonAdi,
                AktifAntrenorSayisi = aktifAntrenorSayisi,
                ToplamAntrenorSayisi = toplamAntrenorSayisi,
                AktifHizmetSayisi = aktifHizmetSayisi,
                ToplamHizmetSayisi = toplamHizmetSayisi,
                ToplamRandevu = toplamRandevu,
                Son30GunRandevu = son30GunRandevu,
                DolulukOrani = toplamAntrenorSayisi > 0 ?
                    (aktifAntrenorSayisi / (double)toplamAntrenorSayisi) * 100 : 0
            };

            return Ok(istatistikler);
        }

        // GET: api/SalonApi/GetAllSalonsForDropdown
        [HttpGet("GetAllSalonsForDropdown")]
        public async Task<ActionResult> GetAllSalonsForDropdown()
        {
            var salonlar = await _context.Salonlar
                .OrderBy(s => s.SalonAdi)
                .Select(s => new
                {
                    Value = s.SalonId,
                    Text = s.SalonAdi,
                    s.Adres
                })
                .ToListAsync();

            return Ok(salonlar);
        }

        // GET: api/SalonApi/CheckAvailability/{id}?date=2024-01-01&time=09:00
        [HttpGet("CheckAvailability/{id}")]
        public async Task<ActionResult> CheckAvailability(int id, DateTime date, TimeSpan time)
        {
            var salon = await _context.Salonlar
                .Include(s => s.Antrenorler)
                .ThenInclude(a => a.Randevular)
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon == null)
            {
                return NotFound(new { message = "Salon bulunamadı." });
            }

            // Salonun çalışma saatlerini kontrol et
            if (time < salon.AcilisSaati || time >= salon.KapanisSaati)
            {
                return Ok(new
                {
                    Available = false,
                    Message = "Salon bu saatte kapalıdır.",
                    OpeningHours = $"{salon.AcilisSaati:hh\\:mm} - {salon.KapanisSaati:hh\\:mm}"
                });
            }

            // Bu saatte salonun antrenörlerinin randevu durumunu kontrol et
            var randevuTarihi = date.Date + time;
            var musaitAntrenorler = salon.Antrenorler?
                .Where(a => a.AktifMi)
                .Select(a => new
                {
                    AntrenorId = a.AntrenorId,
                    AdSoyad = $"{a.Ad} {a.Soyad}",
                    UzmanlikAlanlari = a.UzmanlikAlanlari,
                    Available = !a.Randevular.Any(r =>
                        r.Durum != RandevuDurumu.IptalEdildi &&
                        r.Durum != RandevuDurumu.Reddedildi &&
                        r.RandevuTarihi <= randevuTarihi &&
                        r.BitisTarihi > randevuTarihi)
                })
                .ToList();

            var musaitAntrenorSayisi = musaitAntrenorler?.Count(a => a.Available) ?? 0;

            return Ok(new
            {
                Available = musaitAntrenorSayisi > 0,
                MusaitAntrenorSayisi = musaitAntrenorSayisi,
                ToplamAntrenorSayisi = salon.Antrenorler?.Count(a => a.AktifMi) ?? 0,
                MusaitAntrenorler = musaitAntrenorler?.Where(a => a.Available),
                SalonCalismaSaatleri = new
                {
                    Acilis = salon.AcilisSaati,
                    Kapanis = salon.KapanisSaati
                }
            });
        }

        private bool SalonExists(int id)
        {
            return _context.Salonlar.Any(e => e.SalonId == id);
        }
    }
}