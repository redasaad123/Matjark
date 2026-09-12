using Core.enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Order
    {
        [Key]
        public string Id { get; set; }

        public List<OrderLine> OrderLines { get; set; }

        public Customer Customer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<MissingOrderLine> MissingOrderLines { get; set; }

        // Shopify Sync Tracking
        public long? ShopifyOrderId { get; set; }
        public string? ShopifyOrderNumber { get; set; }
    }
}
