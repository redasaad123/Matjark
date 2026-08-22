using Core.enums;
using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ViewModel
{
    internal class ProductViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }
      
        public string CategoryId { get; set; }

        public string Gender { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public List<Sizes> Sizes { get; set; }

        public bool IsDiscounted { get; set; }

        public decimal DiscountPercentage { get; set; }

    }
}
