using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TradingApp.BusinessLayer.Services;
using TradingApp.UILayer.Models;

namespace TradingApp.UILayer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITradingCache _tradingCache;

        public HomeController(ITradingCache tradingCache)
        {
            _tradingCache = tradingCache;
        }

        public IActionResult Index()
        {
            return View(_tradingCache.GetPairsForView());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
