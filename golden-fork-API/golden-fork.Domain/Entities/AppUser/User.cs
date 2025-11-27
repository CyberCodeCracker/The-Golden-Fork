using golden_fork.core.Entities.AppCart;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.AppUser

{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string POassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string? PhoneNUmber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Cart Cart { get; set; } = null!;
        public int RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        public UserRole Role { get; set; } = null;
        public ICollection<Token> Tokens { get; set; } = new HashSet<Token>();

    }
}
