namespace Core.Models
{
    public class ShopifySettings
    {
        public string ShopUrl { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}

