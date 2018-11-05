using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class OutsourcePostDTO
    {
        public Nullable<int> OutsourceCompany { get; set; }
        public string EmiratesID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public Nullable<System.DateTime> JoinDate { get; set; }



    }
}