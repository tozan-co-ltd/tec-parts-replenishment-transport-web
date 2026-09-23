using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace tec_parts_supply_transport_web.Controllers
{
    public class TopController : Controller
    {
        private readonly ILogger<TopController> _logger;

        public TopController(ILogger<TopController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// トップ画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}