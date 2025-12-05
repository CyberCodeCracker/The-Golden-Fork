using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.DTOs
{
    public class MenuRequest
    {
        [Column("Name")]
        public String Name { get; set; }
        [Column("Description")]
        public String? Description { get; set; }
    }
}
