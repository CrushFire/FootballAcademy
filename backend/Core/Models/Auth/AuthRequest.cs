using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Auth
{
    public class AuthRequest
    {
        [Required]
        public string Password { get; set; }
        // Можно прислать либо логин, либо email — сервер сам определит.
        // Email оставлен для обратной совместимости старых клиентов.
        public string? Identifier { get; set; }
        public string? Email { get; set; }
    }
}
