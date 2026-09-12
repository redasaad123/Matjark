using Infrastructure.InterFace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Project.Controllers
{
    public class ShopifyController : Controller
    {
        private readonly IShopifyService _shopifyService;
        private readonly ILogger<ShopifyController> _logger;

        public ShopifyController(IShopifyService shopifyService, ILogger<ShopifyController> logger)
        {
            _shopifyService = shopifyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            bool isConnected = await _shopifyService.TestConnectionAsync();
            ViewBag.IsConnected = isConnected;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SyncProducts()
        {
            int count = await _shopifyService.ImportProductsFromShopifyAsync();
            TempData["SuccessMessage"] = $"تمت مزامنة واستيراد {count} من المنتجات من Shopify بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SyncOrders()
        {
            int count = await _shopifyService.ImportOrdersFromShopifyAsync();
            TempData["SuccessMessage"] = $"تمت مزامنة واستيراد {count} من الطلبات من Shopify بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ExportProduct(string id)
        {
            bool success = await _shopifyService.ExportProductToShopifyAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "تم تصدير/تحديث المنتج على Shopify بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء تصدير المنتج إلى Shopify.";
            }
            return RedirectToAction("Index", "Product");
        }

        [HttpPost("/api/shopify/webhooks")]
        public async Task<IActionResult> ReceiveWebhook()
        {
            string hmacHeader = Request.Headers["X-Shopify-Hmac-SHA256"].ToString();
            string topic = Request.Headers["X-Shopify-Topic"].ToString();

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            string jsonBody = await reader.ReadToEndAsync();

            bool isValid = _shopifyService.VerifyWebhookHmac(jsonBody, hmacHeader);
            if (!isValid)
            {
                _logger.LogWarning("Unauthorized Shopify Webhook request rejected (HMAC mismatch).");
                return Unauthorized();
            }

            await _shopifyService.ProcessWebhookAsync(topic, jsonBody);
            return Ok();
        }
    }
}
