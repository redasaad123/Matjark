using Core.enums;
using Core.Services;
using Infrastructure.InterFace.Services;
using Infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Project.Controllers
{
    public class ManageController : Controller
    {
        private readonly IProductService productService;
        private readonly ImageStore imageStore;
        private readonly ICategoryService categoryService;
        private readonly IStorageBlobService storageBlob;

        public ManageController(IProductService productService, ImageStore imageStore , ICategoryService categoryService , IStorageBlobService storageBlob)
        {
            this.productService = productService;
            this.imageStore = imageStore;
            this.categoryService = categoryService;
            this.storageBlob = storageBlob;
        }
        
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var products = await productService.GetAllProducts();
            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> AddProducts()
        {
            var cat = await categoryService.GetCategory();
            ViewBag.Category = cat;
            ViewBag.Sizes = Enum.GetValues<Sizes>().ToList();
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProducts(AddProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Category = await categoryService.GetCategory();
                ViewBag.Sizes = Enum.GetValues<Sizes>().ToList();
                return View(model);
            }

            var url = new List<string>();

            if (model.Images != null && model.Images.Count > 0)
            {
                try
                {
                    url = await storageBlob.UploadFileAsync(model.Images);
                }
                catch
                {
                    url = await imageStore.StoreImageAsync(model.Images);
                }

                model.GetImages = url;
            }

            var products = await productService.AddProduct(model);

            return RedirectToAction(nameof(Index));
        }
        
        [Authorize]
        public async Task<IActionResult> EditProducts(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var product = await productService.GetProductById(id);
            if (product == null)
                return NotFound();

            ViewBag.Category = await categoryService.GetCategory();
            ViewBag.Sizes = Enum.GetValues<Sizes>().ToList();
            return View(product);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProducts(string id, AddProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Category = await categoryService.GetCategory();
                ViewBag.Sizes = Enum.GetValues<Sizes>().ToList();
                return View(model);
            }

            if (model.DeletedImages != null && model.DeletedImages.Any())
            {
                model.GetImages?.RemoveAll(x => model.DeletedImages.Contains(x));
                imageStore.DeleteImages(model.DeletedImages);
            }

            var currentImages = model.GetImages?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList() ?? new List<string>();

            if (model.Images != null && model.Images.Count > 0)
            {
                List<string> newUrls;
                try
                {
                    newUrls = await storageBlob.UploadFileAsync(model.Images);
                }
                catch
                {
                    newUrls = await imageStore.StoreImageAsync(model.Images);
                }

                if (newUrls != null && newUrls.Any())
                {
                    currentImages.AddRange(newUrls);
                }
            }

            model.GetImages = currentImages;

            var updated = await productService.UpdateProduct(id, model);
            if (updated == null)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var deletedImages = await productService.DeleteProduct(id);
                if (deletedImages != null && deletedImages.Any())
                {
                    imageStore.DeleteImages(deletedImages);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
