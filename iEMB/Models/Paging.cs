using System;
using System.Collections.Generic;
using System.Text;

namespace iEMB.Models
{
    public class Paging
    {
        public int TotalPage { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
