using Core.Models;
using System.Threading.Tasks;

namespace Infrastructure.InterFace.Services
{
    public interface IShopifyService
    {
        Task<bool> TestConnectionAsync();
        Task<int> ImportProductsFromShopifyAsync();
        Task<bool> ExportProductToShopifyAsync(string productId);
        Task<bool> UpdateShopifyInventoryAsync(long inventoryItemId, int newQuantity);
        Task<int> ImportOrdersFromShopifyAsync();
        Task<bool> FulfillShopifyOrderAsync(long shopifyOrderId);
        Task ProcessWebhookAsync(string topic, string jsonContent);
        bool VerifyWebhookHmac(string jsonContent, string hmacHeader);
    }
}
