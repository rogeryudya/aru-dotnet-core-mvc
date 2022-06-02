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
    [Route("order")]
    public class OrderController : Controller
    {
        private AppDbContext _dbContext;

        public OrderController(AppDbContext context)
        {
            _dbContext = context;
        }

        public IActionResult Index(string search, int currentPage = 1)
        {
            var contentResult = new ContentResult<List<Order>>();

            var query = _dbContext.Order.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(w => w.Id.ToString().Contains(search));
            }

            var maxRow = 1;
            var orders = query
                    .OrderBy(o => o.Id)
                    .Skip((currentPage - 1) * maxRow)
                    .Take(maxRow)
                    .ToList();

            double pageCount = (double)((decimal)query.Count() / Convert.ToDecimal(maxRow));
            contentResult.Result = orders;
            contentResult.CurrentPage = currentPage;
            contentResult.TotalPage = (int)Math.Ceiling(pageCount);
            contentResult.TotalRecord = query.Count();
            ViewBag.SearchQuery = search;

            return View(contentResult);
        }

        [HttpGet("view/{id}")]
        public IActionResult View(int id)
        {
            ViewBag.FormTitle = "ข้อมูลคำสั่งซื้อ";
            var order = _dbContext.Order
            .Include(i => i.Customer)
            .Include(i => i.Carts)
            .ThenInclude(i => i.Product)
            .Where(w => w.Id == id).First();

            return View("Details", order);
        }

        [HttpPost("update-status")]
        public IActionResult UpdateStatus(int orderId, int status)
        {
            var order = _dbContext.Order.First(f => f.Id == orderId);

            order.Status = status;
            _dbContext.SaveChanges();

            return RedirectToAction("View", orderId);
        }
    }
}