using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ViewModel
{
   public class OrderCreateRequest
    {
        public CustomerCreateRequest Customer { get; set; }
        public List<OrderLineCreateRequest> OrderLines { get; set; }
    }
}
