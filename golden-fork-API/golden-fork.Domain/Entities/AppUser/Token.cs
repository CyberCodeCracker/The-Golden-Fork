using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.AppUser
{
    public class Token : BaseEntity
    {
        public string Value { get; set; } = null;
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
