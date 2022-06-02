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
    [Route("employee")]
    [RoleAuthorizeAttribute(RoleConstant.Admin)]
    public class EmployeeController : Controller
    {
        private readonly ILogger<EmployeeController> _logger;

        private AppDbContext _dbContext;

        public EmployeeController(ILogger<EmployeeController> logger, AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            ViewBag.BodyClass = "sidebar-mini";
        }

        public IActionResult Index(string search, int currentPage = 1)
        {
            var contentResult = new ContentResult<List<Employee>>();

            var query = _dbContext.Employee.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(w => w.FirstName.Contains(search) || w.LastName.Contains(search));
            }

            var maxRow = 1;
            var employees = query
                    .OrderBy(emp => emp.Id)
                    .Skip((currentPage - 1) * maxRow)
                    .Take(maxRow)
                    .ToList();

            double pageCount = (double)((decimal)query.Count() / Convert.ToDecimal(maxRow));
            contentResult.Result = employees;
            contentResult.CurrentPage = currentPage;
            contentResult.TotalPage = (int)Math.Ceiling(pageCount);
            contentResult.TotalRecord = query.Count();
            ViewBag.SearchQuery = search;

            return View(contentResult);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.FormTitle = "สร้างข้อมูลพนักงาน";
            var employee = new EmployeeFormDto();

            return View("Form", employee);
        }

        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            ViewBag.FormTitle = "แก้ข้อมูลพนักงาน";
            var employee = _dbContext.Employee.Where(w => w.Id == id)
                .Select(s => new EmployeeFormDto
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Role = s.Role,
                    Username = s.Username
                }).First();

            return View("Form", employee);
        }

        [HttpPost("save")]
        public IActionResult Save(EmployeeFormDto employeeFormDto)
        {
            if (employeeFormDto.Id != 0)
            {
                var employee = _dbContext.Employee.Find(employeeFormDto.Id);
                employee.FirstName = employeeFormDto.FirstName;
                employee.LastName = employeeFormDto.LastName;
                employee.Role = employeeFormDto.Role;
                _dbContext.Employee.Update(employee);
            }
            else
            {
                var employee = new Employee();
                employee.FirstName = employeeFormDto.FirstName;
                employee.LastName = employeeFormDto.LastName;
                employee.Role = employeeFormDto.Role;
                employee.Username = employeeFormDto.Username;
                employee.Password = employeeFormDto.Password;
                _dbContext.Employee.Add(employee);
            }

            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    
        [HttpGet("delete/{id}")]
        public IActionResult delete(int id)
        {
            var employee = _dbContext.Employee.Find(id);
            _dbContext.Employee.Remove(employee);
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}