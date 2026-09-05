using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.ViewModel;
using Infrastructure.InterFace.Services;
using System.Threading.Tasks;
using Core.Services;
using Core.Models;
using Core.enums;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService productService;
        private readonly IOrderService orderService;
        private readonly ImageStore imageStore;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment hosting;

        public HomeController(ILogger<HomeController> logger, IProductService productService, IOrderService orderService, ImageStore imageStore, Microsoft.AspNetCore.Hosting.IWebHostEnvironment hosting)
        {
            _logger = logger;
            this.productService = productService;
            this.orderService = orderService;
            this.imageStore = imageStore;
            this.hosting = hosting;
        }

        public async Task<IActionResult> Index()
        {
            var products = await productService.GetAllProducts();
            return View(products);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var product = await productService.GetProductDetails(id);
            if (product == null)
                return NotFound();

            var allProducts = (await productService.GetAllProducts()).OfType<GetProductViewModel>().ToList();

            var related = allProducts
                .Where(p => p.Id != id && !string.IsNullOrEmpty(product.Cat) && string.Equals(p.Cat?.Trim(), product.Cat?.Trim(), StringComparison.OrdinalIgnoreCase))
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = related;
            return View(product);
        }

        
    }

   
}
