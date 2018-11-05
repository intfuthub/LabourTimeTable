using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class TimesheetEnqGridPostDTO
    {
        public string id { get; set; }
        public string enq_no { get; set; }
        public string job_no { get; set; }
        public string department { get; set; }
        public string purpose { get; set; }
        public string checkin { get; set; }
        public string checkout { get; set; }
        public string emp_id { get; set; }
        public string time { get; set; }
        public string cust_name { get; set; }
        public string remarks { get; set; }
        public string empname { get; set; }
        public bool? is_checkout { get; set; }
        public DateTime? date { get; set; }
        public string groupid { get; set; }
    }
}