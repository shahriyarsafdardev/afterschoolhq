using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Models
{
    public class PagingModel
    {
        public int RecordsPerPage { get; set; }
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int TotalCount { get; set; }
        public int StartingRecord { get { return (RecordsPerPage * (PageNumber - 1)) + 1; } }
        public int EndingRecord { get { return Math.Min(TotalCount, RecordsPerPage * PageNumber); } }
    }
}