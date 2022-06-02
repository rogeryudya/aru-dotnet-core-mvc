using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppDataAccess;
using AppDataAccess.Attribute;
using AppDataAccess.Constants;
using AppDataAccess.Models;
using BackendApp.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendApp.Controllers
{
    [Route("product")]
    [RoleAuthorizeAttribute(RoleConstant.Admin)]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;

        private AppDbContext _dbContext;

        public ProductController(
            ILogger<ProductController> logger,
            AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IActionResult Index(string search, int currentPage = 1)
        {
            var contentResult = new ContentResult<List<Product>>();
            ViewBag.BodyClass = "sidebar-mini";

            var query = _dbContext.Product.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(w => w.Name.Contains(search) || w.Id.ToString().Contains(search));
            }

            var maxRow = 1;
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

        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.BodyClass = "sidebar-mini";
            ViewBag.FormTitle = "สร้างข้อมูลสินค้า";
            var product = new ProductFormDto();

            return View("Form", product);
        }

        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            ViewBag.BodyClass = "sidebar-mini";
            ViewBag.FormTitle = "แก้ข้อมูลสินค้า";
            var product = _dbContext.Product.Where(w => w.Id == id)
                .Select(s => new ProductFormDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    Details = s.Details,
                    ImageByteArray = s.Image,
                    Price = s.Price,
                    ImageType = s.ImageType
                }).First();

            return View("Form", product);
        }

        [HttpPost("save")]
        public IActionResult Save(ProductFormDto productFormDto)
        {
            byte[] file = null;
            string imageType = null;

            if (productFormDto.ImageFile != null)
            {
                using var stream = productFormDto.ImageFile.OpenReadStream();
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                file = memoryStream.ToArray();
                imageType = Path.GetExtension(productFormDto.ImageFile.FileName).Replace(".", "");
            }

            if (productFormDto.Id != 0)
            {
                var product = _dbContext.Product.Find(productFormDto.Id);
                product.Name = productFormDto.Name;
                product.Details = productFormDto.Details;
                product.Price = productFormDto.Price;
                product.Category = productFormDto.Category;

                if (file != null)
                {
                    product.Image = file;
                    product.ImageType = imageType;
                }

                _dbContext.Product.Update(product);
            }
            else
            {
                var product = new Product();
                product.Name = productFormDto.Name;
                product.Details = productFormDto.Details;
                product.Price = productFormDto.Price;
                product.Category = productFormDto.Category;

                if (file != null)
                {
                    product.Image = file;
                    product.ImageType = imageType;
                }

                _dbContext.Product.Add(product);
            }

            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}