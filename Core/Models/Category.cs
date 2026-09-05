using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Category
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }

        public string? NameInArabic { get; set; }

        public List<Products>? Products { get; set; }
    }
}
