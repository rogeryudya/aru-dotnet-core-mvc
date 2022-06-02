using System;
using System.Collections.Generic;
using System.Linq;
using AppDataAccess;
using AppDataAccess.Models;
using FrontendApp.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FrontendApp.Controllers
{
    [Route("")]
    [Route("product")]
    public class ProductController : Controller
    {
        private AppDbContext _dbContext;

        public ProductController(AppDbContext context)
        {
            _dbContext = context;
        }

        public IActionResult Index(string search, int currentPage = 1)
        {
            ViewBag.PageHeader = "รายการสินค้า";
            ViewBag.IsAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            var contentResult = new ContentResult<List<Product>>();

            var query = _dbContext.Product.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(w => w.Name.Contains(search));
            }

            var maxRow = 10;
            var products = query
                    .OrderBy(p => p.Id)
                    .Skip((currentPage - 1) * maxRow)
                    .Take(maxRow)
                    .ToList();

            double pageCount = (double)((decimal)query.Count() / Convert.ToDecimal(maxRow));
            contentResult.Result = products;
            contentResult.CurrentPage = currentPage;
            contentResult.TotalPage = (int)Math.Ceiling(pageCount);
            contentResult.TotalRecord = query.Count();
            ViewBag.SearchQuery = search;

            return View(contentResult);
        }


        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            ViewBag.IsAuthenticated = HttpContext.User.Identity.IsAuthenticated;

            var product = _dbContext.Product.Where(w => w.Id == id)
                .Select(s => new ProductDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    Details = s.Details,
                    ImageByteArray = s.Image,
                    Price = s.Price,
                    ImageType = s.ImageType
                }).First();
            
            return View("Details", product);
        }
    }
}