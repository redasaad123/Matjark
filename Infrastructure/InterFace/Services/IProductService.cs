using Core.Models;
using Infrastructure.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.InterFace.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductViewModel>> GetAllProducts();
        Task<Products> AddProduct(AddProductViewModel model);
        Task<AddProductViewModel?> GetProductById(string id);
        Task<GetProductViewModel?> GetProductDetails(string id);
        Task<Products?> UpdateProduct(string id, AddProductViewModel model);
        Task<List<string>?> DeleteProduct(string id);
    }
}
