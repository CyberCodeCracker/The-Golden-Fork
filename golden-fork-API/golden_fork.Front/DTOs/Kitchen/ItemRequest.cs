using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Front.DTOs.Kitchen
{
    public class ItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; }
        public decimal? SpecialPrice { get; set; }
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsSpecial { get; set; } = false;
    }
}
