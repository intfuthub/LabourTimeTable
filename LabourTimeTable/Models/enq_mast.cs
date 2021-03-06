//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LabourTimeTable.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class enq_mast
    {
        public string doc_no { get; set; }
        public Nullable<System.DateTime> doc_date { get; set; }
        public string doc_rev { get; set; }
        public Nullable<int> custTypeId { get; set; }
        public string ac_code { get; set; }
        public string CustName { get; set; }
        public string owner_code { get; set; }
        public string owner_name { get; set; }
        public Nullable<int> contactId { get; set; }
        public string Inv_Code { get; set; }
        public Nullable<int> reqNatureId { get; set; }
        public string reqNatureName { get; set; }
        public Nullable<bool> tpc_yn { get; set; }
        public string tpc_cust { get; set; }
        public Nullable<decimal> tpc_per { get; set; }
        public Nullable<decimal> tpc_amt { get; set; }
        public Nullable<int> enq_source { get; set; }
        public Nullable<int> enq_rct_by { get; set; }
        public Nullable<int> enq_re { get; set; }
        public Nullable<int> enq_se { get; set; }
        public Nullable<System.DateTime> enq_se_date { get; set; }
        public Nullable<System.DateTime> enq_est_date { get; set; }
        public Nullable<System.DateTime> enq_job_date { get; set; }
        public Nullable<System.DateTime> enq_qot_date { get; set; }
        public Nullable<int> qot_rct_by { get; set; }
        public Nullable<System.DateTime> qot_rct_date { get; set; }
        public string qot_no { get; set; }
        public Nullable<System.DateTime> qot_date { get; set; }
        public Nullable<decimal> qot_amt { get; set; }
        public string remarks1 { get; set; }
        public string remarks2 { get; set; }
        public string job_name { get; set; }
        public string ref_no { get; set; }
        public string PostedBy { get; set; }
        public Nullable<byte> tselect { get; set; }
        public Nullable<byte> enq_level { get; set; }
        public string enq_status { get; set; }
        public string user_name { get; set; }
        public string ord_no { get; set; }
        public Nullable<System.DateTime> ord_date { get; set; }
        public string ext_job { get; set; }
        public string complaintno { get; set; }
        public Nullable<int> reasonId { get; set; }
        public string reason { get; set; }
        public Nullable<byte> job_status { get; set; }
        public Nullable<byte> enq_read { get; set; }
        public Nullable<int> enq_typeId { get; set; }
        public Nullable<decimal> tstkamt { get; set; }
        public string plotno { get; set; }
        public Nullable<int> ProjectTypeId { get; set; }
        public Nullable<byte> enq_level_old { get; set; }
        public string enq_status_old { get; set; }
        public Nullable<decimal> mat_cost { get; set; }
        public Nullable<decimal> lab_cost { get; set; }
        public Nullable<decimal> disc_fix { get; set; }
        public string lpo_no { get; set; }
        public Nullable<int> pay_terms { get; set; }
        public Nullable<byte> Inactive { get; set; }
        public Nullable<int> ColorId { get; set; }
        public Nullable<System.DateTime> jobdecline_date { get; set; }
        public Nullable<System.DateTime> jobopen_date { get; set; }
        public Nullable<System.DateTime> jobhand_date { get; set; }
        public Nullable<System.DateTime> jobclose_date { get; set; }
        public Nullable<System.DateTime> jobwarranty_date { get; set; }
        public string Mod_job { get; set; }
        public Nullable<decimal> Amtpay { get; set; }
        public string AmtpayVout { get; set; }
        public Nullable<double> Credit { get; set; }
        public Nullable<int> EngStatuse { get; set; }
        public Nullable<int> Edit { get; set; }
        public Nullable<System.DateTime> MainsFrom_date { get; set; }
        public Nullable<System.DateTime> MainsTo_date { get; set; }
        public Nullable<System.DateTime> TenderDate { get; set; }
        public string TenderRemark { get; set; }
        public Nullable<int> QPrint { get; set; }
        public string UserQPrint { get; set; }
        public Nullable<int> OpitnQt { get; set; }
        public Nullable<int> EnqExtintion { get; set; }
        public Nullable<System.DateTime> DefectLiabilityDate { get; set; }
    }
}
