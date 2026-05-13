using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace web_smart_recruitment.Controllers
{
    [Authorize(Roles = "UngVien")]
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
