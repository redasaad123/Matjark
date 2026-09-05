using Core.Models;
using Infrastructure.InterFace;
using Infrastructure.InterFace.Services;
using Infrastructure.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork<Category> categoryUnitOfWork;

        public CategoryService(IUnitOfWork<Category> CategoryUnitOfWork)
        {
            categoryUnitOfWork = CategoryUnitOfWork;
        }
        public async Task<IEnumerable<CategoryViewModel>> GetCategory()
        {
            var cat = await categoryUnitOfWork.Entity.GetAllAsync();
            if (cat == null || !cat.Any()) 
                return Enumerable.Empty<CategoryViewModel>();

            var mapping = cat.Select(x => new CategoryViewModel
            {
                Id = x.Id,
                CategoryName = x.Name,
                CategoryNameInArabic = x.NameInArabic


            }).ToList();
            return mapping;
        }

        public async Task<CategoryViewModel?> GetCategoryById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            var cat = await categoryUnitOfWork.Entity.GetById(id);
            if (cat == null)
                return null;

            return new CategoryViewModel
            {
                Id = cat.Id,
                CategoryName = cat.Name,
                CategoryNameInArabic = cat.NameInArabic

            };
        }

        public async Task<Category> AddCategory(CategoryViewModel model)
        {
            var cat = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.CategoryName,
                NameInArabic = model.CategoryNameInArabic

            };

            await categoryUnitOfWork.Entity.AddAsync(cat);
            await categoryUnitOfWork.SaveChangesAsync();
            return cat;
        }


        public async Task<Category?> UpdateCategory( CategoryViewModel model , string id)
        {
            var cat =await categoryUnitOfWork.Entity.GetById(id);
            if (cat == null)
                return null;

            cat.Name = model.CategoryName;
            cat.NameInArabic = model.CategoryNameInArabic;

            await categoryUnitOfWork.Entity.UpdateAsync(cat);
            await categoryUnitOfWork.SaveChangesAsync();
            return cat;
        }

        public async Task DeleteCategory(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            var cat = await categoryUnitOfWork.Entity.GetById(id);
            if (cat == null) return;

            categoryUnitOfWork.Entity.Delete(cat);

            await categoryUnitOfWork.SaveChangesAsync();

        }
    }
}
