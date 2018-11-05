using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class ExceptionModelDTO
    {
        public TimesheetGridDTO _TimesheetGridDTO { get; set; }
        public List<TimesheetGridDTO> _TimesheetGridDTOList { get; set; }
    }
}