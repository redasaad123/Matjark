using Core.enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ViewModel
{
    public class GetProductViewModel : ProductViewModel
    {
        public decimal? Price { get; set; }
        
        public List<string>? Sizes { get; set; } = new();

    }
}
