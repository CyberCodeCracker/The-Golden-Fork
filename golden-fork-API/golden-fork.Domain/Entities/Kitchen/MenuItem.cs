using golden_fork.core.Entities.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.Kitchen
{
    public class MenuItem
    {
        public int MenuId { get; set; }
        public Menu Menu { get; set; } = null!;
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;
        // Position to determine the order of items in the menu
        public int Position { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsSpecial { get; set; }
        public decimal? SpecialPrice { get; set; }
    }
}
