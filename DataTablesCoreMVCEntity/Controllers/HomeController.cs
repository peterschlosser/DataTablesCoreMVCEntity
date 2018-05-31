using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DataTablesCoreMVCEntity.Models;
using DataTablesCoreMVCEntity.Data;

namespace DataTablesCoreMVCEntity.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(CustomerContext context)
        {
            _context = context;
        }

        private readonly CustomerContext _context;

        public async Task<JsonResult> AjaxData([FromBody] DataTablesRequest request)
        {
            var response = await DataTablesLogicEntity.DataTablesRequestAsync(request, _context);
            return new JsonResult(response);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Customers()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
