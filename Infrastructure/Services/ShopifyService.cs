using Core.Models;
using Infrastructure.InterFace.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ShopifyService : Infrastructure.InterFace.Services.IShopifyService
    {
        private readonly ShopifySettings _settings;
        private readonly AppDBContext _dbContext;
        private readonly ILogger<ShopifyService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public ShopifyService(
            IOptions<ShopifySettings> settings,
            AppDBContext dbContext,
            ILogger<ShopifyService> logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache)
        {
            _settings = settings.Value;
            _dbContext = dbContext;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        // Method للحصول على Access Token تلقائياً وتخزينه في الكاش لمدة 23 ساعة
        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_settings.ClientSecret))
            {
                return _settings.ClientSecret;
            }

            const string cacheKey = "Shopify_Access_Token";
            if (_cache.TryGetValue(cacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken;
            }

            var client = _httpClientFactory.CreateClient();
            var requestUrl = $"https://{_settings.ShopUrl}/admin/oauth/access_token";

            var response = await client.PostAsJsonAsync(requestUrl, new
            {
                client_id = _settings.ClientId,
                client_secret = _settings.ClientSecret,
                grant_type = "client_credentials"
            });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to fetch Shopify token: {err}");
                throw new Exception("Could not authenticate with Shopify API.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<ShopifyTokenResponse>();
            var token = tokenResponse?.AccessToken ?? throw new Exception("Token received is null.");

            // Cache token for 23 hours
            _cache.Set(cacheKey, token, TimeSpan.FromHours(23));
            return token;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                var shopService = new ShopService(_settings.ShopUrl, accessToken);
                var shop = await shopService.GetAsync();
                return shop != null && !string.IsNullOrEmpty(shop.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Shopify API.");
                return false;
            }
        }

        public async Task<int> ImportProductsFromShopifyAsync()
        {
            int importedCount = 0;
            try
            {
                var accessToken = await GetAccessTokenAsync();
                var shopifyProductService = new ShopifySharp.ProductService(_settings.ShopUrl, accessToken);
                var shopifyProducts = await shopifyProductService.ListAsync();

                var defaultCategory = await _dbContext.Categories.FirstOrDefaultAsync();
                if (defaultCategory == null)
                {
                    defaultCategory = new Category
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "General Shopify"
                    };
                    await _dbContext.Categories.AddAsync(defaultCategory);
                    await _dbContext.SaveChangesAsync();
                }

                foreach (var sProduct in shopifyProducts.Items)
                {
                    var existingProduct = await _dbContext.Products
                        .FirstOrDefaultAsync(p => p.ShopifyProductId == sProduct.Id || p.Name == sProduct.Title);

                    var firstVariant = sProduct.Variants?.FirstOrDefault();
                    var price = firstVariant?.Price ?? 0m;
                    var quantity = Convert.ToInt32(firstVariant?.InventoryQuantity ?? 0);
                    var images = sProduct.Images?.Select(img => img.Src).ToList() ?? new List<string>();

                    if (existingProduct == null)
                    {
                        var newProduct = new Core.Models.Products
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = sProduct.Title ?? "Shopify Product",
                            Description = sProduct.BodyHtml,
                            CategoryId = defaultCategory.Id,
                            Gender = "Unisex",
                            OldPrice = price,
                            Quantity = quantity,
                            ImageUrl = images,
                            Sizes = new List<Core.enums.Sizes>(),
                            ShopifyProductId = sProduct.Id,
                            ShopifyVariantId = firstVariant?.Id,
                            ShopifyInventoryItemId = firstVariant?.InventoryItemId
                        };
                        await _dbContext.Products.AddAsync(newProduct);
                    }
                    else
                    {
                        existingProduct.Name = sProduct.Title ?? existingProduct.Name;
                        existingProduct.Description = sProduct.BodyHtml ?? existingProduct.Description;
                        existingProduct.OldPrice = price;
                        existingProduct.Quantity = quantity;
                        if (images.Any())
                        {
                            existingProduct.ImageUrl = images;
                        }
                        existingProduct.ShopifyProductId = sProduct.Id;
                        existingProduct.ShopifyVariantId = firstVariant?.Id;
                        existingProduct.ShopifyInventoryItemId = firstVariant?.InventoryItemId;

                        _dbContext.Products.Update(existingProduct);
                    }
                    importedCount++;
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Successfully imported/updated {importedCount} products from Shopify.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing products from Shopify.");
            }

            return importedCount;
        }

        public async Task<bool> ExportProductToShopifyAsync(string productId)
        {
            try
            {
                var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
                if (product == null) return false;

                var accessToken = await GetAccessTokenAsync();
                var shopifyProductService = new ShopifySharp.ProductService(_settings.ShopUrl, accessToken);

                if (product.ShopifyProductId.HasValue)
                {
                    var existingShopifyProduct = await shopifyProductService.GetAsync(product.ShopifyProductId.Value);
                    existingShopifyProduct.Title = product.Name;
                    existingShopifyProduct.BodyHtml = product.Description;

                    if (existingShopifyProduct.Variants != null && existingShopifyProduct.Variants.Any())
                    {
                        existingShopifyProduct.Variants.First().Price = product.UnitPrice ?? product.OldPrice ?? 0m;
                    }

                    await shopifyProductService.UpdateAsync(product.ShopifyProductId.Value, existingShopifyProduct);
                    _logger.LogInformation($"Updated product '{product.Name}' on Shopify.");
                }
                else
                {
                    var newShopifyProduct = new ShopifySharp.Product
                    {
                        Title = product.Name,
                        BodyHtml = product.Description,
                        Variants = new List<ProductVariant>
                        {
                            new ProductVariant
                            {
                                Price = product.UnitPrice ?? product.OldPrice ?? 0m,
                                InventoryQuantity = product.Quantity
                            }
                        }
                    };

                    var created = await shopifyProductService.CreateAsync(newShopifyProduct);
                    product.ShopifyProductId = created.Id;

                    var createdVariant = created.Variants?.FirstOrDefault();
                    if (createdVariant != null)
                    {
                        product.ShopifyVariantId = createdVariant.Id;
                        product.ShopifyInventoryItemId = createdVariant.InventoryItemId;
                    }

                    _dbContext.Products.Update(product);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Exported product '{product.Name}' to Shopify with ID {created.Id}.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting product ID {productId} to Shopify.");
                return false;
            }
        }

        public async Task<bool> UpdateShopifyInventoryAsync(long inventoryItemId, int newQuantity)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                var locationService = new LocationService(_settings.ShopUrl, accessToken);
                var locations = await locationService.ListAsync();
                var locationId = locations.Items.FirstOrDefault()?.Id;

                if (!locationId.HasValue)
                {
                    _logger.LogWarning("No location found on Shopify store to set inventory.");
                    return false;
                }

                var inventoryLevelService = new InventoryLevelService(_settings.ShopUrl, accessToken);
                await inventoryLevelService.SetAsync(new InventoryLevel
                {
                    InventoryItemId = inventoryItemId,
                    LocationId = locationId.Value,
                    Available = newQuantity
                });

                _logger.LogInformation($"Updated Shopify inventory for item {inventoryItemId} to {newQuantity}.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating Shopify inventory for item {inventoryItemId}.");
                return false;
            }
        }

        public async Task<int> ImportOrdersFromShopifyAsync()
        {
            int importedCount = 0;
            try
            {
                var accessToken = await GetAccessTokenAsync();
                var shopifyOrderService = new ShopifySharp.OrderService(_settings.ShopUrl, accessToken);
                var orders = await shopifyOrderService.ListAsync();

                foreach (var sOrder in orders.Items)
                {
                    var existingOrder = await _dbContext.Orders
                        .FirstOrDefaultAsync(o => o.ShopifyOrderId == sOrder.Id);

                    if (existingOrder == null)
                    {
                        var newOrder = new Core.Models.Order
                        {
                            Id = Guid.NewGuid().ToString(),
                            ShopifyOrderId = sOrder.Id,
                            ShopifyOrderNumber = sOrder.OrderNumber?.ToString() ?? sOrder.Name,
                            TotalPrice = sOrder.TotalPrice ?? 0m,
                            CreatedDate = sOrder.CreatedAt?.DateTime ?? DateTime.UtcNow,
                            Status = Core.enums.OrderStatus.Pending,
                            OrderLines = new List<OrderLine>(),
                            MissingOrderLines = new List<MissingOrderLine>(),
                            Customer = new Core.Models.Customer
                            {
                                Name = $"{sOrder.Customer?.FirstName} {sOrder.Customer?.LastName}".Trim(),
                                Email = sOrder.Customer?.Email ?? sOrder.Email ?? "no-email@shopify.com",
                                PhoneNumber = sOrder.Customer?.Phone ?? sOrder.Phone ?? "",
                                Address = sOrder.ShippingAddress?.Address1 ?? "N/A"
                            }
                        };

                        await _dbContext.Orders.AddAsync(newOrder);
                        importedCount++;
                    }
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Successfully imported {importedCount} orders from Shopify.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing orders from Shopify.");
            }

            return importedCount;
        }

        public async Task<bool> FulfillShopifyOrderAsync(long shopifyOrderId)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                var fulfillmentService = new FulfillmentService(_settings.ShopUrl, accessToken);
                var fulfillment = new FulfillmentShipping
                {
                    NotifyCustomer = true
                };

                await fulfillmentService.CreateAsync(fulfillment);
                _logger.LogInformation($"Fulfilled order {shopifyOrderId} on Shopify.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fulfilling order {shopifyOrderId} on Shopify.");
                return false;
            }
        }

        public bool VerifyWebhookHmac(string jsonContent, string hmacHeader)
        {
            if (string.IsNullOrEmpty(hmacHeader) || string.IsNullOrEmpty(_settings.WebhookSecret))
                return false;

            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(_settings.WebhookSecret);
                var bodyBytes = Encoding.UTF8.GetBytes(jsonContent);

                using (var hmac = new HMACSHA256(keyBytes))
                {
                    var hashBytes = hmac.ComputeHash(bodyBytes);
                    var calculatedHmac = Convert.ToBase64String(hashBytes);
                    return calculatedHmac.Equals(hmacHeader, StringComparison.Ordinal);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying webhook HMAC signature.");
                return false;
            }
        }

        public async Task ProcessWebhookAsync(string topic, string jsonContent)
        {
            _logger.LogInformation($"Processing webhook topic: {topic}");

            switch (topic?.ToLowerInvariant())
            {
                case "products/create":
                case "products/update":
                    await ImportProductsFromShopifyAsync();
                    break;

                case "orders/create":
                case "orders/updated":
                    await ImportOrdersFromShopifyAsync();
                    break;

                default:
                    _logger.LogWarning($"Unhandled Webhook Topic: {topic}");
                    break;
            }
        }
    }

    public class ShopifyTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }
}