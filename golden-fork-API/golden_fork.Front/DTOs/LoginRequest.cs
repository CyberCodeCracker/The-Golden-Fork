using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Front.DTOs
{
    public class LoginRequest
    {
        public String Email { get; set; } = string.Empty;
        public String Password { get; set; } = string.Empty;
    }
}
