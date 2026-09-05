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
    public class Products
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }
        [ForeignKey("CategoryId")]
        public string CategoryId { get; set; }
        [NotMapped]
        public Category? Category { get; set; }

        public string Gender { get; set; }

        public List<string>? ImageUrl { get; set; }

        public decimal? OldPrice { get; set; }
        public decimal? UnitPrice => IsDiscounted ? OldPrice - (OldPrice * DiscountPercentage) : OldPrice   ;

        public int Quantity { get; set; }

        public List<Sizes> Sizes { get; set; }

        public bool IsDiscounted { get; set; } = false;

        public decimal? DiscountPercentage { get; set; }




    }
}
