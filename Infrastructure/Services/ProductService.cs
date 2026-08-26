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


            var Mapping = products.Select(x => new GetProductViewModel
            {
               Id = x.Id,
               Name = x.Name,
               Cat = categoryUnitOfWork.Entity.GetById(x.CategoryId).Result.Name,
               Description = x.Description,
               DiscountPercentage = x.DiscountPercentage,
               Gender = x.Gender,
               GetImages = x.ImageUrl,
               IsDiscounted = x.IsDiscounted,
               Price = x.UnitPrice,
               Quantity = x.Quantity,
               Sizes =  x.Sizes.Select(s => s.ToString()).ToList(),
            }).ToList();
            return Mapping;
            
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
                Sizes =  model.Sizes.ToList(),
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




    }
}
