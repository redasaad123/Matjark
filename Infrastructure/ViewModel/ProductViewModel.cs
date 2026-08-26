using Core.enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ViewModel
{
    public class ProductViewModel
    {
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Cat { get; set; } = "man";
        public string? Gender { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<string>? GetImages { get; set; }
        public decimal? OldPrice { get; set; }
        public bool IsDiscounted { get; set; } = false;
        public decimal? DiscountPercentage { get; set; }
    }
}
