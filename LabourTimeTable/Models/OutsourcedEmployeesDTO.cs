using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class OutsourcedEmployeesDTO
    {
        public List<ts_employee> _OutsourcedEmployees { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}