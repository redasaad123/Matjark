using Core.enums;
using Core.Services;
using Infrastructure.InterFace.Services;
using Infrastructure.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Project.Controllers
{
    public class ManageController : Controller
    {
        private readonly IProductService productService;
        private readonly ImageStore imageStore;
        private readonly ICategoryService categoryService;

        public ManageController(IProductService productService, ImageStore imageStore , ICategoryService categoryService)
        {
            this.productService = productService;
            this.imageStore = imageStore;
            this.categoryService = categoryService;
        }
        public async Task<IActionResult> AddProducts()
        {
            var cat =await categoryService.GetCategory();
            ViewBag.Category = cat;
            ViewBag.Sizes = Enum.GetValues<Sizes>().ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProducts(AddProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var url = new List<string>();

            if (model.Images.Count() != 0)
            {
                url = imageStore.StoreImage(model.Images);
                model.GetImages = url;
            }

            var products = await productService.AddProduct(model);

            return RedirectToAction(nameof(AddProducts));

        }

    }
}
