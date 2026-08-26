using Core.enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ViewModel
{
    public class AddProductViewModel : ProductViewModel
    {
        public List<IFormFile>? Images { get; set; }

        public List<Sizes>? Sizes { get; set; } = new();

    }
}
