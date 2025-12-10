
using Microsoft.AspNetCore.Mvc;

namespace SporSalonu.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
