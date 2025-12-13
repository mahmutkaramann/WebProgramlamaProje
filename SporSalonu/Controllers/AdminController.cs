// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YeniSalon.Data;
using YeniSalon.Models;

namespace YeniSalon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = new DashboardViewModel
            {
                ToplamUye = await _context.Users.CountAsync(),
                ToplamAntrenor = await _context.Antrenorler.CountAsync(),
                ToplamRandevu = await _context.Randevular.CountAsync(),
                AktifRandevular = await _context.Randevular
                    .Where(r => r.Durum == RandevuDurumu.Beklemede || r.Durum == RandevuDurumu.Onaylandi)
                    .CountAsync()
            };

            return View(dashboard);
        }
    }

    public class DashboardViewModel
    {
        public int ToplamUye { get; set; }
        public int ToplamAntrenor { get; set; }
        public int ToplamRandevu { get; set; }
        public int AktifRandevular { get; set; }
    }
}
