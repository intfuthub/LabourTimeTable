using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class TimesheetGridDTO
    {
        public string id { get; set; }
        public string job_no { get; set; }
        public DateTime? date { get; set; }

        [Column("checkin")]
        [DisplayFormat(DataFormatString = @"{0:hh\\:mm\\}")]
        public string checkin { get; set; }

        [DisplayFormat(DataFormatString = "{0:hh\\:mm\\}", ApplyFormatInEditMode = true)]
        public string checkout { get; set; }
        public Nullable<int> emp_id { get; set; }
        public string Time { get; set; }
        public string empname { get; set; }
        public Nullable<bool> is_checkout { get; set; }
        public string groupid { get; set; }
    }


    public class TimesheetExceptionDTO
    {
        public string id { get; set; }
        public string job_no { get; set; }
        public DateTime? date { get; set; }
        public string checkin { get; set; }
        public string checkout { get; set; }
        public string traveltime { get; set; }
        public Nullable<int> emp_id { get; set; }
        public string Time { get; set; }
        public string empname { get; set; }
        public Nullable<bool> is_checkout { get; set; }
        public string groupid { get; set; }
    }
}