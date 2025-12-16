using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Front.DTOs
{
    public class RegistrationRequest
    {
        [Column("Username")]
        public string Username { get; set; }
        [Column("Email")]

        public string Email { get; set; }
        [Column("Password")]
        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
        [Column("PhoneNumber")]

        public string? PhoneNumber { get; set; }
        [Column("Address")]

        public string? Address { get; set; }
    }
}
