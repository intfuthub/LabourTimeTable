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
    
    public partial class ts_employee
    {
        public int empID { get; set; }
        public string ac_code { get; set; }
        public string empname { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public Nullable<System.DateTime> JoinDate { get; set; }
        public Nullable<int> NationId { get; set; }
        public string nationality { get; set; }
        public Nullable<int> jobID { get; set; }
        public Nullable<int> emptypeID { get; set; }
        public string EmptypeName { get; set; }
        public string DesignationId { get; set; }
        public string CompanyId { get; set; }
        public Nullable<int> DeptId { get; set; }
        public string workname { get; set; }
        public string workID { get; set; }
        public Nullable<byte> workType { get; set; }
        public Nullable<int> GradeID { get; set; }
        public Nullable<decimal> salary { get; set; }
        public string starmark { get; set; }
        public Nullable<byte> status { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<byte> jobend { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string FileNo { get; set; }
        public string PassportNo { get; set; }
        public Nullable<System.DateTime> PassportIssueDate { get; set; }
        public Nullable<System.DateTime> PassportExpiryDate { get; set; }
        public string LaborNo { get; set; }
        public Nullable<System.DateTime> LaborIssueDate { get; set; }
        public Nullable<System.DateTime> LaborExpiryDate { get; set; }
        public string VisaNo { get; set; }
        public Nullable<System.DateTime> VisaIssueDate { get; set; }
        public Nullable<System.DateTime> VisaExpiryDate { get; set; }
        public string EmidNo { get; set; }
        public Nullable<System.DateTime> EmidIssueDate { get; set; }
        public Nullable<System.DateTime> EmidExpiryDate { get; set; }
        public string HealthNo { get; set; }
        public Nullable<System.DateTime> HealthIssueDate { get; set; }
        public Nullable<System.DateTime> HealthExpiryDate { get; set; }
        public string DrivingLicNo { get; set; }
        public Nullable<System.DateTime> DrivingLicIssueDate { get; set; }
        public Nullable<System.DateTime> DrivingLicExpiryDate { get; set; }
        public string DrivingLicType { get; set; }
        public Nullable<System.DateTime> LabRegDate { get; set; }
        public Nullable<int> WorkcountryID { get; set; }
        public string WorkAddress1 { get; set; }
        public string WorkAddress2 { get; set; }
        public string WorkCity { get; set; }
        public string WorkState { get; set; }
        public string WorkCountry { get; set; }
        public string WorkPostbox { get; set; }
        public string WorkTelno { get; set; }
        public string WorkMobno { get; set; }
        public Nullable<int> HomecountryID { get; set; }
        public string HomeAddress1 { get; set; }
        public string HomeAddress2 { get; set; }
        public string HomeCity { get; set; }
        public string HomeState { get; set; }
        public string HomeCountry { get; set; }
        public string HomePostBox { get; set; }
        public string HomeTelno { get; set; }
        public string HomeMobno { get; set; }
        public Nullable<decimal> Salary_Basic { get; set; }
        public Nullable<decimal> Salary_Hra { get; set; }
        public Nullable<decimal> Salary_Transport { get; set; }
        public Nullable<decimal> Salary_Other { get; set; }
        public Nullable<bool> Airticket { get; set; }
        public Nullable<bool> BankTransfer { get; set; }
        public Nullable<System.DateTime> LaborRegDate { get; set; }
        public string Bankno { get; set; }
        public string Bankname { get; set; }
        public Nullable<System.DateTime> IncrementDate { get; set; }
        public Nullable<decimal> IncrementAmount { get; set; }
        public string IncrementReason { get; set; }
        public string Remarks { get; set; }
        public string Imagepath { get; set; }
        public Nullable<bool> is_technician { get; set; }
        public Nullable<bool> is_project_manager { get; set; }
        public Nullable<bool> is_outsourced { get; set; }
        public Nullable<int> outsourced_company_id { get; set; }
    }
}