using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Front.DTOs.Kitchen
{
    public class MenuItemRequest
    {

        [Range(0, 100)]
        public int Position { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public String Name { get; set; } = string.Empty;
        public String Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsSpecial { get; set; } = false;
        public decimal? SpecialPrice { get; set; }
    }
}
