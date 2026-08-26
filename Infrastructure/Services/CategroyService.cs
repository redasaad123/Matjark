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
    public class CategroyService : ICategoryService
    {
        private readonly IUnitOfWork<Category> categoryUnitOfWork;

        public CategroyService(IUnitOfWork<Category> CategoryUnitOfWork)
        {
            categoryUnitOfWork = CategoryUnitOfWork;
        }
        public async Task<IEnumerable<CategoryViewModel>> GetCategory()
        {
            var cat = await categoryUnitOfWork.Entity.GetAllAsync();
            if (cat.Count() == 0) 
                return Enumerable.Empty<CategoryViewModel>();

            var mapping = cat.Select(x => new CategoryViewModel
            {
                Id = x.Id,
                CategoryName = x.Name

            }).ToList();
            return mapping;
        }
        public async Task<Category> AddCategory(CategoryViewModel model)
        {
            var cat = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.CategoryName,

            };

            await categoryUnitOfWork.Entity.AddAsync(cat);
            categoryUnitOfWork.SaveChanges();
            return cat;
        }


        public async Task<Category> UpdateCategory( CategoryViewModel model , string id)
        {
            var cat =await categoryUnitOfWork.Entity.GetById(id);
            if (cat == null)
                return null;

            cat.Name = model.CategoryName;
            
            await categoryUnitOfWork.Entity.UpdateAsync(cat);
            categoryUnitOfWork.SaveChanges();
            return cat;



            
            
        }

        public async Task DeleteCategory(string id)
        {
            var cat = await categoryUnitOfWork.Entity.GetById(id);
            if (cat == null) { }

            categoryUnitOfWork.Entity.Delete(cat);

            categoryUnitOfWork.SaveChanges();

        }
    }
}
