using Core.Models;
using Infrastructure.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.InterFace.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(OrderCreateRequest order);
        Task<Order> GetOrderByIdAsync(string id);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> UpdateOrderAsync(Order order);
        Task<Order> MarkProductAsMissingAsync(string orderId, string productId);
        Task<Order> RestoreProductToOrderAsync(string orderId, string missingProductId);
        Task<bool> DeleteOrderAsync(string orderId);
    }
}
