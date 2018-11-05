using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class TimesheetPostDTO
    {
        public string JobNo { get; set; }
        public string ProjectName { get; set; }
        public List<int> Team { get; set; }
        public string OutsourceCompanyID { get; set; }
        public List<int> OutsourceTeam { get; set; }
        public DateTime Time { get; set; }
        public bool isOutsourcing { get; set; }
        public location location { get; set; }

    }
}


