using Microsoft.AspNetCore.Mvc;
using CinemaManagement.Models;
using System.Linq;

namespace CinemaManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Movies");
        }
    }
}
