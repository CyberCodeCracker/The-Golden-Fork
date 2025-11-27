using golden_fork.core.Entities.Menu;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.Kitchen
{
    public class ItemImage : BaseEntity
    {
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; } = null!;

        public String Url { get; set; } = String.Empty;
        // thumbnail
        public bool IsMain { get; set; } = false;
    }
}
