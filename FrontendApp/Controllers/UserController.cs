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
using FrontendApp.Dto;
using AppDataAccess.Models;

namespace FrontendApp.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private AppDbContext _dbContext { get; }

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            ViewBag.BodyClass = "login-page";
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            ViewBag.IsAuthenticated = false;

            if(TempData["ActionSuccess"] != null)
            {
                ViewBag.ActionSuccess = true;
            } 

            return View();
        }

        public IActionResult Forbidden()
        {
            return View();
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            ViewBag.IsAuthenticated = HttpContext.User.Identity.IsAuthenticated;

            return View(new CustomerDto());
        }

        [HttpPost("register")]
        public IActionResult SubmitRegister(CustomerDto dto)
        {
            var isExist = _dbContext.Customer.Any(a => a.Username == dto.Username);

            if(isExist)
            {
                TempData["ActionErrorMessage"] = "Username นี้มีผู้ใช้งานแล้ว";
                return View("Register", dto);
            } 

            var customer = new Customer();
            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.Gender = dto.Gender;
            customer.Username = dto.Username;
            customer.Password = dto.Password;
            _dbContext.Add(customer);
            _dbContext.SaveChanges();

            TempData["ActionSuccess"] = true;
            return RedirectToAction("Login");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var customer = _dbContext.Customer.FirstOrDefault(c => c.Username == username && c.Password == password);

            if (customer != null)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, Convert.ToString(customer.Id)),
                    new Claim(ClaimTypes.Name, $"{customer.FirstName} {customer.LastName}")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return Redirect("/");
            }

            ViewBag.IsAuthenticated = false;

            return View("Login");
        }
    
        
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
