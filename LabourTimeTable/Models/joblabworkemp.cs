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
    
    public partial class joblabworkemp
    {
        public long Id { get; set; }
        public string doc_no { get; set; }
        public string jobno { get; set; }
        public string jobsystemId { get; set; }
        public Nullable<System.DateTime> jobdate { get; set; }
        public string ac_code { get; set; }
        public Nullable<int> empid { get; set; }
        public string workid { get; set; }
        public string empname { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> fromtime { get; set; }
        public Nullable<System.DateTime> totime { get; set; }
        public string groupid { get; set; }
    }
}