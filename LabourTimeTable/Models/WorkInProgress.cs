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
    
    public partial class WorkInProgress
    {
        public int id { get; set; }
        public string com_no { get; set; }
        public string reason { get; set; }
        public Nullable<int> NoOfAttempts { get; set; }
        public string AttendedDate { get; set; }
    }
}