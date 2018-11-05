using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class ComplainDTO
    {
        //complain
        public string com_no { get; set; }
        public Nullable<System.DateTime> com_date { get; set; }
        public string com_job { get; set; }
        public string ac_code { get; set; }
        public string com_name { get; set; }
        public Nullable<int> com_id { get; set; }
        public string com_narration { get; set; }
        public string com_iss_to { get; set; }
        public string com_type { get; set; }
        public Nullable<long> ac_sub { get; set; }
        public string com_person { get; set; }
        public string com_phone { get; set; }
        public string com_status { get; set; }
        public string enq_no { get; set; }
        public Nullable<System.DateTime> date_done { get; set; }
        public Nullable<long> cust_rep { get; set; }
        public string Sheetno { get; set; }
        public string techproblem { get; set; }
        public string techsolution { get; set; }
        public Nullable<bool> Priority { get; set; }
        public Nullable<byte> doc_level { get; set; }
        public string doc_status { get; set; }
        public string user_name { get; set; }
        public string remarks { get; set; }
        public string Job_name { get; set; }
        public string com_rec_by { get; set; }


        /// <summary>
        /// /complainttype
        /// </summary>
        public string complainttype_ac_name { get; set; }
        public Nullable<bool> complainttype_ac_type { get; set; }

        /// <summary>
        /// /contact
        /// </summary>

        public decimal contact_ac_sub { get; set; }
        public string contact_ac_code { get; set; }
        public string contact_Empname { get; set; }
        public string contact_Ac_name { get; set; }
        public string contact_Designation { get; set; }
        public string contact_Deptname { get; set; }
        public string contact_Influence { get; set; }
        public string contact_Telno { get; set; }
        public string contact_Mobno { get; set; }
        public string contact_Faxno { get; set; }
        public string contact_Email { get; set; }
        public string contact_Voip { get; set; }
        public string contact_Company { get; set; }
        public string contact_jobno { get; set; }

        /// <summary>
        /// /Customer
        /// </summary>

        public string AC_CODE { get; set; }
        public string AC_NAME { get; set; }
        public Nullable<decimal> AC_BAL { get; set; }
        public string AC_ADDRESS { get; set; }
        public string AC_POBOX { get; set; }
        public Nullable<int> AC_CITYID { get; set; }
        public string AC_CITY { get; set; }
        public Nullable<int> AC_COUNTRYID { get; set; }
        public string AC_COUNTRY { get; set; }
        public string AC_TELE1 { get; set; }
        public string AC_TELE2 { get; set; }
        public string AC_MOBNO { get; set; }
        public string AC_FAXNO { get; set; }
        public string AC_CONTACT { get; set; }
        public string AC_EMAIL { get; set; }
        public string AC_WEBSITE { get; set; }
        public Nullable<short> AC_TERMS { get; set; }
        public Nullable<int> AC_TYPE { get; set; }
        public Nullable<int> AC_SMAN { get; set; }
        public Nullable<int> ctype { get; set; }
        public string DISC_PER { get; set; }
        public string SHIP_TO { get; set; }
        public Nullable<bool> Nonactive { get; set; }
        public Nullable<decimal> War_Type { get; set; }
        public string PassExcel { get; set; }
        public string AccCode { get; set; }
        public Nullable<decimal> CostType { get; set; }
        public Nullable<decimal> @bool { get; set; }
        public Nullable<decimal> id { get; set; }

    }

}

