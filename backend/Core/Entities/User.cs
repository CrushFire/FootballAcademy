using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Role { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public Personal? Personal { get; set; }
        public Sportsman? Sportsman { get; set; }
    }
}
