using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class TimesheetComPostDTO
    {
        public string ComplainNo { get; set; }
        public List<string> Team { get; set; }
        public string OutsourceCompanyID { get; set; }
        public List<int> OutsourceTeam { get; set; }
        public DateTime Time { get; set; }
        public bool isOutsourcing { get; set; }
        public location location { get; set; }
    }

    public class TimesheetComGridPostDTO
    {
        public string id { get; set; }
        public string com_no { get; set; }
        public DateTime? date { get; set; }
        public string checkin { get; set; }
        public string checkout { get; set; }
        public string emp_id { get; set; }
        public string time { get; set; }
        public string empname { get; set; }
        public string remarks { get; set; }
        public bool? is_checkout { get; set; }
        public string groupid { get; set; }


    }

    public class location
    {
        public string ts_timesheet_id { get; set; }
        public string latlng { get; set; }
        public string name { get; set; }
        public string formatted_address { get; set; }

    }
}