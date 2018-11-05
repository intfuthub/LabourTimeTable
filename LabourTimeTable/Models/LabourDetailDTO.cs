using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class LabourDetailDTO
    {
        //pagination
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PagerCount { get; set; }

        public ts_user user { get; set; }
        public List<TimesheetGridDTO> _TimesheetGridDTO { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class LabourComplaintDetailDTO
    {
        //pagination
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PagerCount { get; set; }

        public ts_user user { get; set; }
        public List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class LabourOtherDetailDTO
    {
        //pagination
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PagerCount { get; set; }
        public ts_user user { get; set; }
        public List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class LabourOtherDataBindDTO
    {
        public List<enq_mast> _enq_mastList { get; set; }
        public List<ts_employee> _ts_employee { get; set; }
        public List<ac_dept> _ac_deptList { get; set; }

    }

    public class TimesheetENQPostDTO
    {
        public string EnquiryNo { get; set; }
        public string Customer { get; set; }
        public string Department { get; set; }
        public string Purpose { get; set; }
        public string ProjectName { get; set; }
        public List<string> Team { get; set; }
        public DateTime Time { get; set; }
        public location location { get; set; }
    }
}