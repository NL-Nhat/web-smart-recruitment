using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace web_smart_recruitment.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult Dashboard() => View();
        public IActionResult Users() => View();
        public IActionResult Skills() => View();
        public IActionResult Reports() => View();
        public IActionResult Profile() => View();
        public IActionResult Roles() => View();
    }
}
