using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class DashboardCounter
    {

        public string count_attandace { get; set; }
        public string count_absent { get; set; }
        public string count_exception { get; set; }
        public string count_overtime { get; set; }
        public List<ChartDataDTO> _ChartDataDTOList { get; set; }

    }

    public class DashboardAbsent
    {
        public string Firstname { get; set; }
        public DateTime Date { get; set; }
    }
}