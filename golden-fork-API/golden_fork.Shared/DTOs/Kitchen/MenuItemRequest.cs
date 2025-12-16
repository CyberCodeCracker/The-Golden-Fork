using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.DTOs.Kitchen
{
    public class MenuItemRequest
    {

        [Range(0, 100)]
        public int Position { get; set; }

        public bool IsAvailable { get; set; } = true;
        public bool IsSpecial { get; set; } = false;
        public decimal? SpecialPrice { get; set; }
    }
}
