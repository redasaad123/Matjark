using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.ViewModel;
using Infrastructure.InterFace.Services;
using System.Threading.Tasks;
using Core.Services;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService productService;
        private readonly ImageStore imageStore;
       private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hosting;

        public HomeController(ILogger<HomeController> logger , IProductService productService , ImageStore imageStore , Microsoft.AspNetCore.Hosting.IHostingEnvironment hosting)
        {
            _logger = logger;
            this.productService = productService;
            this.imageStore = imageStore;
            this.hosting = hosting;
        }

        public async Task<IActionResult> Index()
        {


            //var products = new List<>
            //{
            //    new() { Id = 1, Name = "تيشيرت أبيض أوفرسايز", Cat = "men", Price = 450, OldPrice = null, Icon = "ic-tshirt", Sizes = new() { "S", "M", "L", "XL" } },
            //    new() { Id = 2, Name = "جاكيت دنيم كلاسيك", Cat = "men", Price = 950, OldPrice = null, Icon = "ic-jacket", Sizes = new() { "M", "L", "XL" } },
            //    new() { Id = 3, Name = "هودي بيچ مريح", Cat = "men", Price = 650, OldPrice = 820, Icon = "ic-hoodie", Sizes = new() { "S", "M", "L", "XL" } },
            //    new() { Id = 4, Name = "بنطلون تيلر أسود", Cat = "men", Price = 700, OldPrice = null, Icon = "ic-trousers", Sizes = new() { "30", "32", "34", "36" } },
            //    new() { Id = 5, Name = "قميص كتان فاتح", Cat = "men", Price = 550, OldPrice = null, Icon = "ic-shirt", Sizes = new() { "S", "M", "L", "XL" } },
            //    new() { Id = 6, Name = "فستان صيفي مطبع بالورد", Cat = "women", Price = 800, OldPrice = 1000, Icon = "ic-dress", Sizes = new() { "S", "M", "L" } },
            //    new() { Id = 7, Name = "معطف صوف كاميل", Cat = "women", Price = 1800, OldPrice = null, Icon = "ic-coat", Sizes = new() { "S", "M", "L" } },
            //    new() { Id = 8, Name = "سويتر تريكو حريمي", Cat = "women", Price = 600, OldPrice = null, Icon = "ic-sweater", Sizes = new() { "S", "M", "L", "XL" } },
            //    new() { Id = 9, Name = "شنطة كروس جلد", Cat = "accessories", Price = 700, OldPrice = null, Icon = "ic-bag", Sizes = new() { "مقاس واحد" } },
            //    new() { Id = 10, Name = "قبعة كاجوال", Cat = "accessories", Price = 250, OldPrice = 340, Icon = "ic-cap", Sizes = new() { "مقاس واحد" } },
            //};

            var products = await productService.GetAllProducts();

            return View(products);
        }



    }
}
