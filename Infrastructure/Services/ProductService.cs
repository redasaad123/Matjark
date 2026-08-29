using Core.enums;
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
    public class ProductService: IProductService
    {
        private readonly IUnitOfWork<Products> productsUnitOfWork;
        private readonly IUnitOfWork<Category> categoryUnitOfWork;

        public ProductService(IUnitOfWork<Products> ProductsUnitOfWork , IUnitOfWork<Category> CategoryUnitOfWork   )
        {
            productsUnitOfWork = ProductsUnitOfWork;
            categoryUnitOfWork = CategoryUnitOfWork;
        }

        public async Task<IEnumerable<ProductViewModel>> GetAllProducts()
        {
            var products = await productsUnitOfWork.Entity.GetAllAsync();

            if (products == null) 
                return Enumerable.Empty<ProductViewModel>();

            var categories = await categoryUnitOfWork.Entity.GetAllAsync();
            var categoryDict = categories?.ToDictionary(c => c.Id, c => c.Name) ?? new Dictionary<string, string>();

            var Mapping = products.Select(x => new GetProductViewModel
            {
               Id = x.Id,
               Name = x.Name,
               Cat = (!string.IsNullOrEmpty(x.CategoryId) && categoryDict.ContainsKey(x.CategoryId)) 
                   ? categoryDict[x.CategoryId] 
                   : "غير محدد",
               Description = x.Description,
               DiscountPercentage = x.DiscountPercentage,
               Gender = x.Gender,
               GetImages = x.ImageUrl,
               IsDiscounted = x.IsDiscounted,
               Price = x.UnitPrice,
               OldPrice = x.OldPrice,
               Quantity = x.Quantity,
               Sizes = x.Sizes?.Select(s => s.ToString()).ToList() ?? new List<string>(),
            }).ToList();
            return Mapping;
        }

        public async Task<AddProductViewModel?> GetProductById(string id)
        {
            var product = await productsUnitOfWork.Entity.GetById(id);
            if (product == null)
                return null;

            return new AddProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Cat = product.CategoryId,
                Description = product.Description,
                DiscountPercentage = product.DiscountPercentage,
                Gender = product.Gender,
                GetImages = product.ImageUrl,
                IsDiscounted = product.IsDiscounted,
                OldPrice = product.OldPrice,
                Quantity = product.Quantity,
                Sizes = product.Sizes ?? new List<Sizes>()
            };
        }

        public async Task<GetProductViewModel?> GetProductDetails(string id)
        {
            var product = await productsUnitOfWork.Entity.GetById(id);
            if (product == null)
                return null;

            var categories = await categoryUnitOfWork.Entity.GetAllAsync();
            var categoryDict = categories?.ToDictionary(c => c.Id, c => c.Name) ?? new Dictionary<string, string>();

            return new GetProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Cat = (!string.IsNullOrEmpty(product.CategoryId) && categoryDict.ContainsKey(product.CategoryId))
                    ? categoryDict[product.CategoryId]
                    : "غير محدد",
                Description = product.Description,
                DiscountPercentage = product.DiscountPercentage,
                Gender = product.Gender,
                GetImages = product.ImageUrl,
                IsDiscounted = product.IsDiscounted,
                Price = product.UnitPrice,
                OldPrice = product.OldPrice,
                Quantity = product.Quantity,
                Sizes = product.Sizes?.Select(s => s.ToString()).ToList() ?? new List<string>()
            };
        }

        public async Task<Products> AddProduct(AddProductViewModel model)
        {
            if (model == null) 
                throw new ArgumentNullException("model");
  
            var product = new Products
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                OldPrice = model.OldPrice,
                Sizes =  model.Sizes?.ToList() ?? new List<Sizes>(),
                Quantity = model.Quantity,
                DiscountPercentage = model.DiscountPercentage,
                Description = model.Description,
                IsDiscounted = model.IsDiscounted,
                CategoryId = model.Cat,
                Gender = model.Gender,
                ImageUrl = model?.GetImages

            };

            await productsUnitOfWork.Entity.AddAsync(product);
            productsUnitOfWork.SaveChanges();
            return product;
        }

        public async Task<Products?> UpdateProduct(string id, AddProductViewModel model)
        {
            var product = await productsUnitOfWork.Entity.GetById(id);
            if (product == null)
                return null;

            product.Name = model.Name;
            product.CategoryId = model.Cat;
            product.Description = model.Description;
            product.DiscountPercentage = model.DiscountPercentage;
            product.Gender = model.Gender;
            product.IsDiscounted = model.IsDiscounted;
            product.OldPrice = model.OldPrice;
            product.Quantity = model.Quantity;
            if (model.Sizes != null)
            {
                product.Sizes = model.Sizes;
            }
            product.ImageUrl = model.GetImages ?? new List<string>();

            await productsUnitOfWork.Entity.UpdateAsync(product);
            productsUnitOfWork.SaveChanges();
            return product;
        }

        public async Task<List<string>?> DeleteProduct(string id)
        {
            var product = await productsUnitOfWork.Entity.GetById(id);
            if (product != null)
            {
                var images = product.ImageUrl;
                productsUnitOfWork.Entity.Delete(product);
                productsUnitOfWork.SaveChanges();
                return images;
            }
            return null;
        }




    }
}
