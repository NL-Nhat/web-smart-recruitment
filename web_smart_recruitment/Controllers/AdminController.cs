using Microsoft.AspNetCore.Mvc;

namespace web_smart_recruitment.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult Dashboard() => View();
        public IActionResult Users() => View();
        public IActionResult Skills() => View();
        public IActionResult Reports() => View();
        public IActionResult Profile() => View();
    }
}
