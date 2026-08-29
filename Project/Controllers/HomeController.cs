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
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hosting;

        public HomeController(ILogger<HomeController> logger, IProductService productService, IOrderService orderService, ImageStore imageStore, Microsoft.AspNetCore.Hosting.IHostingEnvironment hosting)
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
                .Where(p => p.Id != id && string.Equals(p.Cat, product.Cat, StringComparison.OrdinalIgnoreCase))
                .Take(4)
                .ToList();

            if (related.Count < 4)
            {
                var others = allProducts
                    .Where(p => p.Id != id && !related.Any(r => r.Id == p.Id))
                    .Take(4 - related.Count)
                    .ToList();
                related.AddRange(others);
            }

            ViewBag.RelatedProducts = related;
            return View(product);
        }

        [HttpPost]
        [Route("Home/CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            try
            {
                if (request == null || request.OrderLines == null || !request.OrderLines.Any())
                {
                    return BadRequest(new { message = "الطلب فارغ أو بيانات غير صحيحة" });
                }

                var order = new Order
                {
                    Customer = new Customer
                    {
                        Name = request.Customer?.Name ?? "عميل",
                        Email = request.Customer?.Email ?? "",
                        PhoneNumber = request.Customer?.PhoneNumber ?? "",
                        Address = request.Customer?.Address ?? ""
                    },
                    OrderLines = request.OrderLines.Select(ol => new OrderLine
                    {
                        ProductId = ol.ProductId,
                        Quantity = ol.Quantity,
                        Price = ol.Price,
                        Size = ParseSize(ol.Size)
                    }).ToList()
                };

                var createdOrder = await orderService.CreateOrderAsync(order);

                return Ok(new { id = createdOrder.Id, message = "تم إنشاء الطلب بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "خطأ في إنشاء الطلب" });
            }
        }

        private Sizes ParseSize(string sizeString)
        {
            if (string.IsNullOrEmpty(sizeString))
                return Sizes.M; // Default size

            // Try to parse the size string
            if (Enum.TryParse<Sizes>(sizeString, ignoreCase: true, out var size))
            {
                return size;
            }

            // Handle custom Arabic sizes or patterns
            return sizeString.ToLower() switch
            {
                "مقاس واحد" => Sizes.M,
                "s" => Sizes.S,
                "m" => Sizes.M,
                "l" => Sizes.L,
                "xl" => Sizes.XL,
                "xxl" => Sizes.XXl,
                "xxxl" => Sizes.XXXL,
                _ => Sizes.M
            };
        }
    }

    // DTO Classes for API requests
    public class OrderCreateRequest
    {
        public CustomerCreateRequest Customer { get; set; }
        public List<OrderLineCreateRequest> OrderLines { get; set; }
    }

    public class CustomerCreateRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public class OrderLineCreateRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Size { get; set; }
    }
}
