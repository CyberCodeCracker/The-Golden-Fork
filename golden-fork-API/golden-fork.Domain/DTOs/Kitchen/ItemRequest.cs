using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.DTOs.Kitchen
{
    public class ItemRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        [Required]
        public decimal Price { get; set; }
        public decimal? SpecialPrice { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsSpecial { get; set; } = false;

    }
}
