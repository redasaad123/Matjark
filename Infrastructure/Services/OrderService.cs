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
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork<Order> _unitOfWork;

        public OrderService(IUnitOfWork<Order> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> CreateOrderAsync(OrderCreateRequest request)
        {

            


            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                Customer = new Customer
                {
                    Name = request.Customer?.Name ?? "عميل",
                    Email = request.Customer?.Email ?? "",
                    PhoneNumber = request.Customer?.PhoneNumber ?? "",
                    Address = request.Customer?.Address ?? ""
                },
                OrderLines = request.OrderLines.Select(ol => new OrderLine
                {
                    ProductId = ol.ProductId,
                    ProductName = ol.ProductName,
                    Quantity = ol.Quantity,
                    Price = ol.Price,
                    Size = ParseSize(ol.Size)
                }).ToList()
            };

            // Generate unique ID for order
            

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

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            await _unitOfWork.Entity.UpdateAsync(order);
            _unitOfWork.SaveChanges();

            return order;
        }

        public async Task<Order> MarkProductAsMissingAsync(string orderId, string productId)
        {
            var order = await _unitOfWork.Entity.GetById(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order with id {orderId} not found");

            // Find the order line to remove
            var missingLine = order.OrderLines?.FirstOrDefault(ol => ol.ProductId == productId);
            if (missingLine == null)
                throw new InvalidOperationException($"Product with id {productId} not found in order");

            // Add to missing lines list
            if (order.MissingOrderLines == null)
                order.MissingOrderLines = new List<MissingOrderLine>();

            order.MissingOrderLines.Add(new MissingOrderLine
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = missingLine.ProductId,
                ProductName = missingLine.ProductName,
                Quantity = missingLine.Quantity,
                Price = missingLine.Price,
                Size = missingLine.Size
            });

            // Remove from order lines
            order.OrderLines.Remove(missingLine);

            // Recalculate total price
            if (order.OrderLines.Any())
            {
                order.TotalPrice = order.OrderLines.Sum(ol => ol.Price * ol.Quantity);
            }
            else
            {
                order.TotalPrice = 0;
            }

            // Update order in database
            await _unitOfWork.Entity.UpdateAsync(order);
            _unitOfWork.SaveChanges();

            return order;
        }

        public async Task<Order> RestoreProductToOrderAsync(string orderId, string missingProductId)
        {
            var order = await _unitOfWork.Entity.GetById(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order with id {orderId} not found");

            if (order.MissingOrderLines == null || order.MissingOrderLines.Count == 0)
                throw new InvalidOperationException("No missing products found in this order");

            // Find the missing line to restore
            var missingLine = order.MissingOrderLines.FirstOrDefault(ml => ml.ProductId == missingProductId);
            if (missingLine == null)
                throw new InvalidOperationException($"Missing product with id {missingProductId} not found");

            // Create new OrderLine from missing line
            var restoredOrderLine = new OrderLine
            {
                ProductId = missingLine.ProductId,
                ProductName = missingLine.ProductName,
                Quantity = missingLine.Quantity,
                Price = missingLine.Price,
                Size = missingLine.Size
            };

            // Add back to order lines
            if (order.OrderLines == null)
                order.OrderLines = new List<OrderLine>();

            order.OrderLines.Add(restoredOrderLine);

            // Remove from missing lines
            order.MissingOrderLines.Remove(missingLine);

            // Recalculate total price
            order.TotalPrice = order.OrderLines.Sum(ol => ol.Price * ol.Quantity);

            // Update order in database
            await _unitOfWork.Entity.UpdateAsync(order);
            _unitOfWork.SaveChanges();

            return order;
        }

        public async Task<bool> DeleteOrderAsync(string orderId)
        {
            try
            {
                var order = await _unitOfWork.Entity.GetById(orderId);
                if (order == null)
                    return false;

                _unitOfWork.Entity.Delete(order);
                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private Sizes ParseSize(string sizeString)
        {
            if (string.IsNullOrEmpty(sizeString))
                return Sizes.M; // Default size

            // Try to parse the size string
            if (Enum.TryParse<Sizes>(sizeString, ignoreCase: true, out var size))
            {
                return size;
            }

            // Handle custom Arabic sizes or patterns
            return sizeString.ToLower() switch
            {
                "مقاس واحد" => Sizes.M,
                "s" => Sizes.S,
                "m" => Sizes.M,
                "l" => Sizes.L,
                "xl" => Sizes.XL,
                "xxl" => Sizes.XXl,
                "xxxl" => Sizes.XXXL,
                _ => Sizes.M
            };
        }
    }
}
