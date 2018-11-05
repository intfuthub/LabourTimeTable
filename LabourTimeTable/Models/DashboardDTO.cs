using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class DashboardDTO
    {

        //pagination
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PagerCount { get; set; }

        public List<DashboardGridDTO> _DashboardGridDTO { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }

    }

    public class DashboardGridDTO
    {
        public string doc_no { get; set; }
        public string jobno { get; set; }
        public string jobsystemId { get; set; }
        public Nullable<decimal> qty_item { get; set; }
        public Nullable<int> rowno { get; set; }
        public string joblabcostId { get; set; }
        public Nullable<System.DateTime> jobdate { get; set; }
        public Nullable<int> FlageMod { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> fromtime { get; set; }
        public Nullable<System.DateTime> totime { get; set; }
        public Nullable<int> JobCode { get; set; }
        public long Id { get; set; }
        public Nullable<decimal> time_min { get; set; }
        public string activity { get; set; }
        public string SystemName { get; set; }
        public Nullable<decimal> hr_cost { get; set; }
        public Nullable<int> qty_man { get; set; }
        public Nullable<decimal> totTime_hrs { get; set; }

    }
}