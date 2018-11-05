using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class GridDashboardEmployeeDTO
    {
        public string JobNo { get; set; }
        public DateTime? jobdate { get; set; }
        public string SystemName { get; set; }
        public string activity { get; set; }
        public string qty_item { get; set; }
        public DateTime? fromtime { get; set; }
        public DateTime? totime { get; set; }
        public string jobsystemId { get; set; }

        public List<joblabworkemp> _joblabworkemplist { get; set; }
    }
}