using Core.Models;
using Infrastructure.InterFace;
using Infrastructure.InterFace.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork<Order> _unitOfWork;

        public OrderService(IUnitOfWork<Order> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            // Generate unique ID for order
            order.Id = Guid.NewGuid().ToString();

            // Calculate total price
            if (order.OrderLines != null && order.OrderLines.Any())
            {
                order.TotalPrice = order.OrderLines.Sum(ol => ol.Price * ol.Quantity);
            }

            // Add to repository
            await _unitOfWork.Entity.AddAsync(order);
            _unitOfWork.SaveChanges();

            return order;
        }

        public async Task<Order> GetOrderByIdAsync(string id)
        {
            return await _unitOfWork.Entity.GetById(id);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _unitOfWork.Entity.GetAllAsync();
        }
    }
}
