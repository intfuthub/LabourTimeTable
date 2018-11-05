using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class SystemTaskDTO
    {
        public List<JobDailyProduction> _JobDailyProductionCBList { get; set; }
        public List<JobDailyProduction> _JobDailyProductionITAList { get; set; }
        public List<JobDailyProduction> _JobDailyProductionTCList { get; set; }
    }

    public class TaskName
    {
        public string TaskNameDesc { get; set; }
    }
}