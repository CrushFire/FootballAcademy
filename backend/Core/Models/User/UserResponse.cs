using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.User
{
    public class UserResponse
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
