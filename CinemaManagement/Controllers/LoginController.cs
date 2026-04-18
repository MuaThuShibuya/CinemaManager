using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers
{
    public class LoginController
    {
        public IActionResult Login()
        {
            return View();
        }

        private IActionResult View()
        {
            throw new NotImplementedException();
        }
    }
}
