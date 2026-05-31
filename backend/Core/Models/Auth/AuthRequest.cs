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
        // Идентификатор: логин или email — в зависимости от выбранного на фронте режима.
        public string? Identifier { get; set; }
        // Mode: "login" | "email". Управляет тем, по какому полю искать пользователя.
        // Если не задан — ищем по обоим (обратная совместимость).
        public string? Mode { get; set; }
        // Email оставлен для обратной совместимости старых клиентов.
        public string? Email { get; set; }
    }
}
