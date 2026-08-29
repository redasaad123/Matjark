using Core.Models;
using Infrastructure.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.InterFace.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryViewModel>> GetCategory();
        Task<CategoryViewModel?> GetCategoryById(string id);
        Task<Category> AddCategory(CategoryViewModel model);
        Task<Category> UpdateCategory(CategoryViewModel model , string id);
        Task DeleteCategory(string id);

    }
}
