using Core.enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Products
    {
        public string Id { get; set; }

        public string Name { get; set; }
        [ForeignKey("CategoryId")]
        public string CategoryId { get; set; }
        [NotMapped]
        public Category Category { get; set; }

        public string Gender { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public List<Sizes> Sizes { get; set; }

        public bool IsDiscounted { get; set; }

        public decimal DiscountPercentage { get; set; }


    }
}
