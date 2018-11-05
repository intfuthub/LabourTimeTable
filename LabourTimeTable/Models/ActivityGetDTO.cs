using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class ActivityGetDTO
    {
        public enq_mast _enq_mast { get; set; }
        public List<enq_tran> _enq_tranList { get; set; }
    }
}