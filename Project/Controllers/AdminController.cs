using Infrastructure.InterFace;
using Infrastructure.InterFace.Services;
using Infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Models;
using Core.enums;

namespace Project.Controllers
{
    [Authorize]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly IProductService productService;
        private readonly ICategoryService categoryService;
        private readonly IOrderService orderService;

        public AdminController(
            IProductService productService,
            ICategoryService categoryService,
            IOrderService orderService)
        {
            this.productService = productService;
            this.categoryService = categoryService;
            this.orderService = orderService;
        }

        // ==================== DASHBOARD ====================
        [HttpGet]
        [Route("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var products = await productService.GetAllProducts();
                var categories = await categoryService.GetCategory();
                var orders = await orderService.GetAllOrdersAsync();
                
                var lowStockProducts = products?.Where(p => p.Quantity < 10).Count() ?? 0;

                var dashboardData = new Dictionary<string, object>
                {
                    { "TotalProducts", products?.Count() ?? 0 },
                    { "TotalCategories", categories?.Count() ?? 0 },
                    { "TotalOrders", orders?.Count() ?? 0 },
                    { "PendingOrders", orders?.Where(o => o.Status == OrderStatus.Pending).Count() ?? 0 },
                    { "ProcessedOrders", orders?.Where(o => o.Status == OrderStatus.Completed).Count() ?? 0 },
                    { "TotalRevenue", orders?.Sum(o => o.TotalPrice) ?? 0 },
                    { "MissingItems", orders?.Where(o => o.Status == OrderStatus.MissingItems).Count() ?? 0 },
                    { "LowStockProducts", lowStockProducts }
                };

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "حدث خطأ: " + ex.Message;
                return View(new Dictionary<string, object>());
            }
        }

        // ==================== ANALYTICS & MONITORING ====================
        [HttpGet]
        [Route("Analytics")]
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var products = await productService.GetAllProducts();
                var orders = await orderService.GetAllOrdersAsync();

                var missingOrders = orders?.Where(o => o.Status == OrderStatus.MissingItems).ToList() ?? new List<Order>();
                var topProducts = products?.OrderByDescending(p => p.OldPrice).Take(5).ToList() ?? new List<ProductViewModel>();
                var ordersByStatus = orders?.GroupBy(o => o.Status).Select(g => (object)new { Status = g.Key.ToString(), Count = g.Count() }).ToList() ?? new List<object>();
                var recentOrders = orders?.OrderByDescending(o => o.CreatedDate).Take(10).ToList() ?? new List<Order>();

                ViewBag.MissingOrders = missingOrders;
                ViewBag.TopProducts = topProducts;
                ViewBag.OrdersByStatus = ordersByStatus;
                ViewBag.RecentOrders = recentOrders;

                var analytics = new Dictionary<string, object>
                {
                    { "MissingOrders", missingOrders },
                    { "TopProducts", topProducts },
                    { "OrdersByStatus", ordersByStatus },
                    { "RecentOrders", recentOrders }
                };

                return View(analytics);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "خطأ: " + ex.Message;
                return View(new Dictionary<string, object>());
            }
        }
    }
}
