using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using AppDataAccess;
using AppDataAccess.Models;
using FrontendApp.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrontendApp.Controllers
{
    [Route("order")]
    public class OrderController : Controller
    {
        private AppDbContext _dbContext;

        public OrderController(AppDbContext context)
        {
            _dbContext = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index(string search, int currentPage = 1)
        {
            ViewBag.IsAuthenticated = true;
            var contentResult = new ContentResult<List<Order>>();
            var userIdx = HttpContext.User.Claims;
            var userId = HttpContext.User.Claims.First(f => f.Type == ClaimTypes.NameIdentifier);
            var query = _dbContext.Order.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(w => w.Id.ToString().Contains(search));
            }

            var maxRow = 10;
            var orders = query
                    .Where(w => w.CustomerId.ToString() == userId.Value)
                    .OrderBy(p => p.Id)
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

        [Authorize]
        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            ViewBag.IsAuthenticated = true;
            var userId = HttpContext.User.Claims.First(f => f.Type == ClaimTypes.NameIdentifier);
            var order = _dbContext.Order
            .Include(i => i.Customer)
            .Include(i => i.Carts)
            .ThenInclude(i => i.Product)
            .FirstOrDefault(f => f.Id == id && f.CustomerId.ToString() == userId.Value);

            return View(order);
        }

        [Authorize]
        [HttpPost("cart")]
        public IActionResult AddCart(OrderCartDto dto)
        {
            var carts = new List<OrderCartDto>();

            if (HttpContext.Session.Keys.Any(k => k == "carts"))
            {
                carts = JsonSerializer.Deserialize<List<OrderCartDto>>(HttpContext.Session.GetString("carts"));
            }

            var cartProduct = carts.FirstOrDefault(f => f.ProductId == dto.ProductId);
            if (cartProduct != null)
            {
                cartProduct.Quantity += dto.Quantity;
            }
            else
            {
                carts.Add(new OrderCartDto
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            HttpContext.Session.SetString("carts", JsonSerializer.Serialize(carts));

            return Redirect($"/product/details/{dto.ProductId}");
        }

        [Authorize]
        [HttpPost("change")]
         public IActionResult ChangeCart(List<OrderCartDto> dtoCarts)
        {
            var carts = new List<OrderCartDto>();

            if (dtoCarts.Count > 0)
            {
                carts = dtoCarts;
            }

            HttpContext.Session.SetString("carts", JsonSerializer.Serialize(carts));

            return Redirect($"/order/cart");
        }

        [Authorize]
        [HttpGet("cart")]
        public IActionResult ListCart()
        {
            ViewBag.IsAuthenticated = HttpContext.User.Identity.IsAuthenticated;

            var carts = new List<OrderCartDto>();

            if (HttpContext.Session.Keys.Any(k => k == "carts"))
            {
                carts = JsonSerializer.Deserialize<List<OrderCartDto>>(HttpContext.Session.GetString("carts"));
            }
            var productIds = carts.Select(s => s.ProductId).ToArray();
            var products = _dbContext.Product.AsNoTracking().Where(w => productIds.Contains(w.Id)).ToList();

            var joinedCarts = (from cart in carts   
                              join product in products on cart.ProductId equals product.Id
                              select new OrderCartProductDto{
                                ProductId = cart.ProductId,
                                Quantity = cart.Quantity,
                                Price = product.Price,
                                Image = product.Image,
                                ImageType = product.ImageType,
                                Name = product.Name
                              }).ToList();

            return View("Cart", joinedCarts);
        }

        [Authorize]
        [HttpGet("submit")]
        public IActionResult SubmitOrder()
        {
            var carts = JsonSerializer.Deserialize<List<OrderCartDto>>(HttpContext.Session.GetString("carts"));
            var productIds = carts.Select(s => s.ProductId).ToArray();
            var products = _dbContext.Product.AsNoTracking().Where(w => productIds.Contains(w.Id)).ToList();

            var joinedCarts = (from cart in carts   
                              join product in products on cart.ProductId equals product.Id
                              select new OrderCart{
                                ProductId = cart.ProductId,
                                Quantity = cart.Quantity,
                                Price = product.Price
                              }).ToList();

            var order = new Order();
            order.CustomerId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            order.TotalPrice = joinedCarts.Sum(s => s.Quantity * s.Price);
            order.OrderDate = DateTime.Now;
            order.Carts = joinedCarts;

            _dbContext.Add(order);
            _dbContext.SaveChanges();

            HttpContext.Session.SetString("carts", JsonSerializer.Serialize(new List<OrderCartDto>()));

            return RedirectToAction("Index");
        }
    }
}