using Chart.Mvc.ComplexChart;
using Chart.Mvc.Extensions;
using Chart.Mvc.SimpleChart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LabourTimeTable.Models
{
    public class ChartDataDTO
    {
        public string job { get; set; }
        public string qty { get; set; }
        public string person { get; set; }
        public string comment { get; set; }
        public string targethours { get; set; }
        public string workedhours { get; set; }
    }
}