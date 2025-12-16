using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Front.DTOs.Kitchen
{
    public class MenuRequest
    {
        [Required(ErrorMessage = "Menu name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3-100 characters")]
        public string Name { get; set; } = string.Empty;
        [DataType(DataType.MultilineText)]
        [MaxLength(500)]
        public string? Description { get; set; }

    }
}
