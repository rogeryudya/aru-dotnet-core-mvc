using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using AppDataAccess;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace BackendApp.Controllers
{
    public class GuestController : Controller
    {
        private readonly ILogger<GuestController> _logger;

        private AppDbContext _dbContext { get; }

        public GuestController(ILogger<GuestController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.BodyClass = "login-page";

            return View();
        }

        public IActionResult Forbidden()
        {
            return View();
        }

        [Route("/")]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var employee = _dbContext.Employee.FirstOrDefault(c => c.Username == username && c.Password == password);

            if (employee != null)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, Convert.ToString(employee.Id)),
                    new Claim(ClaimTypes.Name, employee.Username),
                    new Claim(ClaimTypes.Role, employee.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return Redirect("employee");
            }

            return View("Index", new { username, password });
        }
    }
}
