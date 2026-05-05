using Microsoft.AspNetCore.Mvc;

namespace web_smart_recruitment.Controllers
{
    public class HrController : Controller
    {
        public IActionResult Dashboard() => View();
        public IActionResult Jobs() => View();
        public IActionResult Applications() => View();
        public IActionResult Interviews() => View();
        public IActionResult JobForm() => View();
        public IActionResult JobStatus() => View();
        public IActionResult Company() => View();
        public IActionResult Profile() => View();
        public IActionResult AiCandidate() => View();
    }
}
