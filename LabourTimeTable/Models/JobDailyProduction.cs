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
    
    public partial class JobDailyProduction
    {
        public long Id { get; set; }
        public string doc_no { get; set; }
        public string jobsystemId { get; set; }
        public string jobno { get; set; }
        public Nullable<int> Jobcode { get; set; }
        public Nullable<decimal> job_per { get; set; }
        public string weight_per { get; set; }
        public string taskname { get; set; }
        public string empcode { get; set; }
        public Nullable<int> qty_man { get; set; }
        public Nullable<decimal> qty_item { get; set; }
        public string unit { get; set; }
        public Nullable<decimal> time_min { get; set; }
        public Nullable<int> factor { get; set; }
        public string tot_time_hour { get; set; }
        public Nullable<decimal> hr_cost { get; set; }
        public string tot_cost { get; set; }
        public Nullable<int> blanklines { get; set; }
        public Nullable<int> srno { get; set; }
        public Nullable<decimal> flageTrans { get; set; }
        public Nullable<decimal> flagecancel { get; set; }
        public Nullable<decimal> flagereplese { get; set; }
        public Nullable<int> flageEdit { get; set; }
        public Nullable<int> flageFix { get; set; }
        public Nullable<decimal> PlanQty { get; set; }
        public string PlanDay { get; set; }
        public Nullable<decimal> QtyDone { get; set; }
        public Nullable<decimal> Qty { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> FromTime { get; set; }
        public Nullable<System.DateTime> ToTime { get; set; }
        public Nullable<System.DateTime> Updatedate { get; set; }
        public string joblabcostId { get; set; }
        public Nullable<System.DateTime> ondate { get; set; }
        public string groupid { get; set; }
    }
}