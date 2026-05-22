using Core.Models;
using Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utils
{
    public static class Validator
    {
        public static bool IsValidPagination(Pagination pag, out string error)
        {
            if (pag.Page < 1)
            {
                error = "Номер страницы не может быть отрицательным или равен 0";
                return false;
            }

            if (pag.PageSize <= 0)
            {
                error = "Размер страницы должен быть больше 0";
                return false;
            }

            error = null;
            return true;
        }
    }
}
