using System;
using System.Collections.Generic;
using System.Linq;
using AppDataAccess;
using AppDataAccess.Models;
using BackendApp.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApp.Controllers
{
    [Route("report")]
    public class ReportController : Controller
    {
        private AppDbContext _dbContext;

        public ReportController(AppDbContext context)
        {
            _dbContext = context;
        }

        [HttpGet("monthly")]
        public IActionResult Monthly(int year, int month = 1)
        {
            var query = (from order in _dbContext.Order
                         select new
                         {
                             order.OrderDate,
                             order.TotalPrice
                         });


            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            query = query.Where(w => w.OrderDate.Year == year && w.OrderDate.Month == month);
            ViewBag.Year = year;
            ViewBag.Month = month;

            var orders = query.GroupBy(g => new { g.OrderDate.Year, g.OrderDate.Month })
                        .OrderBy(o => o.Key.Month)
                        .Select(s => new ReportResultDto { Label = s.Key.Month, Value = s.Sum(d => d.TotalPrice) })
                        .ToList();

            var reports = new List<ReportResultDto>();
            var reportDate = new DateTime(year, month+1, 1).AddDays(-1);

            for (int i = 1; i <= reportDate.Day; i++)
            {
                reports.Add(new ReportResultDto
                {
                    Label = i,
                    Value = orders.FirstOrDefault(f => f.Label == i)?.Value ?? 0
                });
            }

            return View("Monthly", reports);
        }

        [HttpGet("yearly")]
        public IActionResult Yearly(int search = 0)
        {
            var query = (from order in _dbContext.Order
                         select new
                         {
                             order.OrderDate,
                             order.TotalPrice
                         });


            if (search == 0)
            {
                search = DateTime.Now.Year;
            }

            query = query.Where(w => w.OrderDate.Year == search);
            ViewBag.SearchQuery = search;

            var orders = query.GroupBy(g => new { g.OrderDate.Year, g.OrderDate.Month })
                        .OrderBy(o => o.Key.Month)
                        .Select(s => new ReportResultDto { Label = s.Key.Month, Value = s.Sum(d => d.TotalPrice) })
                        .ToList();

            var reports = new List<ReportResultDto>();

            for (int i = 1; i <= 12; i++)
            {
                reports.Add(new ReportResultDto
                {
                    Label = i,
                    Value = orders.FirstOrDefault(f => f.Label == i)?.Value ?? 0
                });
            }

            return View("Yearly", reports);
        }
    }
}