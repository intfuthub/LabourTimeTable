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
    
    public partial class enq_tran
    {
        public string doc_no { get; set; }
        public Nullable<System.DateTime> doc_date { get; set; }
        public string qot_no { get; set; }
        public string ord_no { get; set; }
        public string jobsystemId { get; set; }
        public string jobsystemname { get; set; }
        public Nullable<double> tstkgross { get; set; }
        public Nullable<double> disc_per { get; set; }
        public Nullable<double> disc_fix { get; set; }
        public Nullable<double> margin_per { get; set; }
        public Nullable<double> tstkamt { get; set; }
        public Nullable<double> tstkcp { get; set; }
        public Nullable<System.DateTime> tstkdttrn { get; set; }
        public string tcustcode { get; set; }
        public string tstkref { get; set; }
        public Nullable<double> mat_cost { get; set; }
        public Nullable<double> lab_cost { get; set; }
        public Nullable<double> mat_per { get; set; }
        public Nullable<double> lab_per { get; set; }
        public Nullable<System.DateTime> startdate1 { get; set; }
        public Nullable<System.DateTime> startdate2 { get; set; }
        public Nullable<System.DateTime> startdate3 { get; set; }
        public Nullable<System.DateTime> mat_del_date { get; set; }
        public Nullable<System.DateTime> job_hand_date { get; set; }
        public Nullable<byte> show1 { get; set; }
        public Nullable<byte> show2 { get; set; }
        public Nullable<byte> show3 { get; set; }
        public Nullable<int> flaage { get; set; }
        public Nullable<bool> canceled { get; set; }
        public Nullable<bool> transfer { get; set; }
        public string RemarkMod { get; set; }
        public long Id { get; set; }
        public Nullable<decimal> Replacement { get; set; }
        public Nullable<decimal> FlageFinish { get; set; }
        public Nullable<bool> Edit { get; set; }
        public Nullable<System.DateTime> MainsFrom_date { get; set; }
        public Nullable<System.DateTime> MainsTo_date { get; set; }
        public Nullable<decimal> del_per { get; set; }
        public Nullable<decimal> del_amount { get; set; }
    }
}
