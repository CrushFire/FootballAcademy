using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Pagination
    {
        [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть больше 0")]
        public int Page { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Размер страницы должен быть больше 0")]
        public int PageSize { get; set; }
    }
}
