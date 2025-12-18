using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Front.DTOs.Kitchen
{
    public class ItemUpdate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? SpecialPrice { get; set; }
        public bool IsSpecial { get; set; }
        public bool IsAvailable { get; set; }
        public int? CategoryId { get; set; }
    }
}
