using Infrastructure.InterFace.Services;
using Infrastructure.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> AddCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            var cat = categoryService.AddCategory(model);
            return RedirectToAction(nameof(AddCategory));

        }


    }
}
