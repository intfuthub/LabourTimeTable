//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LabourTimeTable.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class joblabwork
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