using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class ProjectDetailsDTO
    {
        public List<enq_mast> _enq_mast { get; set; }
        public List<ts_employee> _employee { get; set; }
        public List<OutSourceDetail> _OutSourceDetail { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }
    }
}