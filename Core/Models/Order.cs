using Core.enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<MissingOrderLine> MissingOrderLines { get; set; }
    }
}
