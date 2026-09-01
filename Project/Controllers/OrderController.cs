using Core.enums;
using Core.Models;
using Infrastructure.InterFace.Services;
using Infrastructure.Services;
using Infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService , ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // عرض قائمة جميع الطلبات
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        // عرض تفاصيل الطلب الواحد
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            try
            {
                if (request == null || request.OrderLines == null || !request.OrderLines.Any())
                {
                    return BadRequest(new { message = "الطلب فارغ أو بيانات غير صحيحة" });
                }

                var createdOrder = await _orderService.CreateOrderAsync(request);

                return Ok(new { id = createdOrder.Id, message = "تم إنشاء الطلب بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "خطأ في إنشاء الطلب" });
            }
        }

        // تحديث حالة الطلب - تم التعبئة
        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            order.Status = OrderStatus.Completed;
            await _orderService.UpdateOrderAsync(order);

            return RedirectToAction(nameof(Index));
        }

        // تحديث حالة الطلب - فيه حاجة ناقصة
        [HttpPost]
        public async Task<IActionResult> MarkAsMissingItems(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            order.Status = OrderStatus.MissingItems;
            await _orderService.UpdateOrderAsync(order);

            return RedirectToAction(nameof(Index));
        }

        // تحديث حالة الطلب - تم التسليم للمورد
        [HttpPost]
        public async Task<IActionResult> MarkAsDeliveredToSupplier(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            order.Status = OrderStatus.DeliveredToSupplier;
            await _orderService.UpdateOrderAsync(order);

            return RedirectToAction(nameof(Index));
        }

        // وضع علامة على منتج معين كناقص
        [HttpPost]
        public async Task<IActionResult> MarkProductAsMissing(string orderId, string productId)
        {
            try
            {
                var order = await _orderService.MarkProductAsMissingAsync(orderId, productId);
                return RedirectToAction(nameof(Details), new { id = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking product as missing in order {orderId}");
                return BadRequest(new { message = "خطأ في تحديث حالة المنتج" });
            }
        }

        // استرجاع منتج من قائمة الناقصة إلى الأصلية
        [HttpPost]
        public async Task<IActionResult> RestoreProduct(string orderId, string missingProductId)
        {
            try
            {
                var order = await _orderService.RestoreProductToOrderAsync(orderId, missingProductId);
                return RedirectToAction(nameof(Details), new { id = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring product in order {orderId}");
                return BadRequest(new { message = "خطأ في استرجاع المنتج" });
            }
        }

        // حذف الطلب بالكامل
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (!result)
                    return NotFound();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order {id}");
                return BadRequest(new { message = "خطأ في حذف الطلب" });
            }
        }

        
    }
}
