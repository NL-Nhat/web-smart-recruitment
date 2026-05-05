using Microsoft.AspNetCore.Mvc;

namespace web_smart_recruitment.Controllers
{
    public class CandidateController : Controller
    {
        public IActionResult Jobs() => View();
        public IActionResult JobDetail() => View();
        public IActionResult Applications() => View();
        public IActionResult Interviews() => View();
        public IActionResult Profile() => View();
        public IActionResult CompanyDetail() => View();
    }
}
