using BierDex.Data;
using BierDex.Models;
using BierDex.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BierDex.Controllers
{
    public class HomeController : Controller
    {
        private readonly BierdexDBContext _context;

        public HomeController(BierdexDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var beers = await _context.Beers
                .Select(b => new HomeViewModel
                {
                    Barcode = b.barcode,
                    Name = b.name,
                    Type = b.type,
                    ImagePath = b.imagePath,
                    Abv = b.abv
                })
                .ToListAsync();

            return View(beers);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
        [HttpGet]
        public async Task<IActionResult> Search(int barcode)
        {
            var beer = await _context.Beers
                .Where(b => b.barcode == barcode)
                .Select(b => new HomeViewModel
                {
                    Barcode = b.barcode,
                    Name = b.name,
                    Type = b.type,
                    ImagePath = b.imagePath
                })
                .FirstOrDefaultAsync();

            if (beer == null)
                return NotFound();

            return Json(beer);
        }
    }
}