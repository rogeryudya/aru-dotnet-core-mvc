using System;
using System.Collections.Generic;
using System.Linq;
using AppDataAccess;
using AppDataAccess.Attribute;
using AppDataAccess.Constants;
using AppDataAccess.Models;
using BackendApp.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BackendApp.Controllers
{
    [Route("customer")]
    [RoleAuthorizeAttribute(RoleConstant.Admin, RoleConstant.Employee)]
    public class CustomerController : Controller
    {
        private readonly ILogger<CustomerController> _logger;

        private AppDbContext _dbContext;

        public CustomerController(ILogger<CustomerController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            ViewBag.BodyClass = "sidebar-mini";
        }

        public IActionResult Index(string search, int currentPage = 1)
        {
            var contentResult = new ContentResult<List<Customer>>();

            var query = _dbContext.Customer.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(w => w.FirstName.Contains(search) || w.LastName.Contains(search));
            }

            var maxRow = 1;
            var customers = query
                    .OrderBy(cus => cus.Id)
                    .Skip((currentPage - 1) * maxRow)
                    .Take(maxRow)
                    .ToList();

            double pageCount = (double)((decimal)query.Count() / Convert.ToDecimal(maxRow));
            contentResult.Result = customers;
            contentResult.CurrentPage = currentPage;
            contentResult.TotalPage = (int)Math.Ceiling(pageCount);
            contentResult.TotalRecord = query.Count();
            ViewBag.SearchQuery = search;

            return View(contentResult);
        }
    
        [HttpGet("view/{id}")]
        public IActionResult View(int id)
        {
            ViewBag.FormTitle = "ข้อมูลลูกค้า";
            var customer = _dbContext.Customer.Where(w => w.Id == id)
                .Select(s => new CustomerFormDto
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Gender = s.Gender
                }).First();

            return View("Form", customer);
        }

    }
}