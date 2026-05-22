using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Filter
    {
        public Filters? Filters { get; set; }
        public Sort? Sort { get; set; }
        public Pagination? Pagination { get; set; }
    }
}
