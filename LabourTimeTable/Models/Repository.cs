using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LabourTimeTable.Models;
using System.Globalization;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Data.Objects.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Text;
using Echovoice.JSON;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using Tarczynski.NtpDateTime;
using System.Web.Helpers;

namespace LabourTimeTable.Models
{

    public class Repository
    {
        TimesheetEntities _TimesheetEntities = new TimesheetEntities();
        InterfutureEntities _InterfutureEntities = new InterfutureEntities();

        private static string _PlanTime;
        private static decimal _PlanTime1;
        private static string _Worjtime;
        private static decimal _Worjtime1;
        string Email = "";
        string _com_no = "";
        string _reason = "";

        public async Task< LabourDetailDTO> getLogin(LoginModel _LoginModel)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var yourTime1 = TimeZoneInfo.ConvertTime(currentDate, cet);

            try
            {
                _LabourDetailDTO.user = await Task.Run(()=> _TimesheetEntities.ts_user
                                                          .Where(e => e.username == _LoginModel.UserName && e.password == _LoginModel.Password)
                                                          .FirstOrDefault());
                if (_LabourDetailDTO.user != null)
                {
                    _LabourDetailDTO.Status = true;

                    var query = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                 join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                 join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                 from tu in ps.DefaultIfEmpty()
                                 where ts.user_id == _LabourDetailDTO.user.id
                                 //&& System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime1)
                                 select new TimesheetGridDTO
                                 {
                                     id = ts.id,
                                     job_no = ts.job_no,
                                     checkin = ts.checkin,
                                     checkout = ts.checkout,
                                     emp_id = ts.emp_id,
                                     Time = ts.time,
                                     empname = em.Firstname,
                                     is_checkout = ts.is_checkout,
                                     groupid = ts.groupid

                                 }).OrderByDescending(x => x.checkin).ToList());

                    _LabourDetailDTO._TimesheetGridDTO = query;
                }
                else
                {
                    _LabourDetailDTO.Status = false;
                    _LabourDetailDTO.Message = "Looks like user or password does not match.";
                }
            }
            catch (Exception e)
            {
                _LabourDetailDTO.Message = e.Message.ToString();
            }
            return _LabourDetailDTO;
        }

        public async Task<ProjectDetailsDTO> getProjectDetails(int? EmpId)
        {
            ProjectDetailsDTO _ProjectDetailsDTO = new ProjectDetailsDTO();

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

                List<ts_employee> _employees = new List<ts_employee>();


                _ProjectDetailsDTO._enq_mast = await Task.Run(() => _InterfutureEntities.enq_mast
                                                .Where(x => x.ord_no != null && x.enq_status == "Job Opened")
                                                .OrderByDescending(x => x.doc_date)
                                                .ToList());

                #region

                var rest1 = await Task.Run(() => (from post in _TimesheetEntities.ts_employee
                                                  where (post.workType == 1
                                                          && post.Inactive == false
                                                          && post.jobend == 0
                                                          && post.is_outsourced == false
                                                          && post.empID != EmpId)
                                                  select new
                                                  {
                                                      Post = post

                                                  }).ToList());

                var rest = await Task.Run(() => (from post in _TimesheetEntities.ts_employee
                                                 join meta in _TimesheetEntities.ts_timesheet on post.empID equals meta.emp_id
                                                 where (post.workType == 1
                                                         && post.Inactive == false
                                                         && post.jobend == 0
                                                         && post.is_outsourced == false
                                                         && post.empID != EmpId
                                                         && meta.is_checkout == false
                                                         && System.Data.Entity.DbFunctions.TruncateTime(meta.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime))
                                                 select new
                                                 {
                                                     Post = post

                                                 }).ToList());

                var result = rest1.Except(rest).ToList();


                for (int i = 0; i < result.Count; i++)
                {
                    ts_employee _Employee = new ts_employee();
                    _Employee.empID = result[i].Post.empID;
                    _Employee.empname = result[i].Post.empname;
                    _Employee.Firstname = result[i].Post.Firstname;
                    _Employee.Lastname = result[i].Post.Lastname;

                    _employees.Add(_Employee);
                }

                #endregion

                _ProjectDetailsDTO._employee = _employees;// theexceptList;

                _ProjectDetailsDTO._OutSourceDetail = await Task.Run(() => _InterfutureEntities.OutSourceDetails
                                                                               .GroupBy(a => a.companyName)
                                                                               .Select(g => g.FirstOrDefault())
                                                                               .ToList());

            }
            catch (Exception e)
            {
                _ProjectDetailsDTO.Message = e.Message.ToString();
            }

            return _ProjectDetailsDTO;
        }

        public async Task<ProjectDetailsDTO> getAttandanceEmployeeList(int? EmpId)
        {
            ProjectDetailsDTO _ProjectDetailsDTO = new ProjectDetailsDTO();

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

                List<ts_employee> _employees = new List<ts_employee>();

                #region

                var rest1 = await Task.Run(() => (from post in _TimesheetEntities.ts_employee
                                                  where (post.workType == 1
                                                          && post.Inactive == false
                                                          && post.jobend == 0
                                                          && post.is_outsourced == false
                                                          && post.empID != EmpId)
                                                  select new
                                                  {
                                                      Post = post

                                                  }).ToList());


                for (int i = 0; i < rest1.Count; i++)
                {
                    ts_employee _Employee = new ts_employee();
                    _Employee.empID = rest1[i].Post.empID;
                    _Employee.empname = rest1[i].Post.empname;
                    _Employee.Firstname = rest1[i].Post.Firstname;
                    _Employee.Lastname = rest1[i].Post.Lastname;

                    _employees.Add(_Employee);
                }

                #endregion

                _ProjectDetailsDTO._employee = _employees;// theexceptList;

            }
            catch (Exception e)
            {
                _ProjectDetailsDTO.Message = e.Message.ToString();
            }

            return _ProjectDetailsDTO;
        }

        public OutsourcedEmployeesDTO getOutSourcedEmployees(int outsourced_id)
        {
            OutsourcedEmployeesDTO _OutsourcedEmployeesDTO = new OutsourcedEmployeesDTO();
            try
            {
                _OutsourcedEmployeesDTO._OutsourcedEmployees = _TimesheetEntities.ts_employee
                                                .Where(x => x.workType == 1 && x.Inactive == false && x.jobend == 0 && x.is_outsourced == true && x.outsourced_company_id == outsourced_id)
                                                .OrderBy(x => x.empname)
                                                .ToList();

            }
            catch (Exception e)
            {
                _OutsourcedEmployeesDTO.Message = e.Message.ToString();
            }

            return _OutsourcedEmployeesDTO;
        }

        public List<TimesheetGridDTO> setTimesheetData(TimesheetPostDTO _TimesheetPostDTO, ts_user _ts_user)
        {
            List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();

            try
            {
                string _groupId = Guid.NewGuid().ToString();

                //interfuture employee
                #region  interfuture employee
                if (_TimesheetPostDTO.Team.Count > 0)
                {
                    for (int i = 0; i < _TimesheetPostDTO.Team.Count; i++)
                    {
                        if (_TimesheetPostDTO.Team[i] != 0)
                        {
                            ts_timesheet _ts_timesheet = new ts_timesheet();
                            _ts_timesheet.id = Guid.NewGuid().ToString();
                            _ts_timesheet.user_id = _ts_user.id;
                            _ts_timesheet.emp_id = _TimesheetPostDTO.Team[i];
                            _ts_timesheet.job_no = _TimesheetPostDTO.JobNo;
                            _ts_timesheet.project_name = _TimesheetPostDTO.ProjectName;
                            _ts_timesheet.checkin = _TimesheetPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                            //DateTime _checkout = Convert.ToDateTime("05:00 PM");
                            _ts_timesheet.checkout = null;// _checkout.ToString("HH:mm tt", CultureInfo.InvariantCulture);
                            _ts_timesheet.time = null;
                            _ts_timesheet.place = null;
                            _ts_timesheet.is_checkout = false;
                            _ts_timesheet.is_project = true;

                            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                            var yourTime1 = TimeZoneInfo.ConvertTime(currentDate, cet);

                            _ts_timesheet.ondate_created = yourTime1;
                            _ts_timesheet.createdby = _ts_user.name;
                            _ts_timesheet.groupid = _groupId;

                            _TimesheetEntities.ts_timesheet.Add(_ts_timesheet);
                            _TimesheetEntities.SaveChanges();
                        }
                    }
                }
                #endregion  interfuture employee

                //outsourced team
                #region outsourced team
                if (_TimesheetPostDTO.OutsourceTeam != null)
                {
                    for (int i = 0; i < _TimesheetPostDTO.OutsourceTeam.Count; i++)
                    {
                        ts_timesheet _ts_timesheet = new ts_timesheet();
                        _ts_timesheet.id = Guid.NewGuid().ToString();
                        _ts_timesheet.user_id = _ts_user.id;
                        _ts_timesheet.emp_id = _TimesheetPostDTO.OutsourceTeam[i];
                        _ts_timesheet.job_no = _TimesheetPostDTO.JobNo;
                        _ts_timesheet.project_name = _TimesheetPostDTO.ProjectName;
                        _ts_timesheet.checkin = _TimesheetPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                        //DateTime _checkout = Convert.ToDateTime("05:00 PM");
                        _ts_timesheet.checkout = null;// _checkout.ToString("HH:mm tt", CultureInfo.InvariantCulture);
                        _ts_timesheet.time = null;
                        _ts_timesheet.place = null;
                        _ts_timesheet.is_project = true;

                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                        DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                        var yourTime1 = TimeZoneInfo.ConvertTime(currentDate, cet);

                        _ts_timesheet.ondate_created = yourTime1;
                        _ts_timesheet.createdby = _ts_user.name;
                        _ts_timesheet.is_outsourced = true;
                        _ts_timesheet.is_checkout = false;

                        _ts_timesheet.outsourced_company_id = _TimesheetPostDTO.OutsourceCompanyID;
                        _ts_timesheet.groupid = _groupId;

                        _TimesheetEntities.ts_timesheet.Add(_ts_timesheet);
                        _TimesheetEntities.SaveChanges();
                    }
                }
                #endregion outsourced team

                //login user
                #region login user
                ts_timesheet _ts_timesheetlogin = new ts_timesheet();
                _ts_timesheetlogin.id = Guid.NewGuid().ToString();
                _ts_timesheetlogin.user_id = _ts_user.id;
                _ts_timesheetlogin.emp_id = _ts_user.emp_id;
                _ts_timesheetlogin.job_no = _TimesheetPostDTO.JobNo;
                _ts_timesheetlogin.project_name = _TimesheetPostDTO.ProjectName;
                _ts_timesheetlogin.checkin = _TimesheetPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                //DateTime _xcheckout = Convert.ToDateTime("05:00 PM");
                _ts_timesheetlogin.checkout = null;// _xcheckout.ToString("HH:mm tt", CultureInfo.InvariantCulture);
                _ts_timesheetlogin.time = null;
                _ts_timesheetlogin.place = null;
                _ts_timesheetlogin.is_checkout = false;
                _ts_timesheetlogin.is_project = true;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

                _ts_timesheetlogin.ondate_created = swedishTime;
                _ts_timesheetlogin.createdby = _ts_user.name;
                _ts_timesheetlogin.groupid = _groupId;

                _TimesheetEntities.ts_timesheet.Add(_ts_timesheetlogin);
                _TimesheetEntities.SaveChanges();

                #endregion login user

                //location
                ts_place _ts_place = new ts_place();
                _ts_place.ts_timesheet_groupid = _groupId;
                _ts_place.latlang = _TimesheetPostDTO.location.latlng;
                _ts_place.name = _TimesheetPostDTO.location.name;
                _ts_place.formatted_address = _TimesheetPostDTO.location.formatted_address;

                _TimesheetEntities.ts_place.Add(_ts_place);
                _TimesheetEntities.SaveChanges();

                //Return Value
                #region ReturnValue

                var query = (from ts in _TimesheetEntities.ts_timesheet
                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                             from tu in ps.DefaultIfEmpty()
                             where ts.user_id == _ts_user.id
                             && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                             && ts.is_project == true
                             select new TimesheetGridDTO
                             {
                                 id = ts.id,
                                 job_no = ts.job_no,
                                 checkin = ts.checkin,
                                 checkout = ts.checkout,
                                 emp_id = ts.emp_id,
                                 Time = ts.time,
                                 empname = em.Firstname,
                                 is_checkout = ts.is_checkout,
                                 groupid = ts.groupid

                             }).OrderByDescending(x => x.checkin).ToList();
                #endregion ReturnValue

                _TimesheetGridDTO = query;
            }
            catch (Exception e)
            {
                ////throw e;
                //_TimesheetGridDTO[0].empname = e.ToString();
            }

            return _TimesheetGridDTO;
        }

        public bool saveOutsourcedEmployee(OutsourcePostDTO _OutsourcePostDTO)
        {
            using (var db = new TimesheetEntities())
            {
                try
                {
                    ts_employee _ts_employee = new ts_employee();

                    _ts_employee.empID = GenerateRandomNo();
                    _ts_employee.empname = _OutsourcePostDTO.FirstName + " " + _OutsourcePostDTO.LastName;
                    _ts_employee.workType = 1;
                    _ts_employee.Inactive = false;
                    _ts_employee.jobend = 0;
                    _ts_employee.is_technician = false;
                    _ts_employee.is_project_manager = false;
                    _ts_employee.Firstname = _OutsourcePostDTO.FirstName;
                    _ts_employee.Middlename = _OutsourcePostDTO.MiddleName;
                    _ts_employee.Lastname = _OutsourcePostDTO.LastName;
                    _ts_employee.DesignationId = _OutsourcePostDTO.Designation;
                    _ts_employee.JoinDate = _OutsourcePostDTO.JoinDate;
                    _ts_employee.is_outsourced = true;
                    _ts_employee.outsourced_company_id = _OutsourcePostDTO.OutsourceCompany;

                    _TimesheetEntities.ts_employee.Add(_ts_employee);
                    _TimesheetEntities.SaveChanges();

                    ts_emirates_id _ts_emirates_id = new ts_emirates_id();
                    _ts_emirates_id.emirates_id = _OutsourcePostDTO.EmiratesID;
                    _ts_emirates_id.empID = _ts_employee.empID.ToString();
                    _ts_emirates_id.empname = _ts_employee.empname;

                    _TimesheetEntities.ts_emirates_id.Add(_ts_emirates_id);
                    _TimesheetEntities.SaveChanges();

                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception("There was a problem saving this record: " + e.Message);
                }
            }

        }

        public ts_emirates_id CheckIfLaborWithEmiratesIDExist(string emirates_id)
        {
            ts_emirates_id _ts_emirates_id = new ts_emirates_id();
            try
            {
                _ts_emirates_id = _TimesheetEntities.ts_emirates_id
                                    .Where(x => x.emirates_id.Trim() == emirates_id.Trim()).FirstOrDefault();
            }
            catch (Exception e)
            {

            }

            return _ts_emirates_id;
        }

        public async Task<List<TimesheetGridDTO>> getTimesheetData(string user_id)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();
            var query = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                              join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                              join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                              from tu in ps.DefaultIfEmpty()
                                              where ts.user_id == user_id && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                              && ts.is_project == true
                                              select new TimesheetGridDTO
                                              {
                                                  id = ts.id,
                                                  job_no = ts.job_no,
                                                  checkin = ts.checkin,
                                                  checkout = ts.checkout,
                                                  emp_id = ts.emp_id,
                                                  Time = ts.time,
                                                  empname = em.Firstname,
                                                  is_checkout = ts.is_checkout,
                                                  groupid = ts.groupid

                                              }).OrderByDescending(x => x.checkin).ToList());

            return _TimesheetGridDTO = query;
        }

        public List<ts_timesheet> getOutSourceEmployeePerGroup(string user_id, string groupid)
        {
            List<ts_timesheet> _ts_timesheetList = new List<ts_timesheet>();
            _ts_timesheetList = _TimesheetEntities.ts_timesheet.Where(x => x.user_id == user_id && x.groupid == groupid && x.is_outsourced == true).ToList();

            return _ts_timesheetList;
        }

        public List<ts_timesheet> getEmployeePerGroup(string user_id, string groupid)
        {
            List<ts_timesheet> _ts_timesheetList = new List<ts_timesheet>();
            _ts_timesheetList = _TimesheetEntities.ts_timesheet.Where(x => x.user_id == user_id && x.groupid == groupid).ToList();

            return _ts_timesheetList;
        }

        public List<TimesheetGridDTO> AllCheckOut(ts_user _ts_user, string endtime)
        {
            List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

            try
            {
                var eQuery = (from ts in _TimesheetEntities.ts_timesheet
                              where ts.user_id == _ts_user.id
                              && ts.is_checkout == false
                              && ts.is_project == true
                              && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                              select new { ts }).ToList();

                foreach (var value in eQuery)
                {
                    TimeSpan duration = DateTime.Parse(endtime).Subtract(DateTime.Parse(value.ts.checkin));
                    value.ts.is_checkout = true;
                    value.ts.checkout = endtime;
                    value.ts.time = duration.ToString(@"hh\:mm");
                    _TimesheetEntities.SaveChanges();
                }

                var query = (from ts in _TimesheetEntities.ts_timesheet
                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                             from tu in ps.DefaultIfEmpty()
                             where ts.user_id == _ts_user.id && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                             select new TimesheetGridDTO
                             {
                                 id = ts.id,
                                 job_no = ts.job_no,
                                 checkin = ts.checkin,
                                 checkout = ts.checkout,
                                 emp_id = ts.emp_id,
                                 Time = ts.time,
                                 empname = em.Firstname,
                                 is_checkout = ts.is_checkout,
                                 groupid = ts.groupid

                             }).OrderByDescending(x => x.checkin).ToList();

                _TimesheetGridDTO = query;

            }
            catch (Exception e)
            {

            }

            return _TimesheetGridDTO;
        }

        public List<TimesheetGridDTO> IndividualCheckOut(ts_user _ts_user, int? emp_id, string endtime)
        {
            List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

            try
            {
                var eQuery = (from ts in _TimesheetEntities.ts_timesheet
                              where ts.emp_id == emp_id
                              && ts.is_checkout == false
                              && ts.is_project == true
                              && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                              select new { ts }).FirstOrDefault();


                if (eQuery != null)
                {
                    TimeSpan duration = DateTime.Parse(endtime).Subtract(DateTime.Parse(eQuery.ts.checkin));

                    eQuery.ts.is_checkout = true;
                    eQuery.ts.checkout = endtime;
                    eQuery.ts.time = duration.ToString(@"hh\:mm");

                    _TimesheetEntities.SaveChanges();
                }

                var query = (from ts in _TimesheetEntities.ts_timesheet
                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                             from tu in ps.DefaultIfEmpty()
                             where ts.user_id == _ts_user.id && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                             select new TimesheetGridDTO
                             {
                                 id = ts.id,
                                 job_no = ts.job_no,
                                 checkin = ts.checkin,
                                 checkout = ts.checkout,
                                 emp_id = ts.emp_id,
                                 Time = ts.time,
                                 empname = em.Firstname,
                                 is_checkout = ts.is_checkout,
                                 groupid = ts.groupid

                             }).ToList();

                _TimesheetGridDTO = query;

            }
            catch (Exception e)
            {

            }

            return _TimesheetGridDTO;
        }

        public List<ts_timesheet> CheckUserIsAdmin(string userid)
        {
            List<ts_timesheet> _ts_timesheetList = new List<ts_timesheet>();
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

                _ts_timesheetList = _TimesheetEntities.ts_timesheet
                                    .Where(x => x.user_id == userid
                                                        && x.is_checkout == false
                                                        && System.Data.Entity.DbFunctions.TruncateTime(x.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)).ToList();
            }
            catch (Exception e)
            {

            }

            return _ts_timesheetList;
        }

        public async Task<ActivityGetDTO> getActivityDetails(string JobNo)
        {
            ActivityGetDTO _ActivityGetDTO = new ActivityGetDTO();

            _ActivityGetDTO._enq_mast = await Task.Run(() => _InterfutureEntities.enq_mast.Where(e => e.ord_no == JobNo).FirstOrDefault());
            if (_ActivityGetDTO._enq_mast != null)
            {
                _ActivityGetDTO._enq_tranList = await Task.Run(() => _InterfutureEntities.enq_tran.Where(e => e.doc_no == _ActivityGetDTO._enq_mast.doc_no).ToList());
            }

            return _ActivityGetDTO;
        }

        public async Task<List<JobDailyProduction>> GoLabCost(string _doc_no, string _jobno, string _jobsystemId, DateTime? _jobdate, string _groupid, DateTime _FromTime, DateTime _ToTime)
        {
            List<JobDailyProduction> _JobDailyProductionList = new List<JobDailyProduction>();
            OutSourceDetail objOsD = new OutSourceDetail();

            InterfutureEntities db = new InterfutureEntities();
            TimesheetEntities dbx = new TimesheetEntities();


            //_JobDailyProductionList = await Task.Run(() => db.JobDailyProductions
            //                                               .Where(x => x.jobno == _jobno
            //                                                && x.jobsystemId == _jobsystemId)
            //                                                .OrderBy(x => x.Id).ToList());


            try
            {

                var xResult = dbx.ts_timesheet.Where(x => x.groupid == _groupid).ToList();

                for (int i = 0; i < xResult.Count; i++)
                {
                    int? _empID = xResult[i].emp_id;

                    ts_employee objEmp = (from q in dbx.ts_employee
                                          where q.empID == _empID
                                          select q).FirstOrDefault();

                    try
                    {
                        var _xgroupid = _groupid;
                        var _xo = db.empTmps.Where(p => p.groupId == _xgroupid && p.EmpId == _empID).ToList();

                        if (_xo.Count == 0)
                        {
                            empTmp objEmpTmp = new empTmp();
                            objEmpTmp.EmpId = Convert.ToInt32(_empID);
                            objEmpTmp.EmpName = objEmp.empname;
                            objEmpTmp.groupId = _groupid;
                            db.empTmps.Add(objEmpTmp);
                            db.SaveChanges();
                        }
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                }

                UpdateEmpDetails(_doc_no, _jobno, _jobdate, _jobsystemId, _ToTime, _FromTime, _groupid);


                #region

                int u = 0;
                string SystemId = _jobsystemId;
                //////////////////////Daily Production Table///////////
                var objdp = (from o in db.JobDailyProductions
                             where o.jobsystemId == SystemId && o.jobno == _jobno
                             select o).FirstOrDefault();

                ////////////////////Lab Work////////////////////////////
                var objlw = from lw in db.joblabworks
                            where lw.jobno == _jobno
                            where lw.jobsystemId == SystemId
                            where lw.jobdate == _jobdate
                            select lw;
                //////////////////////Lab Cost///////////////////////////
                List<joblabcost> objlc = (from lc in db.joblabcosts
                                          where lc.doc_no == _doc_no
                                          where lc.jobsystemId == SystemId
                                          orderby lc.Id
                                          select lc).ToList();


                List<JobDailyProduction> lstJobDP = (from q in db.JobDailyProductions
                                                     where q.jobno == _jobno && q.jobsystemId == SystemId
                                                     select q).ToList();

                if (lstJobDP.Count > 0)
                {
                    foreach (var shift in lstJobDP)
                    {
                        db.Entry(shift).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                    }
                }



                foreach (var l in objlc)
                {
                    string _joblabcostId = l.Id.ToString();
                    joblabwork olw = (from ol in db.joblabworks
                                      where ol.jobsystemId == SystemId && ol.joblabcostId == _joblabcostId
                                      select ol).FirstOrDefault();
                    var qDone = (from q in db.joblabworks
                                 where q.jobsystemId == l.jobsystemId && q.joblabcostId == _joblabcostId
                                 select q.qty_item).Sum();
                    decimal qD = 0;
                    if (qDone != null)
                    {
                        qD = Convert.ToDecimal(qDone);
                    }
                    try
                    {
                        #region

                        if ((l.jobsystemId.Substring(0, 2) == "13" || l.jobsystemId.Substring(0, 2) == "16" || l.jobsystemId.Substring(0, 2) == "15" || l.jobsystemId.Substring(0, 2) == "17" || l.jobsystemId.Substring(0, 2) == "20") && (l.taskname == "Cable Pulling" || l.taskname == "Point to Junction Box"))
                        {
                            string joblabcostIdA = l.Id.ToString() + "A";
                            string joblabcostIdB = l.Id.ToString() + "B";
                            string _xjoblabcostId = l.Id.ToString();
                            var Q1 = (from q in db.joblabworks
                                      where q.joblabcostId == _xjoblabcostId
                                      select q.qty_item).Sum();
                            var Q1A = (from q in db.joblabworks
                                       where q.joblabcostId == joblabcostIdA
                                       select q.qty_item).Sum();
                            var Q1B = (from q in db.joblabworks
                                       where q.joblabcostId == joblabcostIdB
                                       select q.qty_item).Sum();
                            decimal Q = 0, QA = 0, QB = 0;
                            if (Q1 != null)
                            {
                                Q = Convert.ToDecimal(Q1);
                            }
                            if (Q1A != null)
                            {
                                QA = Convert.ToDecimal(Q1A);
                            }
                            if (Q1B != null)
                            {
                                QB = Convert.ToDecimal(Q1B);
                            }
                            decimal diff = Q - ((QA / 2) + (QB / 2));
                            if (l.jobsystemId.Substring(0, 2) == "13" && l.taskname == "Point to Junction Box")
                            {

                                int cnt = 0;
                                while (cnt < 3)
                                {
                                    JobDailyProduction objjobDP = new JobDailyProduction();
                                    objjobDP.doc_no = _doc_no;
                                    objjobDP.jobno = _jobno;
                                    objjobDP.jobsystemId = SystemId;
                                    objjobDP.empcode = l.empcode;
                                    objjobDP.qty_man = l.qty_man;
                                    objjobDP.qty_item = l.qty_item;
                                    objjobDP.unit = l.unit;
                                    objjobDP.time_min = l.time_min;
                                    objjobDP.factor = l.factor;
                                    objjobDP.tot_time_hour = l.tot_time_hour;
                                    objjobDP.hr_cost = l.hr_cost;
                                    objjobDP.tot_cost = l.tot_cost;
                                    objjobDP.blanklines = l.blanklines;
                                    objjobDP.srno = l.srno;
                                    objjobDP.flageTrans = l.flageTrans;
                                    objjobDP.flagecancel = l.flagecancel;
                                    objjobDP.flagereplese = l.flagereplese;
                                    objjobDP.flageEdit = l.flageEdit;
                                    objjobDP.ondate = DateTime.Now;
                                    objjobDP.groupid = _groupid;

                                    if (olw != null)
                                    {

                                        objjobDP.QtyDone = qD;////from labwork qty-item is stored in Qty Done field in daily production table                    
                                        objjobDP.Updatedate = olw.jobdate;
                                        objjobDP.Notes = olw.Notes;

                                        if (olw.fromtime.ToString().Contains("99"))
                                        {
                                            objjobDP.FromTime = olw.jobdate;
                                            /////update time in labwork table
                                            olw.fromtime = olw.jobdate;
                                        }
                                        else
                                            objjobDP.FromTime = olw.fromtime;

                                        if (olw.totime.ToString().Contains("99"))
                                        {
                                            objjobDP.ToTime = olw.jobdate;
                                            /////update timein labwork table
                                            olw.totime = olw.jobdate;
                                        }
                                        else
                                            objjobDP.ToTime = olw.totime;

                                        objjobDP.flageFix = olw.JobCode;
                                    }

                                    objjobDP.Jobcode = l.Jobcode;
                                    objjobDP.job_per = Convert.ToDecimal(l.job_per);
                                    objjobDP.weight_per = l.weight_per;
                                    if (cnt == 0)
                                    {
                                        objjobDP.QtyDone = Q;
                                        objjobDP.joblabcostId = l.Id.ToString();
                                        objjobDP.taskname = l.taskname;

                                        db.JobDailyProductions.Add(objjobDP);
                                        db.SaveChanges();
                                    }
                                    else if (cnt == 1)
                                    {
                                        DateTime? FrmTime = GetFrmTime(_FromTime);
                                        DateTime? ToTime = GetToTime(_ToTime);

                                        decimal qtyDone = Q + (QA / 2) - (QB / 2);
                                        objjobDP.QtyDone = qtyDone;
                                        objjobDP.joblabcostId = joblabcostIdA;
                                        joblabwork objJLWRK = (from q in db.joblabworks
                                                               where q.joblabcostId == joblabcostIdA && q.jobdate == _jobdate && q.fromtime == FrmTime && q.totime == ToTime
                                                               select q).SingleOrDefault();
                                        decimal qtyA = 0;
                                        if (objJLWRK != null)
                                        {
                                            qtyA = Convert.ToDecimal(objJLWRK.qty_item);
                                        }
                                        objjobDP.Qty = qtyA;
                                        objjobDP.taskname = "* From Point to JunctionBox";

                                        db.JobDailyProductions.Add(objjobDP);
                                        db.SaveChanges();
                                    }
                                    else if (cnt == 2)
                                    {
                                        DateTime? FrmTime = GetFrmTime(_FromTime);
                                        DateTime? ToTime = GetToTime(_ToTime);

                                        decimal qtyDone = Q + (QB / 2) - (QA / 2);
                                        objjobDP.QtyDone = qtyDone;
                                        objjobDP.joblabcostId = joblabcostIdB;
                                        joblabwork objJLWRK = (from q in db.joblabworks
                                                               where q.joblabcostId == joblabcostIdB && q.jobdate == _jobdate && q.fromtime == FrmTime && q.totime == ToTime
                                                               select q).SingleOrDefault();
                                        decimal qtyB = 0;
                                        if (objJLWRK != null)
                                        {
                                            qtyB = Convert.ToDecimal(objJLWRK.qty_item);
                                        }
                                        objjobDP.Qty = qtyB;
                                        objjobDP.taskname = "* From JunctionBox to Main Room";
                                        db.JobDailyProductions.Add(objjobDP);
                                        db.SaveChanges();
                                    }
                                    cnt++;
                                }

                            }
                            else if (l.jobsystemId.Substring(0, 2) != "13" && l.taskname == "Cable Pulling")
                            {
                                int cnt = 0;
                                while (cnt < 3)
                                {
                                    JobDailyProduction objjobDP = new JobDailyProduction();
                                    objjobDP.doc_no = _doc_no;
                                    objjobDP.jobno = _jobno;
                                    objjobDP.jobsystemId = SystemId;
                                    objjobDP.empcode = l.empcode;
                                    objjobDP.qty_man = l.qty_man;
                                    objjobDP.qty_item = l.qty_item;
                                    objjobDP.unit = l.unit;
                                    objjobDP.time_min = l.time_min;
                                    objjobDP.factor = l.factor;
                                    objjobDP.tot_time_hour = l.tot_time_hour;
                                    objjobDP.hr_cost = l.hr_cost;
                                    objjobDP.tot_cost = l.tot_cost;
                                    objjobDP.blanklines = l.blanklines;
                                    objjobDP.srno = l.srno;
                                    objjobDP.flageTrans = l.flageTrans;
                                    objjobDP.flagecancel = l.flagecancel;
                                    objjobDP.flagereplese = l.flagereplese;
                                    objjobDP.flageEdit = l.flageEdit;
                                    objjobDP.ondate = DateTime.Now;
                                    objjobDP.groupid = _groupid;

                                    if (olw != null)
                                    {

                                        objjobDP.QtyDone = olw.qty_item;////from labwork qty-item is stored in Qty Done field in daily production table                    
                                        objjobDP.Updatedate = olw.jobdate;
                                        objjobDP.Notes = olw.Notes;

                                        if (olw.fromtime.ToString().Contains("99"))
                                        {
                                            objjobDP.FromTime = olw.jobdate;
                                            /////update time in labwork table
                                            olw.fromtime = olw.jobdate;
                                        }
                                        else
                                            objjobDP.FromTime = olw.fromtime;

                                        if (olw.totime.ToString().Contains("99"))
                                        {
                                            objjobDP.ToTime = olw.jobdate;
                                            /////update time in labwork table
                                            olw.totime = olw.jobdate;
                                        }
                                        else
                                            objjobDP.ToTime = olw.totime;

                                        objjobDP.flageFix = olw.JobCode;
                                    }
                                    objjobDP.Jobcode = l.Jobcode;
                                    objjobDP.job_per = Convert.ToDecimal(l.job_per);
                                    objjobDP.weight_per = l.weight_per;
                                    if (cnt == 0)
                                    {
                                        objjobDP.QtyDone = Q;
                                        objjobDP.joblabcostId = l.Id.ToString();
                                        objjobDP.taskname = l.taskname;
                                        db.JobDailyProductions.Add(objjobDP);
                                        db.SaveChanges();
                                    }
                                    else if (cnt == 1)
                                    {
                                        DateTime? FrmTime = GetFrmTime(_FromTime);
                                        DateTime? ToTime = GetToTime(_ToTime);

                                        decimal qtyDone = Q + (QA / 2) - (QB / 2);
                                        objjobDP.QtyDone = qtyDone;
                                        objjobDP.joblabcostId = joblabcostIdA;
                                        joblabwork objJLWRK = (from q in db.joblabworks
                                                               where q.joblabcostId == joblabcostIdA && q.jobdate == _jobdate && q.fromtime == FrmTime && q.totime == ToTime
                                                               select q).SingleOrDefault();
                                        decimal qtyA = 0;
                                        if (objJLWRK != null)
                                        {
                                            qtyA = Convert.ToDecimal(objJLWRK.qty_item);
                                        }
                                        objjobDP.Qty = qtyA;
                                        objjobDP.taskname = "* From Point to JunctionBox";
                                        db.JobDailyProductions.Add(objjobDP);
                                        db.SaveChanges();
                                    }
                                    else if (cnt == 2)
                                    {
                                        DateTime? FrmTime = GetFrmTime(_FromTime);
                                        DateTime? ToTime = GetToTime(_ToTime);

                                        decimal qtyDone = Q + (QB / 2) - (QA / 2);
                                        objjobDP.QtyDone = qtyDone;
                                        objjobDP.joblabcostId = joblabcostIdB;
                                        objjobDP.joblabcostId = joblabcostIdB;
                                        joblabwork objJLWRK = (from q in db.joblabworks
                                                               where q.joblabcostId == joblabcostIdB && q.jobdate == _jobdate && q.fromtime == FrmTime && q.totime == ToTime
                                                               select q).SingleOrDefault();
                                        decimal qtyB = 0;
                                        if (objJLWRK != null)
                                        {
                                            qtyB = Convert.ToDecimal(objJLWRK.qty_item);
                                        }
                                        objjobDP.Qty = qtyB;
                                        objjobDP.taskname = "* From JunctionBox to Main Room";
                                        db.JobDailyProductions.Add(objjobDP);
                                        db.SaveChanges();
                                    }
                                    cnt++;
                                }
                            }
                            else
                            {
                                JobDailyProduction objjobDP = new JobDailyProduction();
                                objjobDP.doc_no = _doc_no;
                                objjobDP.jobno = _jobno;
                                objjobDP.jobsystemId = SystemId;
                                objjobDP.joblabcostId = l.Id.ToString();

                                if (olw != null)
                                {

                                    objjobDP.QtyDone = qD;////from labwork qty-item is stored in Qty Done field in daily production table                    
                                    objjobDP.Updatedate = olw.jobdate;
                                    objjobDP.Notes = olw.Notes;

                                    if (olw.fromtime.ToString().Contains("99"))
                                    {
                                        objjobDP.FromTime = olw.jobdate;
                                        /////update time in labwork table
                                        olw.fromtime = olw.jobdate;
                                    }
                                    else
                                        objjobDP.FromTime = olw.fromtime;

                                    if (olw.totime.ToString().Contains("99"))
                                    {
                                        objjobDP.ToTime = olw.jobdate;
                                        /////update time in labwork table
                                        olw.totime = olw.jobdate;
                                    }
                                    else
                                        objjobDP.ToTime = olw.totime;

                                    objjobDP.flageFix = olw.JobCode;
                                }
                                objjobDP.Jobcode = l.Jobcode;
                                objjobDP.job_per = Convert.ToDecimal(l.job_per);
                                objjobDP.weight_per = l.weight_per;
                                objjobDP.taskname = l.taskname;
                                objjobDP.empcode = l.empcode;
                                objjobDP.qty_man = l.qty_man;
                                objjobDP.qty_item = l.qty_item;
                                objjobDP.unit = l.unit;
                                objjobDP.time_min = l.time_min;
                                objjobDP.factor = l.factor;
                                objjobDP.tot_time_hour = l.tot_time_hour;
                                objjobDP.hr_cost = l.hr_cost;
                                objjobDP.tot_cost = l.tot_cost;
                                objjobDP.blanklines = l.blanklines;
                                objjobDP.srno = l.srno;
                                objjobDP.flageTrans = l.flageTrans;
                                objjobDP.flagecancel = l.flagecancel;
                                objjobDP.flagereplese = l.flagereplese;
                                objjobDP.flageEdit = l.flageEdit;
                                objjobDP.ondate = DateTime.Now;
                                objjobDP.groupid = _groupid;

                                ///////////////////////////////////////////
                                db.JobDailyProductions.Add(objjobDP);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            JobDailyProduction objjobDP = new JobDailyProduction();
                            objjobDP.doc_no = _doc_no;
                            objjobDP.jobno = _jobno;
                            objjobDP.jobsystemId = SystemId;
                            objjobDP.joblabcostId = l.Id.ToString();

                            if (olw != null)
                            {
                                objjobDP.QtyDone = qD;////from labwork qty-item is stored in Qty Done field in daily production table                    
                                objjobDP.Updatedate = olw.jobdate;
                                objjobDP.Notes = olw.Notes;

                                if (olw.fromtime.ToString().Contains("99"))
                                {
                                    objjobDP.FromTime = olw.jobdate;
                                    /////update time in labwork table
                                    olw.fromtime = olw.jobdate;
                                }
                                else
                                    objjobDP.FromTime = olw.fromtime;

                                if (olw.totime.ToString().Contains("99"))
                                {
                                    objjobDP.ToTime = olw.jobdate;
                                    /////update time in labwork table
                                    olw.totime = olw.jobdate;
                                }
                                else
                                    objjobDP.ToTime = olw.totime;

                                objjobDP.flageFix = olw.JobCode;
                            }

                            objjobDP.Jobcode = l.Jobcode;
                            objjobDP.job_per = Convert.ToDecimal(l.job_per);
                            objjobDP.weight_per = l.weight_per;
                            objjobDP.taskname = l.taskname;
                            objjobDP.empcode = l.empcode;
                            objjobDP.qty_man = l.qty_man;
                            objjobDP.qty_item = l.qty_item;
                            objjobDP.unit = l.unit;
                            objjobDP.time_min = l.time_min;
                            objjobDP.factor = l.factor;
                            objjobDP.tot_time_hour = l.tot_time_hour;
                            objjobDP.hr_cost = l.hr_cost;
                            objjobDP.tot_cost = l.tot_cost;
                            objjobDP.blanklines = l.blanklines;
                            objjobDP.srno = l.srno;
                            objjobDP.flageTrans = l.flageTrans;
                            objjobDP.flagecancel = l.flagecancel;
                            objjobDP.flagereplese = l.flagereplese;
                            objjobDP.flageEdit = l.flageEdit;
                            objjobDP.ondate = DateTime.Now;
                            objjobDP.groupid = _groupid;

                            ///////////////////////////////////////////
                            //db.JobDailyProductions.InsertOnSubmit(objjobDP);
                            //db.SubmitChanges();

                            db.JobDailyProductions.Add(objjobDP);
                            db.SaveChanges();
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

                DateTime dt = yourTime;
                string DateSelected = dt.ToString("yyyy-MM-dd");
                DateTime _DateSelected = Convert.ToDateTime(DateSelected);

                DateTime? xFrmTime = GetFrmTime(_FromTime);
                DateTime? xToTime = GetToTime(_ToTime);

                var xolw = (from lw in db.joblabworks
                            where lw.jobno == _jobno && lw.jobdate == _DateSelected
                            && lw.jobsystemId == _jobsystemId != null && lw.fromtime == xFrmTime && lw.totime == xToTime
                            select lw).FirstOrDefault();

                if (xolw != null)
                {
                    //txtComments.Text = xolw.Notes;
                }


                ValidationMod(_jobno, _jobsystemId);
                _JobDailyProductionList = await Task.Run(() => db.JobDailyProductions
                                                    .Where(x => x.jobno == _jobno
                                                     && x.jobsystemId == _jobsystemId
                                                     && System.Data.Entity.DbFunctions.TruncateTime(x.ondate) == System.Data.Entity.DbFunctions.TruncateTime(yourTime))
                                                     .OrderBy(x => x.Id).ToList());
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _JobDailyProductionList;
        }

        protected void CheckCommentsOnly(DateTime _jobdate, string _jobno, string _jobsystemId, DateTime _ToTime, DateTime _FromTime)
        {
            InterfutureEntities db = new InterfutureEntities();
            DateTime dt = Convert.ToDateTime(_jobdate);
            string DateSelected = dt.ToString("yyyy-MM-dd");

            DateTime _DateSelected = Convert.ToDateTime(DateSelected);
            DateTime? _FrmTime = GetFrmTime(_FromTime);
            DateTime? _xToTime = GetToTime(_ToTime);



            List<joblabwork> lstJlabWrk = (from q in db.joblabworks
                                           where q.jobno == _jobno && q.jobsystemId == _jobsystemId && q.jobdate == _DateSelected
                                           && q.fromtime == _FrmTime && q.totime == _xToTime
                                           select q).ToList();
            if (lstJlabWrk.Count > 1)
            {
                joblabwork obj = lstJlabWrk.Find(x => x.joblabcostId == null);
                if (obj != null)
                {
                    db.joblabworks.Remove(obj);
                    db.SaveChanges();
                }
            }
        }

        public List<JobDailyProduction> UpdateJobDailyProduction(string _doc_no, int Key, decimal Qty, string _jobno, DateTime? _jobdate, string _jobsystemId, string OutSourceCount, string OutSourceCompanyName, string _groupid, DateTime _ToTime, DateTime _FromTime, string SystemName)
        {
            List<JobDailyProduction> _JobDailyProductionList = new List<JobDailyProduction>();
            InterfutureEntities db = new InterfutureEntities();
            TimesheetEntities dbx = new TimesheetEntities();

            var _ObjJobDailyProduction = db.JobDailyProductions.Where(x => x.Id == Key && x.groupid == _groupid && x.jobsystemId == _jobsystemId).FirstOrDefault();

            try
            {
                string _qty = _ObjJobDailyProduction.taskname;
                string labcostid = _ObjJobDailyProduction.joblabcostId;
                string _taskname = _ObjJobDailyProduction.taskname;
                string _empcode = _ObjJobDailyProduction.empcode;
                int? _qty_man = _ObjJobDailyProduction.qty_man;
                decimal? _qty_item = _ObjJobDailyProduction.qty_item;
                decimal? _qty_done = _ObjJobDailyProduction.QtyDone;
                string _jobsystemid = _jobsystemId;
                string _unit = _ObjJobDailyProduction.unit;
                decimal? _time_min = _ObjJobDailyProduction.time_min;
                int? strJobcode = _ObjJobDailyProduction.Jobcode;
                double _Tqty_item = (Convert.ToDouble(_qty_item) * Convert.ToDouble(_time_min)); ;
                double x1 = ((Convert.ToDouble(_qty_item) * Convert.ToDouble(_time_min) / 60));
                double x2 = (x1 / 4);
                double _TTimeHrs = x1 + x2;
                decimal? _hr_cost = _ObjJobDailyProduction.hr_cost;
                double _TCost = ((x1 + x2) * (Convert.ToDouble(_hr_cost) * Convert.ToDouble(_qty_man)));
                string _jobname = _ObjJobDailyProduction.taskname;
                string strQty = Convert.ToString(Qty);
                string joblabcost1 = "";
                //InterfutureEntities db = new InterfutureEntities();
                long ids = Key;
                decimal Qty_Done = 0;
                decimal QQ = 0, QQA = 0, QQB = 0;
                decimal rem = 0;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

                DateTime dt = yourTime;
                string DateSelected = dt.ToString("yyyy-MM-dd");
                JobDailyProduction objdp = (from Dp in db.JobDailyProductions
                                            where Dp.Id == ids
                                            select Dp).FirstOrDefault();
                //decimal Qty = 0;
                if (string.IsNullOrEmpty(strQty))
                    Qty = 0;
                else
                    Qty = Convert.ToDecimal(strQty);
                int jobcode = 0;
                if (string.IsNullOrEmpty(strJobcode.ToString()))
                    jobcode = 0;
                else
                    jobcode = Convert.ToInt32(strJobcode);

                int qtyMan = 0;
                if (string.IsNullOrEmpty(_qty_man.ToString()))
                    qtyMan = 0;
                else
                    qtyMan = Convert.ToInt32(_qty_man);

                decimal hrCst = 0;

                if (string.IsNullOrEmpty(_hr_cost.ToString()))
                    hrCst = 0;
                else
                    hrCst = Convert.ToInt32(_hr_cost);

                if (labcostid.Contains('A') || labcostid.Contains('B'))
                {
                    if (labcostid.Contains('A'))
                    {
                        joblabcost1 = labcostid.Substring(0, labcostid.IndexOf('A'));
                    }
                    else if (labcostid.Contains('B'))
                    {
                        joblabcost1 = labcostid.Substring(0, labcostid.IndexOf('B'));
                    }
                    string joblabcostIdA = joblabcost1 + "A";
                    string joblabcostIdB = joblabcost1 + "B";
                    var Q2 = (from q in db.joblabworks
                              where q.joblabcostId == joblabcost1 && q.jobdate != dt
                              select q.qty_item).Sum();
                    var Q2A = (from q in db.joblabworks
                               where q.joblabcostId == joblabcostIdA && q.jobdate != dt
                               select q.qty_item).Sum();
                    var Q2B = (from q in db.joblabworks
                               where q.joblabcostId == joblabcostIdB && q.jobdate != dt
                               select q.qty_item).Sum();
                    if (Q2 != null)
                    {
                        QQ = Convert.ToDecimal(Q2);
                    }
                    if (Q2A != null)
                    {
                        QQA = Convert.ToDecimal(Q2A);
                    }
                    if (Q2B != null)
                    {
                        QQB = Convert.ToDecimal(Q2B);
                    }
                    decimal res = (QQA + QQB) / 2;
                    if (res == QQ)
                    {
                        if (objdp != null)
                        {
                            if (_qty_done.ToString() != "")
                            {
                                DateTime _DateSelected = Convert.ToDateTime(DateSelected);
                                DateTime? dateFromTime = GetFrmTime(_FromTime);
                                DateTime? dateToTime = GetToTime(_ToTime);


                                var objjlw = (from jl in db.joblabworks
                                              where jl.jobno == _jobno && jl.jobsystemId == _jobsystemid && jl.joblabcostId == objdp.joblabcostId && jl.jobdate != _DateSelected
                                              select jl.qty_item).Sum();
                                var todayQty = (from q in db.joblabworks
                                                where q.jobno == _jobno && q.jobsystemId == _jobsystemid && q.joblabcostId == objdp.joblabcostId && q.fromtime != dateFromTime && q.totime != dateToTime && q.jobdate == _DateSelected
                                                select q.qty_item).Sum();
                                Qty_Done = Convert.ToDecimal(objjlw) + Convert.ToDecimal(todayQty);
                                Qty_Done = Qty_Done + Qty;
                            }
                            else
                            {
                                Qty_Done = Qty;
                            }
                            ///////////////////
                            if (Qty_Done > Convert.ToDecimal(_qty_item))
                            {

                                //error Qty cannot exceed total Qty
                                //RadWindowManager1.RadAlert("<b>Qty cannot exceed total Qty</b>", 300, 150, "Alert", null);
                                //return;
                            }
                            //////////
                            decimal Curr_Qty = 0;
                            Curr_Qty = Qty;
                            objdp.Qty = Curr_Qty;

                            objdp.QtyDone = Qty_Done;

                            if (_FromTime != null)
                                objdp.FromTime = Convert.ToDateTime(_FromTime);

                            if (_ToTime != null)
                                objdp.ToTime = Convert.ToDateTime(_ToTime);

                            objdp.Updatedate = DateTime.Now;
                            db.SaveChanges();
                        }

                    }
                    else
                    {
                        rem = QQ - ((QQA + QQB) / 2);
                        if (labcostid.Contains('A'))
                        {
                            decimal QdonA = rem + QQA;
                            Qty_Done = QdonA + Qty;
                        }
                        else if (labcostid.Contains('B'))
                        {
                            decimal QdonB = rem + QQB;
                            Qty_Done = QdonB + Qty;
                        }
                        if (Qty_Done > Convert.ToDecimal(_qty_item))
                        {
                            //RadWindowManager1.RadAlert("<b>Qty cannot exceed total Qty</b>", 300, 150, "Alert", null);
                            //return;
                        }
                        decimal Curr_Qty = 0;
                        Curr_Qty = Qty;
                        objdp.Qty = Curr_Qty;

                        objdp.QtyDone = Qty_Done;

                        if (_FromTime != null)
                            objdp.FromTime = Convert.ToDateTime(_FromTime);

                        if (_ToTime != null)
                            objdp.ToTime = Convert.ToDateTime(_ToTime);

                        objdp.Updatedate = DateTime.Now;
                        db.SaveChanges();

                    }
                }
                else
                {
                    try
                    {


                        if (objdp != null)
                        {
                            DateTime? dateFromTime = GetFrmTime(_FromTime);
                            DateTime? dateToTime = GetToTime(_ToTime);

                            DateTime _DateSelected = Convert.ToDateTime(DateSelected);

                            if (Convert.ToString(_qty_done) != "" || Convert.ToString(_qty_done) != null)
                            {
                                var objjlw = (from jl in db.joblabworks
                                              where jl.jobno == _jobno && jl.jobsystemId == _jobsystemid && jl.joblabcostId == objdp.joblabcostId && jl.jobdate != _DateSelected
                                              select jl.qty_item).Sum();
                                var todayQty = (from q in db.joblabworks
                                                where q.jobno == _jobno && q.jobsystemId == _jobsystemid && q.joblabcostId == objdp.joblabcostId && q.fromtime != dateFromTime && q.totime != dateToTime && q.jobdate == _DateSelected
                                                select q.qty_item).Sum();
                                Qty_Done = Convert.ToDecimal(objjlw) + Convert.ToDecimal(todayQty);
                                Qty_Done = Qty_Done + Qty;
                            }
                            else
                            {
                                Qty_Done = Qty;
                            }
                            ///////////////////
                            if (Qty_Done > Convert.ToDecimal(_qty_item))
                            {
                                //RadWindowManager1.RadAlert("<b>Qty cannot exceed total Qty</b>", 300, 150, "Alert", null);
                                //return;
                            }
                            //////////
                            decimal Curr_Qty = 0;
                            Curr_Qty = Qty;
                            objdp.Qty = Curr_Qty;

                            objdp.QtyDone = Qty_Done;

                            if (_FromTime != null)
                                objdp.FromTime = _FromTime;

                            if (_ToTime != null)
                                objdp.ToTime = _ToTime;

                            //var yourTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));
                            objdp.Updatedate = yourTime;
                            db.SaveChanges();
                        }
                    }
                    catch (System.Data.EntitySqlException ex)
                    {
                        throw ex;
                    }
                }


                DateTime? xdateFromTime = GetFrmTime(_FromTime);
                DateTime? xdateToTime = GetToTime(_ToTime);

                DateTime _xDateSelected = Convert.ToDateTime(DateSelected);

                joblabwork objlw1 = (from lw in db.joblabworks
                                     where lw.doc_no == _doc_no && lw.jobsystemId == _jobsystemid
                                     && lw.jobno == _jobno && lw.joblabcostId == labcostid
                                     & lw.jobdate == _xDateSelected
                                     && lw.fromtime == xdateFromTime && lw.totime == xdateToTime
                                     select lw).FirstOrDefault();

                //This the place where other than point to JunctionBox,JunctionBox to MainRoom is added.
                if (objlw1 == null)
                {
                    if (Qty > 0)
                    {
                        objlw1 = new joblabwork();
                        objlw1.jobno = _jobno;
                        objlw1.jobsystemId = _jobsystemid;
                        objlw1.doc_no = _doc_no;
                        objlw1.qty_item = Qty;
                        objlw1.rowno = 0;
                        objlw1.joblabcostId = labcostid;
                        objlw1.jobdate = DateTime.Now;
                        objlw1.fromtime = GetFrmTime(_FromTime);
                        objlw1.totime = GetToTime(_ToTime);
                        objlw1.activity = _taskname;
                        objlw1.time_min = Convert.ToDecimal(_time_min);
                        objlw1.SystemName = SystemName;
                        //objlw1.Notes = Comments;
                        objlw1.JobCode = jobcode;
                        objlw1.hr_cost = hrCst;
                        objlw1.qty_man = qtyMan;
                        objlw1.totTime_hrs = GetTotTime_Hr(Qty, Convert.ToDecimal(_time_min), qtyMan);

                        db.joblabworks.Add(objlw1);
                        db.SaveChanges();
                    }

                }
                else
                {
                    if (Qty > 0)
                    {
                        if (objlw1.jobdate == DateTime.Now)
                        {
                            ///update the existing value
                            objlw1.jobno = _jobno;
                            objlw1.jobsystemId = _jobsystemid;
                            objlw1.doc_no = _doc_no;
                            objlw1.qty_item = Qty;
                            objlw1.rowno = 0;
                            objlw1.joblabcostId = labcostid;
                            objlw1.jobdate = DateTime.Now;
                            objlw1.fromtime = GetFrmTime(_FromTime);
                            objlw1.totime = GetToTime(_ToTime);
                            objlw1.activity = _taskname;
                            objlw1.time_min = Convert.ToDecimal(_time_min);
                            objlw1.SystemName = SystemName;
                            //objlw1.Notes = Comments;
                            objlw1.JobCode = jobcode;
                            objlw1.hr_cost = hrCst;
                            objlw1.qty_man = qtyMan;
                            objlw1.totTime_hrs = GetTotTime_Hr(Qty, Convert.ToDecimal(_time_min), qtyMan);
                            db.SaveChanges();
                        }
                        else
                        {
                            /////////add new row
                            objlw1 = new joblabwork();
                            objlw1.jobno = _jobno;
                            objlw1.jobsystemId = _jobsystemid;
                            objlw1.doc_no = _doc_no;
                            objlw1.qty_item = Qty;
                            objlw1.rowno = 0;
                            objlw1.joblabcostId = labcostid;
                            objlw1.jobdate = DateTime.Now;
                            objlw1.fromtime = GetFrmTime(_FromTime);
                            objlw1.totime = GetToTime(_ToTime);
                            objlw1.activity = _taskname;
                            objlw1.time_min = Convert.ToDecimal(_time_min);
                            objlw1.SystemName = SystemName;
                            //objlw1.Notes = Comments;
                            objlw1.JobCode = jobcode;
                            objlw1.hr_cost = hrCst;
                            objlw1.qty_man = qtyMan;
                            objlw1.totTime_hrs = GetTotTime_Hr(Qty, Convert.ToDecimal(_time_min), qtyMan);
                            db.joblabworks.Add(objlw1);
                            db.SaveChanges();

                        }
                    }
                    else
                    {
                        db.joblabworks.Remove(objlw1);
                        db.SaveChanges();
                    }
                }
                decimal totQtyDone = 0;
                //Main Activity with joblabcostid without A or B is added in this block
                if (labcostid.Contains('A') || labcostid.Contains('B'))
                {
                    List<string> lstStrJLC = new List<string>();
                    string strJLCA = joblabcost1 + "A";
                    string strJLCB = joblabcost1 + "B";
                    lstStrJLC.Add(strJLCA);
                    lstStrJLC.Add(strJLCB);
                    totQtyDone = Convert.ToDecimal((from q in db.joblabworks
                                                    where lstStrJLC.Contains(q.joblabcostId) && q.jobdate == _xDateSelected
                                                    select q.qty_item).Sum());
                    joblabwork objlabWork = (from lw in db.joblabworks
                                             where lw.doc_no == _jobno && lw.jobsystemId == _jobsystemid && lw.jobno == _jobno
                                             && lw.joblabcostId == joblabcost1 && lw.jobdate == _xDateSelected
                                             select lw).FirstOrDefault();
                    if (totQtyDone > 0)
                    {
                        decimal qtyDone = Convert.ToDecimal(totQtyDone) / 2;

                        if (objlabWork != null)
                        {
                            objlabWork.jobno = _jobno;
                            objlabWork.jobsystemId = _jobsystemid;
                            objlabWork.doc_no = _doc_no;
                            objlabWork.qty_item = qtyDone;
                            objlabWork.rowno = 0;
                            objlabWork.joblabcostId = joblabcost1;
                            objlabWork.jobdate = DateTime.Now;
                            objlabWork.fromtime = GetFrmTime(_FromTime);
                            objlabWork.totime = GetToTime(_ToTime);
                            //objlabWork.activity = _taskname;
                            objlabWork.time_min = Convert.ToDecimal(_time_min);
                            objlabWork.SystemName = SystemName;
                            //objlabWork.Notes = Comments;
                            objlabWork.JobCode = 1;
                            objlabWork.hr_cost = hrCst;
                            objlabWork.qty_man = qtyMan;
                            objlabWork.totTime_hrs = GetTotTime_Hr(qtyDone, Convert.ToDecimal(_time_min), qtyMan);
                            db.SaveChanges();
                        }
                        else
                        {
                            joblabwork objlbwk12 = new joblabwork();
                            objlabWork.jobno = _jobno;
                            objlabWork.jobsystemId = _jobsystemid;
                            objlabWork.doc_no = _doc_no;
                            objlbwk12.qty_item = Convert.ToDecimal(totQtyDone) / 2;
                            objlbwk12.rowno = 0;
                            objlbwk12.joblabcostId = joblabcost1;
                            objlbwk12.jobdate = _jobdate;
                            objlbwk12.fromtime = GetFrmTime(_FromTime);
                            objlbwk12.totime = GetToTime(_ToTime);
                            objlbwk12.activity = _taskname;
                            objlbwk12.time_min = Convert.ToDecimal(_time_min);
                            objlbwk12.SystemName = SystemName;
                            //objlbwk12.Notes = Comments;
                            objlbwk12.JobCode = 1;
                            objlbwk12.hr_cost = hrCst;
                            objlbwk12.qty_man = qtyMan;
                            objlbwk12.totTime_hrs = GetTotTime_Hr(qtyDone, Convert.ToDecimal(_time_min), qtyMan);
                            db.joblabworks.Add(objlbwk12);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (objlabWork != null)
                        {
                            db.joblabworks.Remove(objlabWork);
                            db.SaveChanges();
                        }
                    }
                }
                if (joblabcost1 != "")
                {
                    JobDailyProduction objJDP = (from q in db.JobDailyProductions
                                                 where q.joblabcostId == joblabcost1
                                                 select q).SingleOrDefault();
                    decimal ttQtyDn = Convert.ToDecimal((from q in db.joblabworks
                                                         where q.joblabcostId == joblabcost1
                                                         select q.qty_item).Sum());
                    if (objJDP != null)
                    {
                        objJDP.QtyDone = Convert.ToDecimal(ttQtyDn);
                        db.SaveChanges();
                    }
                }

                UpdateEmpDetails(_doc_no, _jobno, _jobdate, _jobsystemid, _ToTime, _FromTime, _groupid);
                UpdateOutSrcDetails(_doc_no, _jobno, _jobsystemid, _jobdate, OutSourceCount, OutSourceCompanyName, _ToTime, _FromTime);
                DateTime _varjobdate = Convert.ToDateTime(_jobdate);
                CheckCommentsOnly(_varjobdate, _jobno, _jobsystemId, _ToTime, _FromTime);
                ValidationMod(_jobno, _jobsystemid);

                _JobDailyProductionList = db.JobDailyProductions.Where(x => x.jobno == _jobno && x.groupid == _groupid && x.jobsystemId == _jobsystemId).ToList();


            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            //catch (System.Exception ex)
            //{
            //    Logger.Log(ex);
            //}

            return _JobDailyProductionList;
        }

        public List<JobDailyProduction> UpdateProductionComments(string _jobno, string _jobsystemId, string _groupid, string Comments, DateTime Checkin, DateTime Checkout)
        {
            List<JobDailyProduction> _JobDailyProductionList = new List<JobDailyProduction>();
            InterfutureEntities db = new InterfutureEntities();
            TimesheetEntities dbx = new TimesheetEntities();

            var Docno = db.enq_mast.Where(y => y.ord_no == _jobno).FirstOrDefault();
            string doc_no = Docno.doc_no;
            var systemname = db.enq_tran.Where(b => b.doc_no == doc_no && b.jobsystemId == _jobsystemId).FirstOrDefault();

            var _ObjJobDailyProduction = db.JobDailyProductions.Where(x => x.jobno == _jobno && x.groupid == _groupid && x.jobsystemId == _jobsystemId).FirstOrDefault();
            if (_ObjJobDailyProduction != null)
            {
                _ObjJobDailyProduction.Notes = Comments.ToString();
            }

            var xResult = db.joblabworkemps.Where(x => x.jobno == _jobno && x.jobsystemId == _jobsystemId && x.groupid == _groupid).ToList();
            for (int i = 0; i < xResult.Count; i++)
            {
                xResult[i].Notes = Comments;
                db.SaveChanges();
            }

            SaveComments(doc_no, _jobno, _jobsystemId, Comments, systemname.jobsystemname, Checkin, Checkout);

            _JobDailyProductionList = db.JobDailyProductions.Where(x => x.jobno == _jobno && x.groupid == _groupid && x.jobsystemId == _jobsystemId).ToList();

            return _JobDailyProductionList;
        }

        public List<JobDailyProduction> splitList(List<JobDailyProduction> _JobDailyProduction, int nSize)
        {
            var list = new List<JobDailyProduction>();

            for (int i = 0; i < nSize && i < _JobDailyProduction.Count; i++)
            {
                list.Add(_JobDailyProduction[i]);
            }

            return list;
        }

        protected bool SaveComments(string doc_no, string _jobno, string _jobsystemId, string Comments, string SystemName, DateTime _FromTime, DateTime _ToTime)
        {
            try
            {
                InterfutureEntities db = new InterfutureEntities();

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

                DateTime dt = yourTime;
                string DateSelected = dt.ToString("yyyy-MM-dd");

                DateTime _DateSelected = Convert.ToDateTime(DateSelected);
                DateTime? _FrmTime = GetFrmTime(_FromTime);
                DateTime? _xToTime = GetToTime(_ToTime);


                List<joblabwork> olw = (from lw in db.joblabworks
                                        where lw.jobno == _jobno && lw.jobsystemId == _jobsystemId
                                        && System.Data.Entity.DbFunctions.TruncateTime(lw.jobdate) == System.Data.Entity.DbFunctions.TruncateTime(_DateSelected)
                                        && lw.jobsystemId == _jobsystemId
                                        && lw.fromtime == _FrmTime && lw.totime == _xToTime
                                        select lw).ToList();

                if (olw.Count > 0)
                {
                    if (string.IsNullOrEmpty(Comments))
                    {
                        joblabwork objJLW = olw.Find(x => x.joblabcostId == null);
                        if (objJLW != null)
                        {
                            db.joblabworks.Remove(objJLW);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        foreach (joblabwork objJLW in olw)
                        {
                            objJLW.Notes = Comments;
                            db.SaveChanges();
                        }
                    }
                    return true;
                }
                else
                {
                    if (Comments == "")
                    {
                        return false;
                    }
                    else
                    {
                        joblabwork objJobLabWrk = new joblabwork();
                        objJobLabWrk.doc_no = doc_no;
                        objJobLabWrk.jobno = _jobno;
                        objJobLabWrk.jobdate = Convert.ToDateTime(dt);
                        objJobLabWrk.jobsystemId = _jobsystemId;
                        objJobLabWrk.fromtime = GetFrmTime(_FromTime);
                        objJobLabWrk.totime = GetToTime(_ToTime);
                        objJobLabWrk.Notes = Comments;
                        objJobLabWrk.qty_item = 0;
                        objJobLabWrk.activity = " ";
                        objJobLabWrk.SystemName = SystemName;
                        db.joblabworks.Add(objJobLabWrk);
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        private void UpdateOutSrcDetails(string _doc_no, string _jobno, string _jobsystemId, DateTime? _jobdate, string OutSourceCount, string OutSourceCompanyName, DateTime _ToTime, DateTime _FromTime)
        {
            try
            {
                InterfutureEntities db = new InterfutureEntities();
                DateTime? fTime = GetFrmTime(_FromTime);
                DateTime? tTime = GetToTime(_ToTime);
                List<joblabwork> lstJLW = (from q in db.joblabworks
                                           where q.jobno == _jobno && q.jobdate == _jobdate && q.fromtime == fTime && q.totime == tTime
                                           select q).ToList();
                OutSourceDetail objOSdetail = (from q in db.OutSourceDetails
                                               where q.jobno == _jobno && q.jobdate == _jobdate && q.fromtime == fTime && q.totime == tTime
                                               select q).FirstOrDefault();
                if (lstJLW.Count <= 0)
                {
                    if (objOSdetail != null)
                    {
                        db.OutSourceDetails.Remove(objOSdetail);
                        db.SaveChanges();
                    }
                }
                else
                {
                    int noOS = 0;
                    if (!string.IsNullOrEmpty(OutSourceCount))
                        noOS = Convert.ToInt32(OutSourceCount);
                    if (objOSdetail != null)
                    {
                        if (noOS == 0)
                        {
                            db.OutSourceDetails.Remove(objOSdetail);
                        }
                        else
                        {
                            objOSdetail.noOfOutsource = noOS;
                            objOSdetail.companyName = OutSourceCompanyName;
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        if (noOS > 0)
                        {
                            objOSdetail = new OutSourceDetail();
                            objOSdetail.doc_no = _doc_no;
                            objOSdetail.jobno = _jobno;
                            objOSdetail.jobdate = _jobdate;
                            objOSdetail.noOfOutsource = noOS;
                            objOSdetail.companyName = OutSourceCompanyName;
                            objOSdetail.fromtime = fTime;
                            objOSdetail.totime = tTime;
                            db.OutSourceDetails.Add(objOSdetail);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void UpdateEmpDetails(string _doc_no, string _jobno, DateTime? _jobdate, string _jobsystemId, DateTime _ToTime, DateTime _FromTime, string _groupId)
        {
            InterfutureEntities db = new InterfutureEntities();
            TimesheetEntities dbx = new TimesheetEntities();

            DateTime? fTime = GetFrmTime(_FromTime);
            DateTime? tTime = GetToTime(_ToTime);

            try
            {
                List<joblabworkemp> lstJLWrkEmp = (from q in db.joblabworkemps
                                                   where q.jobno == _jobno
                                                   && System.Data.Entity.DbFunctions.TruncateTime(q.jobdate) == System.Data.Entity.DbFunctions.TruncateTime(_jobdate)
                                                   && q.jobsystemId == _jobsystemId
                                                   && q.fromtime == fTime
                                                   && q.totime == tTime
                                                   select q).ToList();

                List<string> lstJWrkEmp = (from q in db.joblabworkemps
                                           where q.jobno == _jobno
                                           && System.Data.Entity.DbFunctions.TruncateTime(q.jobdate) == System.Data.Entity.DbFunctions.TruncateTime(_jobdate)
                                           && q.fromtime == fTime
                                           && q.totime == tTime
                                           select q.empname).ToList();

                List<joblabwork> lstJLW = (from q in db.joblabworks
                                           where q.jobno == _jobno && q.jobsystemId == _jobsystemId
                                           && System.Data.Entity.DbFunctions.TruncateTime(q.jobdate) == System.Data.Entity.DbFunctions.TruncateTime(_jobdate)
                                           && q.fromtime == fTime
                                           && q.totime == tTime
                                           select q).ToList();

                if (lstJLW.Count <= 0)
                {
                    if (lstJLWrkEmp.Count > 0)
                    {
                        foreach (var shift in lstJLWrkEmp)
                        {
                            db.Entry(shift).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    //if (lstJLWrkEmp.Count <= 0)
                    //{
                    List<empTmp> lstEmpTmp = (from q in db.empTmps
                                              where q.groupId == _groupId
                                              select q).ToList();

                    if (lstJLWrkEmp.Count == 0)
                    {
                        foreach (empTmp objEmptmp in lstEmpTmp)
                        {
                            joblabworkemp objJLWrkEmp = new joblabworkemp();
                            objJLWrkEmp.doc_no = _doc_no;
                            objJLWrkEmp.jobno = _jobno;
                            objJLWrkEmp.jobsystemId = _jobsystemId;
                            objJLWrkEmp.jobdate = _jobdate;
                            objJLWrkEmp.empid = objEmptmp.EmpId;
                            objJLWrkEmp.empname = objEmptmp.EmpName;
                            //objJLWrkEmp.Notes = _groupId.ToString();
                            objJLWrkEmp.fromtime = fTime;
                            objJLWrkEmp.totime = tTime;
                            objJLWrkEmp.groupid = _groupId.ToString();

                            db.joblabworkemps.Add(objJLWrkEmp);
                            db.SaveChanges();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void ValidationMod(string _jobno, string _jobsystemid)
        {
            try
            {

                InterfutureEntities db = new InterfutureEntities();
                var deleteOrderDetails = from details in db.JobLabWorkTemps
                                         select details;
                foreach (var detail in deleteOrderDetails)
                {
                    db.JobLabWorkTemps.Remove(detail);
                }
                db.SaveChanges();




                //==========

                InterfutureEntities context = new InterfutureEntities();
                string cd1, date1;

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                DateTime yourTime1 = TimeZoneInfo.ConvertTime(currentDate, cet);


                cd1 = System.DateTime.Today.ToLongDateString();
                string hour = "00";
                string minute = "00";
                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.Month.ToString();
                string day = DateTime.Now.Day.ToString();
                date1 = "'" + year + "-" + month + "-" + day + " " + hour + ":" + minute + ":00'";// '2003-12-23 22:12:00' - Note the '   
                int[] r1 = new int[] { 1, 2, 3 };
                foreach (int i in r1)
                {
                    //=======PlanTime
                    string ConnStringg = ConfigurationManager.ConnectionStrings["InterfutureConnectionString"].ConnectionString;
                    SqlConnection con5 = new SqlConnection(ConnStringg);
                    string mycmd5 = "select sum(timealloc) as num from jobplanday where jobno='" + _jobno + "' and jobsystemid='" + _jobsystemid + "'  and jobcode=" + i + " and trandate <=" + date1 + "";
                    SqlCommand mysqlcmd5 = new SqlCommand(mycmd5, con5);
                    con5.Open();
                    SqlDataReader myreader5 = mysqlcmd5.ExecuteReader();
                    while (myreader5.Read())
                    {
                        _PlanTime = myreader5["num"].ToString();
                        if (_PlanTime != "")
                        {
                            _PlanTime1 = Convert.ToDecimal(_PlanTime);
                        }
                        else
                        {
                            _PlanTime1 = 0;
                        }

                    }
                    con5.Close();
                    myreader5.Close();
                    //=======Worktime
                    // string ConnStringg = ConfigurationManager.ConnectionStrings["InterfutureConnectionString"].ConnectionString;
                    SqlConnection con6 = new SqlConnection(ConnStringg);
                    string mycmd6 = "select sum(joblabwork.qty_item*joblabcost.time_min) as num from joblabwork left join joblabcost on joblabcost.id=joblabwork.jobLabCostId where joblabwork.joblabcostId not like '%A' and joblabwork.joblabcostId not like '%B' and joblabwork.jobno='" + _jobno + "' and joblabwork.jobsystemid='" + _jobsystemid + "' and joblabcost.jobcode=" + i + " and jobdate <=" + date1 + "";
                    SqlCommand mysqlcmd6 = new SqlCommand(mycmd6, con6);
                    con6.Open();
                    SqlDataReader myreader6 = mysqlcmd6.ExecuteReader();
                    while (myreader6.Read())
                    {
                        _Worjtime = myreader6["num"].ToString();
                        if (_Worjtime != "")
                        {
                            _Worjtime1 = Convert.ToDecimal(_Worjtime);
                        }
                        else
                        {
                            _Worjtime1 = 0;
                        }
                    }
                    con6.Close();
                    myreader6.Close();
                    //Balance

                    decimal _Balance = _PlanTime1 - _Worjtime1;
                    InterfutureEntities db3 = new InterfutureEntities();
                    JobLabWorkTemp item = new JobLabWorkTemp();
                    if (i == 1)
                    {
                        item.TaskName = "Cabling";
                    }
                    else if (i == 2)
                    {
                        item.TaskName = "Installation";
                    }

                    else if (i == 3)
                    {
                        item.TaskName = "Commissioning";
                    }

                    item.PlanTime = Convert.ToDouble(_PlanTime1);
                    item.Worjtime = Convert.ToDouble(_Worjtime1);
                    item.Balance = Convert.ToDouble(_Balance);
                    db3.JobLabWorkTemps.Add(item);
                    db3.SaveChanges();
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log(ex);
            }
        }

        #region Helper

        public static DataTable GetDataTable(string query)
        {
            string ConnString = ConfigurationManager.ConnectionStrings["InterfutureConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(ConnString);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = new SqlCommand(query, conn);

            DataTable myDataTable = new DataTable();

            conn.Open();
            try
            {
                adapter.Fill(myDataTable);
            }
            finally
            {
                conn.Close();
            }
            return myDataTable;
        }

        private DateTime? GetFrmTime(DateTime FrmTime)
        {
            try
            {
                //if (RadTimePicker1.SelectedDate != null)
                //{
                DateTime d = FrmTime;
                string time = d.ToString("HH:mm:ss");
                string dtt = DateTime.Now.ToShortDateString() + " " + time;
                DateTime datTime = Convert.ToDateTime(dtt);
                return datTime;
                //}
                //else
                //    return null;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }

        }

        private DateTime? GetToTime(DateTime ToTime)
        {
            try
            {
                //if (RadTimePicker2.SelectedDate != null)
                //{
                DateTime d = ToTime;
                string time = d.ToString("HH:mm:ss");
                string dtt = DateTime.Now.ToShortDateString() + " " + time;
                DateTime datTime = Convert.ToDateTime(dtt);
                return datTime;
                //}
                //else
                //    return null;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        private decimal GetTotTime_Hr(decimal qty_item, decimal time_min, int qty_man)
        {
            decimal totTime_Hrs = 0;
            if (qty_man != 0)
            {
                decimal totTime_min = (qty_item * time_min) / qty_man;
                decimal tot_time_hr1 = totTime_min / 60;
                decimal tot_time_hr2 = totTime_min / 240;
                totTime_Hrs = tot_time_hr1 + tot_time_hr2;
            }
            return totTime_Hrs;
        }

        public int GenerateRandomNo()
        {
            int _min = 3000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        public object[,] xConvert(DataTable dt)
        {
            var rows = dt.Rows;
            int rowCount = rows.Count;
            int colCount = dt.Columns.Count;
            var result = new object[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                var row = rows[i];
                for (int j = 0; j < colCount; j++)
                {
                    result[i, j] = row[j];
                }
            }

            return result;
        }

        public void WriteTsv<T>(IEnumerable<T> data, TextWriter output)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in props)
            {
                output.Write(prop.DisplayName); // header
                output.Write("\t");
            }
            output.WriteLine();
            foreach (T item in data)
            {
                foreach (PropertyDescriptor prop in props)
                {
                    output.Write(prop.Converter.ConvertToString(
                         prop.GetValue(item)));
                    output.Write("\t");
                }
                output.WriteLine();
            }
        }

        public DateTime GetNetworkTime()
        {
            var ntpDateTime = DateTime.Now.FromNtp();
            return ntpDateTime;
        }

        #endregion Helper

        #region Dashboard

        public List<DashboardGridDTO> DashboardDetailsForProjectEngineers(DateTime fromdate, DateTime todate, string jobno)
        {
            List<DashboardGridDTO> _DashboardGridDTO = new List<DashboardGridDTO>();

            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);

            try
            {
                if (jobno != null && jobno != "")
                {
                    var query = (from ts in _TimesheetEntities.joblabworks
                                 where System.Data.Entity.DbFunctions.TruncateTime(ts.jobdate) >= System.Data.Entity.DbFunctions.TruncateTime(_fromdate)
                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.jobdate) <= System.Data.Entity.DbFunctions.TruncateTime(_todate)
                                 && (ts.jobno == jobno)
                                 select new DashboardGridDTO
                                 {
                                     doc_no = ts.doc_no,
                                     jobno = ts.jobno,
                                     jobsystemId = ts.jobsystemId,
                                     qty_item = ts.qty_item,
                                     rowno = ts.rowno,
                                     joblabcostId = ts.joblabcostId,
                                     jobdate = ts.jobdate,
                                     FlageMod = ts.FlageMod,
                                     Notes = ts.Notes,
                                     fromtime = ts.fromtime,
                                     totime = ts.totime,
                                     JobCode = ts.JobCode,
                                     Id = ts.Id,
                                     time_min = ts.time_min,
                                     activity = ts.activity,
                                     SystemName = ts.SystemName,
                                     qty_man = ts.qty_man,
                                     hr_cost = ts.hr_cost,
                                     totTime_hrs = ts.totTime_hrs
                                 }).OrderByDescending(x => x.jobdate).ToList();

                    _DashboardGridDTO = query;
                }
                else
                {

                    var query = (from ts in _TimesheetEntities.joblabworks
                                 where System.Data.Entity.DbFunctions.TruncateTime(ts.jobdate) >= System.Data.Entity.DbFunctions.TruncateTime(_fromdate)
                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.jobdate) <= System.Data.Entity.DbFunctions.TruncateTime(_todate)
                                 select new DashboardGridDTO
                                 {
                                     doc_no = ts.doc_no,
                                     jobno = ts.jobno,
                                     jobsystemId = ts.jobsystemId,
                                     qty_item = ts.qty_item,
                                     rowno = ts.rowno,
                                     joblabcostId = ts.joblabcostId,
                                     jobdate = ts.jobdate,
                                     FlageMod = ts.FlageMod,
                                     Notes = ts.Notes,
                                     fromtime = ts.fromtime,
                                     totime = ts.totime,
                                     JobCode = ts.JobCode,
                                     Id = ts.Id,
                                     time_min = ts.time_min,
                                     activity = ts.activity,
                                     SystemName = ts.SystemName,
                                     qty_man = ts.qty_man,
                                     hr_cost = ts.hr_cost,
                                     totTime_hrs = ts.totTime_hrs
                                 }).OrderByDescending(x => x.jobdate).ToList();

                    _DashboardGridDTO = query;
                }

            }
            catch (Exception e)
            {
                //_joblabwork.Message = e.Message.ToString();
            }
            return _DashboardGridDTO;
        }

        public GridDashboardEmployeeDTO DashboardEmployeesFromDashboardGrid(string JobNo, DateTime? jobdate, string SystemName, string activity, string qty_item, DateTime? fromtime, DateTime? totime, string jobsystemId)
        {

            GridDashboardEmployeeDTO _GridDashboardEmployeeDTO = new GridDashboardEmployeeDTO();

            List<joblabworkemp> _joblabworkemp = new List<joblabworkemp>();

            _GridDashboardEmployeeDTO.JobNo = JobNo;
            _GridDashboardEmployeeDTO.jobdate = jobdate;
            _GridDashboardEmployeeDTO.SystemName = SystemName;
            _GridDashboardEmployeeDTO.activity = activity;
            _GridDashboardEmployeeDTO.qty_item = qty_item;
            _GridDashboardEmployeeDTO.fromtime = fromtime;
            _GridDashboardEmployeeDTO.totime = totime;

            try
            {


                _joblabworkemp = _InterfutureEntities.joblabworkemps
                                  .Where(p => p.jobno == JobNo
                                    //&& System.Data.Entity.DbFunctions.TruncateTime(p.jobdate) == System.Data.Entity.DbFunctions.TruncateTime(jobdate)
                                    && p.fromtime == fromtime && p.totime == totime)
                                  .GroupBy(p => new { p.empname })
                                  .Select(g => g.FirstOrDefault())
                                  .ToList();
            }
            catch (Exception)
            {

                throw;
            }

            _GridDashboardEmployeeDTO._joblabworkemplist = _joblabworkemp;

            return _GridDashboardEmployeeDTO;

        }

        #endregion Dashboard

        #region Complain

        public async Task<List<TimesheetComGridPostDTO>> getComplaintTimesheetData(string user_id)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO = new List<TimesheetComGridPostDTO>();
            _TimesheetComGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID // ts.emp_id equals em.empID
                                                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps //ts.emp_id equals tu.emp_id into ps
                                                             from tu in ps.DefaultIfEmpty()
                                                             where ts.user_id == user_id
                                                             && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                             && ts.is_complain == true
                                                             select new TimesheetComGridPostDTO
                                                             {
                                                                 id = ts.id,
                                                                 com_no = ts.job_no,
                                                                 checkin = ts.checkin,
                                                                 checkout = ts.checkout,
                                                                 emp_id = ts.emp_id.ToString(),
                                                                 time = ts.time,
                                                                 empname = em.Firstname,
                                                                 is_checkout = ts.is_checkout,
                                                                 groupid = ts.groupid

                                                             }).OrderByDescending(x => x.checkin).ToList());
            return _TimesheetComGridPostDTO;
        }

        public async Task<List<ComplainDTO>> getUnsolvedComplainList()
        {
            List<ComplainDTO> _ComplainDTOList = new List<ComplainDTO>();
            try
            {
                _ComplainDTOList = await Task.Run(() => (from a in _TimesheetEntities.complaints
                                                         join b in _TimesheetEntities.customers on a.ac_code equals b.AC_CODE
                                                         join c in _TimesheetEntities.complainttypes on new { com_type = a.com_type } equals new { com_type = (System.Data.Entity.SqlServer.SqlFunctions.StringConvert((decimal)c.Id)).Trim() }
                                                         join d in _TimesheetEntities.contacts on ((decimal)a.ac_sub) equals d.ac_sub
                                                         where a.doc_status != "* Solved *"
                                                         select new ComplainDTO
                                                         {
                                                             //complaint
                                                             com_no = a.com_no,
                                                             com_date = a.com_date,//
                                                             com_job = a.com_job,//
                                                             com_name = a.com_name,//
                                                             com_narration = a.com_narration,//
                                                             com_person = a.com_person,//
                                                             com_phone = a.com_phone,//
                                                             com_status = a.com_status,//
                                                             Priority = a.Priority,//
                                                             Job_name = a.Job_name,//

                                                             //Customer
                                                             AC_NAME = b.AC_NAME,
                                                             AC_TELE1 = b.AC_TELE1,
                                                             AC_MOBNO = b.AC_MOBNO,
                                                             AC_EMAIL = b.AC_EMAIL,
                                                             //complainttype
                                                             complainttype_ac_name = c.ac_name,
                                                             //contact

                                                             contact_Telno = d.Telno,
                                                             contact_Empname = d.Empname,
                                                             contact_Mobno = d.Mobno,
                                                             contact_Email = d.Email

                                                         }).ToList());

            }
            catch (Exception s)
            {

                throw s;
            }

            return _ComplainDTOList;
        }

        public async Task<List<WorkInProgress>> getAllActivitiesPerComplain(string com_no)
        {
            List<WorkInProgress> _WorkInProgressList = new List<WorkInProgress>();

            _WorkInProgressList = await Task.Run(() => _TimesheetEntities.WorkInProgresses.Where(x => x.com_no == com_no).ToList());
            return _WorkInProgressList;

        }

        public async Task<List<TimesheetComGridPostDTO>> setTimesheetComData(TimesheetComPostDTO _TimesheetComPostDTO, ts_user _ts_user)
        {
            List<TimesheetComGridPostDTO> _objTimesheetComPostDTO = new List<TimesheetComGridPostDTO>();

            try
            {
                string _groupId = Guid.NewGuid().ToString();
                //interfuture employee
                #region  interfuture employee
                if (_TimesheetComPostDTO.Team.Count > 0)
                {
                    for (int i = 0; i < _TimesheetComPostDTO.Team.Count; i++)
                    {
                        if (_TimesheetComPostDTO.Team[i] != "")
                        {
                            ts_timesheet _ts_timesheet_com = new ts_timesheet();
                            _ts_timesheet_com.id = Guid.NewGuid().ToString();
                            _ts_timesheet_com.user_id = _ts_user.id;
                            _ts_timesheet_com.emp_id = Convert.ToInt32(_TimesheetComPostDTO.Team[i]);
                            _ts_timesheet_com.job_no = _TimesheetComPostDTO.ComplainNo;
                            _ts_timesheet_com.checkin = _TimesheetComPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                            //DateTime _checkout = Convert.ToDateTime("05:00 PM");
                            _ts_timesheet_com.checkout = null;// _checkout.ToString("HH:mm tt", CultureInfo.InvariantCulture);
                            _ts_timesheet_com.time = null;
                            _ts_timesheet_com.place = null;
                            _ts_timesheet_com.is_checkout = false;
                            _ts_timesheet_com.is_complain = true;

                            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                            var yourTime1 = TimeZoneInfo.ConvertTime(currentDate, cet);

                            _ts_timesheet_com.ondate_created = yourTime1;
                            _ts_timesheet_com.createdby = _ts_user.name;
                            _ts_timesheet_com.groupid = _groupId;

                            _TimesheetEntities.ts_timesheet.Add(_ts_timesheet_com);
                            _TimesheetEntities.SaveChanges();
                        }
                    }
                }
                #endregion  interfuture employee

                //outsourced team
                #region outsourced team
                if (_TimesheetComPostDTO.OutsourceTeam != null)
                {
                    for (int i = 0; i < _TimesheetComPostDTO.OutsourceTeam.Count; i++)
                    {
                        ts_timesheet _ts_timesheet_com = new ts_timesheet();
                        _ts_timesheet_com.id = Guid.NewGuid().ToString();
                        _ts_timesheet_com.user_id = _ts_user.id;
                        _ts_timesheet_com.emp_id = Convert.ToInt32(_TimesheetComPostDTO.Team[i]);
                        _ts_timesheet_com.job_no = _TimesheetComPostDTO.ComplainNo;
                        _ts_timesheet_com.checkin = _TimesheetComPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                        //DateTime _checkout = Convert.ToDateTime("05:00 PM");
                        _ts_timesheet_com.checkout = null;// _checkout.ToString("HH:mm tt", CultureInfo.InvariantCulture);
                        _ts_timesheet_com.time = null;
                        _ts_timesheet_com.place = null;
                        _ts_timesheet_com.is_outsourced = true;
                        _ts_timesheet_com.is_checkout = false;
                        _ts_timesheet_com.is_complain = true;

                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                        DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                        var yourTime1 = TimeZoneInfo.ConvertTime(currentDate, cet);

                        _ts_timesheet_com.ondate_created = yourTime1;
                        _ts_timesheet_com.createdby = _ts_user.name;
                        _ts_timesheet_com.groupid = _groupId;

                        _TimesheetEntities.ts_timesheet.Add(_ts_timesheet_com);
                        _TimesheetEntities.SaveChanges();
                    }
                }
                #endregion outsourced team

                //login user
                #region login user
                ts_timesheet _ts_timesheetlogin = new ts_timesheet();
                _ts_timesheetlogin.id = Guid.NewGuid().ToString();
                _ts_timesheetlogin.user_id = _ts_user.id;
                _ts_timesheetlogin.emp_id = _ts_user.emp_id;
                _ts_timesheetlogin.job_no = _TimesheetComPostDTO.ComplainNo;
                _ts_timesheetlogin.checkin = _TimesheetComPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                //DateTime _xcheckout = Convert.ToDateTime("05:00 PM");
                _ts_timesheetlogin.checkout = null;// _xcheckout.ToString("HH:mm tt", CultureInfo.InvariantCulture);
                _ts_timesheetlogin.time = null;
                _ts_timesheetlogin.place = null;
                _ts_timesheetlogin.is_checkout = false;
                _ts_timesheetlogin.is_complain = true;

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

                _ts_timesheetlogin.ondate_created = swedishTime;
                _ts_timesheetlogin.createdby = _ts_user.name;
                _ts_timesheetlogin.groupid = _groupId;

                _TimesheetEntities.ts_timesheet.Add(_ts_timesheetlogin);
                _TimesheetEntities.SaveChanges();

                #endregion login user


                //location
                ts_place _ts_place = new ts_place();
                _ts_place.ts_timesheet_groupid = _groupId;
                _ts_place.latlang = _TimesheetComPostDTO.location.latlng;
                _ts_place.name = _TimesheetComPostDTO.location.name;
                _ts_place.formatted_address = _TimesheetComPostDTO.location.formatted_address;

                _TimesheetEntities.ts_place.Add(_ts_place);
                _TimesheetEntities.SaveChanges();

                //Return Value
                #region ReturnValue

                _objTimesheetComPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                from tu in ps.DefaultIfEmpty()
                                                                where ts.user_id == _ts_user.id
                                                                && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                                && ts.is_complain == true
                                                                select new TimesheetComGridPostDTO
                                                                {
                                                                    id = ts.id,
                                                                    com_no = ts.job_no,
                                                                    checkin = ts.checkin,
                                                                    checkout = ts.checkout,
                                                                    emp_id = ts.emp_id.ToString(),
                                                                    time = ts.time,
                                                                    empname = em.Firstname,
                                                                    is_checkout = ts.is_checkout,
                                                                    groupid = ts.groupid

                                                                }).OrderByDescending(x => x.checkin).ToList());
                #endregion ReturnValue

            }
            catch (Exception e)
            {
                ////throw e;
                //_TimesheetGridDTO[0].empname = e.ToString();
            }

            return _objTimesheetComPostDTO;
        }

        public async Task<List<TimesheetComGridPostDTO>> AllComplainCheckOut(ts_user _ts_user, string endtime)
        {
            List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO = new List<TimesheetComGridPostDTO>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

            try
            {
                var eQuery = (from ts in _TimesheetEntities.ts_timesheet
                              where ts.user_id == _ts_user.id
                              && ts.is_checkout == false
                              && ts.is_complain == true
                              && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                              select new { ts }).ToList();

                foreach (var value in eQuery)
                {
                    TimeSpan duration = DateTime.Parse(endtime).Subtract(DateTime.Parse(value.ts.checkin));
                    value.ts.is_checkout = true;
                    value.ts.checkout = endtime;
                    value.ts.time = duration.ToString(@"hh\:mm");
                    _TimesheetEntities.SaveChanges();
                }

                _TimesheetComGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                 join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                 join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                 from tu in ps.DefaultIfEmpty()
                                                                 where ts.user_id == _ts_user.id
                                                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                                                                 select new TimesheetComGridPostDTO
                                                                 {
                                                                     id = ts.id,
                                                                     com_no = ts.job_no,
                                                                     checkin = ts.checkin,
                                                                     checkout = ts.checkout,
                                                                     emp_id = ts.emp_id.ToString(),
                                                                     time = ts.time,
                                                                     empname = em.Firstname,
                                                                     is_checkout = ts.is_checkout,
                                                                     groupid = ts.groupid

                                                                 }).OrderByDescending(x => x.checkin).ToList());

            }
            catch (Exception e)
            {

            }

            return _TimesheetComGridPostDTO;
        }

        public List<ts_timesheet_com> CheckComplainUserIsAdmin(string userid)
        {
            List<ts_timesheet_com> _ts_timesheet_com = new List<ts_timesheet_com>();
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

                _ts_timesheet_com = _TimesheetEntities.ts_timesheet_com
                                    .Where(x => x.userid == userid
                                                        && x.is_checkout == false
                                                        && System.Data.Entity.DbFunctions.TruncateTime(x.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)).ToList();
            }
            catch (Exception e)
            {

            }

            return _ts_timesheet_com;
        }

        public async Task<List<TimesheetComGridPostDTO>> IndividualComplainCheckOut(ts_user _ts_user, int? emp_id, string endtime)
        {
            List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO = new List<TimesheetComGridPostDTO>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

            try
            {

                var eQuery = (from ts in _TimesheetEntities.ts_timesheet
                              where ts.emp_id == emp_id
                              && ts.is_checkout == false
                              && ts.is_complain == true
                              && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                              select new { ts }).FirstOrDefault();


                if (eQuery != null)
                {
                    TimeSpan duration = DateTime.Parse(endtime).Subtract(DateTime.Parse(eQuery.ts.checkin));

                    eQuery.ts.is_checkout = true;
                    eQuery.ts.checkout = endtime;
                    eQuery.ts.time = duration.ToString(@"hh\:mm");

                    _TimesheetEntities.SaveChanges();
                }

                _TimesheetComGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                 join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                 join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                 from tu in ps.DefaultIfEmpty()
                                                                 where ts.user_id == _ts_user.id
                                                                 && ts.is_complain == true
                                                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                                                                 select new TimesheetComGridPostDTO
                                                                 {
                                                                     id = ts.id,
                                                                     com_no = ts.job_no,
                                                                     checkin = ts.checkin,
                                                                     checkout = ts.checkout,
                                                                     emp_id = ts.emp_id.ToString(),
                                                                     time = ts.time,
                                                                     empname = em.Firstname,
                                                                     is_checkout = ts.is_checkout,
                                                                     groupid = ts.groupid

                                                                 }).OrderByDescending(x => x.checkin).ToList());

            }
            catch (Exception e)
            {

            }

            return _TimesheetComGridPostDTO;
        }

        public async Task<ComplainDTO> getComplainActivityDetails(string ComNo)
        {
            ComplainDTO _ComplainDTO = new ComplainDTO();

            _ComplainDTO = await Task.Run(() => (from a in _TimesheetEntities.complaints
                                                 join b in _TimesheetEntities.customers on a.ac_code equals b.AC_CODE
                                                 join c in _TimesheetEntities.complainttypes on new { com_type = a.com_type } equals new { com_type = (System.Data.Entity.SqlServer.SqlFunctions.StringConvert((decimal)c.Id)).Trim() }
                                                 join d in _TimesheetEntities.contacts on ((decimal)a.ac_sub) equals d.ac_sub
                                                 where a.com_no == ComNo
                                                 select new ComplainDTO
                                                 {
                                                     //complaint
                                                     com_no = a.com_no,
                                                     com_date = a.com_date,//
                                                     com_job = a.com_job,//
                                                     com_name = a.com_name,//
                                                     com_narration = a.com_narration,//
                                                     com_person = a.com_person,//
                                                     com_phone = a.com_phone,//
                                                     com_status = a.com_status,//
                                                     Priority = a.Priority,//
                                                     Job_name = a.Job_name,//

                                                     //Customer
                                                     AC_NAME = b.AC_NAME,
                                                     AC_TELE1 = b.AC_TELE1,
                                                     AC_MOBNO = b.AC_MOBNO,
                                                     AC_EMAIL = b.AC_EMAIL,
                                                     //complainttype
                                                     complainttype_ac_name = c.ac_name,
                                                     //contact

                                                     contact_Telno = d.Telno,
                                                     contact_Empname = d.Empname,
                                                     contact_Mobno = d.Mobno,
                                                     contact_Email = d.Email

                                                 }).FirstOrDefault());


            return _ComplainDTO;
        }

        public async Task<string> SubmitComplainActivity(string is_solved, string com_no, string techproblem, string techsolution, string remarks, string reason)
        {
            string Message = "";
            try
            {
                _com_no = com_no;
                _reason = reason;
                complaint objCOM = await Task.Run(() => (from com in _TimesheetEntities.complaints
                                                         where com.com_no == com_no
                                                         select com).FirstOrDefault());

                var result = await Task.Run(() => (from com in _TimesheetEntities.ts_timesheet
                                                   where com.job_no == com_no
                                                   select com).FirstOrDefault());

                if (objCOM != null)
                {
                    objCOM.techproblem = techproblem;
                    objCOM.techsolution = techsolution;

                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                    DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                    var yourTime1 = TimeZoneInfo.ConvertTime(currentDate, cet);

                    objCOM.com_iss_time = yourTime1 + result.checkin + "," + yourTime1;

                    objCOM.remarks = remarks;
                    decimal ac_sub = Convert.ToDecimal(objCOM.ac_sub);
                    var cont = await Task.Run(() => (from con in _TimesheetEntities.contacts
                                                     where con.ac_sub == ac_sub
                                                     select con).FirstOrDefault());
                    Email = cont.Email.ToString();
                    if (is_solved.ToString() == "YES")
                    {
                        objCOM.doc_status = "* Solved *";
                        objCOM.doc_level = 0;
                        _TimesheetEntities.SaveChanges();
                        await Task.Run(() => SolvedComplainMailtoCustomer());
                    }
                    if (is_solved.ToString() == "NO")
                    {
                        objCOM.doc_status = "Work In Progress";
                        objCOM.doc_level = 1;
                        _TimesheetEntities.SaveChanges();
                        await Task.Run(() => NotSolvedComplainMailtoTechnicalDept());
                    }
                }
                return Message;
            }
            catch (Exception e)
            {

                return Message = e.ToString();
            }
        }

        private void SolvedComplainMailtoCustomer()
        {
            try
            {
                TimesheetEntities db = new TimesheetEntities();
                //Mail Message
                MailMessage mM = new MailMessage();
                //Mail Address
                mM.From = new MailAddress("noreply.Interfuture@gmail.com");
                //receiver email id      
                if (Email != "")
                {
                    //mM.To.Add("khazi.mohdshafi@gmail.com");//For TestEmail
                    mM.To.Add("sazid62@gmail.com");//For Live
                    mM.Bcc.Add("marketing@interfuture.ae");//For Live
                    mM.Bcc.Add("kimberly@interfuture.ae");
                }
                else
                {
                    //mM.To.Add("khazi.mohdshafi@gmail.com");//For Test                
                    mM.To.Add("marketing@interfuture.ae");//For Live
                    mM.Bcc.Add("kimberly@interfuture.ae");
                }
                ///////Customer mail Format///////////
                mM.Subject = "Your Complaint has been Solved. ";

                string mailformat = SolvedCustomerMailFormat();
                mM.IsBodyHtml = true;
                mM.Body = mailformat.ToString();
                //////////////////////////////////////
                ////SMTP client
                //SmtpClient sC = new SmtpClient("smtp.gmail.com");
                ////port number for Hot mail
                //sC.Port = 587;
                ////credentials to login in to hotmail account
                //sC.UseDefaultCredentials = false;
                //sC.Credentials = new NetworkCredential("noreply.Interfuture@gmail.com", "interfuture123");
                //sC.DeliveryMethod = SmtpDeliveryMethod.Network;
                ////enabled SSL
                //sC.EnableSsl = true;
                ////Send an email
                //sC.Send(mM);


                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("noreply.Interfuture@gmail.com", "interfuture123");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mM);



            }
            catch (Exception ex)
            {


            }
        }

        private string SolvedCustomerMailFormat()
        {
            try
            {
                TimesheetEntities db = new TimesheetEntities();
                complaint objCOM = (from com in db.complaints
                                    where com.com_no == _com_no
                                    select com).FirstOrDefault();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<div>");
                sb.AppendLine("<table style='width:100%;font-family:Corbel'>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<td  style='font-weight:bold'>");
                sb.AppendFormat("Dear Valued Customer,");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
                sb.AppendLine("<tr style='height:10px'><td></td></tr>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<td >");
                sb.AppendLine("We would like to inform you that your complaint has been solved.");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
                sb.AppendLine("<tr style='height:10px'><td></td></tr>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<td style='font-weight: bold;'>Below are the details: </td>");
                sb.AppendLine("</tr>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<td> <span style='font-weight:bold;'>Complaint No: </span>");
                sb.AppendFormat("" + _com_no + " </td>");
                sb.AppendLine("</tr>");
                //sb.AppendLine("<tr><td><br /></td></tr>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<td ><span style='font-weight:bold;'>Job No: </span>");
                sb.AppendFormat("" + objCOM.com_job + "</td>");
                sb.AppendLine("</tr>");
                sb.AppendLine("<tr style='height:10px'><td></td></tr>");

                if (objCOM != null)
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td ><span style='font-weight:bold;text-decoration:underline'> Attended by:</span> ");
                    sb.AppendFormat("</td>");
                    sb.AppendLine("</tr>");
                    // sb.AppendLine("<tr style='height:10px'><td></td></tr>");

                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td> <span style='font-weight:bold;'> Technician Name:</span>");
                    sb.AppendFormat("" + objCOM.com_iss_to.ToString() + "  </td>");
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td> <span style='font-weight:bold;'>Date :</span>");
                    string comdate = DateTime.Now.ToString();
                    string[] comdate1 = comdate.Split(' ');
                    sb.AppendFormat("" + comdate1[0] + "  </td>");
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr style='height:10px'><td></td></tr>");
                }

                sb.AppendLine("<tr>");
                sb.AppendLine("<td>For any questions or concerns, please call InterFuture Customer Contact Center at the following numbers: T: +971 4 262 4465  </td>");
                sb.AppendLine("</tr>");
                sb.AppendLine("<tr style='height:10px'><td></td></tr>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>And you may also send an email to marketing@interfuture.ae  </td>");
                sb.AppendLine("</tr>");
                sb.AppendLine("<tr style='height:10px'><td></td></tr>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<td> This is a system-generated e-mail. Please do not reply.  </td>");
                sb.AppendLine("</tr>");
                sb.AppendLine("<tr style='height:10px'><td></td></tr>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>");
                sb.AppendLine("Thank you for doing business with us.");
                sb.AppendLine("<br /><br />");
                sb.AppendLine("<span style='font-weight: bold;'>InterFuture Team </span>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
                sb.AppendLine("</table>");
                sb.AppendLine("</div>");

                return sb.ToString();
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        private async Task NotSolvedComplainMailtoTechnicalDept()
        {
            try
            {
                //Mail Message
                MailMessage mM = new MailMessage();

                //Mail Address
                mM.From = new MailAddress("noreply.Interfuture@gmail.com");
                ////////////////For Live////////////////
                mM.To.Add("mahmood@interfuture.ae");
                mM.Bcc.Add("marketing@interfuture.ae");
                mM.Bcc.Add("kimberly@interfuture.ae");
                mM.To.Add("maintenance@interfuture.ae");
                //mM.CC.Add("projects@interfuture.ae");
                mM.CC.Add("arief@interfuture.ae");
                ///////////////////////////////////////
                /////////////For Testing///////////////
                //mM.To.Add("khazi.mohdshafi@gmail.com");
                ////////////////////////////////////////

                ///////////////////////////Own Mail Format//////////////
                mM.Subject = "Complaint No:" + _com_no + ", is attended but not solved";
                string companymailformat = await Task.Run(() => CompanyMailFormat());
                mM.IsBodyHtml = true;
                mM.Body = companymailformat.ToString();
                ///////Customer mail Format///////////
                //mM.Subject = "Your Complaint has been registered. ";
                //string mailformat = CustomerMailFormat();
                // mM.IsBodyHtml = true;
                // mM.Body = mailformat.ToString();
                //////////////////////////////////////
                //SMTP client
                SmtpClient sC = new SmtpClient("smtp.gmail.com");
                //port number for Hot mail
                sC.Port = 587;
                //credentials to login in to hotmail account
                sC.Credentials = new NetworkCredential("noreply.Interfuture@gmail.com", "interfuture123");
                sC.DeliveryMethod = SmtpDeliveryMethod.Network;
                //enabled SSL
                sC.EnableSsl = true;
                //Send an email
                sC.Send(mM);

            }
            catch (Exception)
            {

            }
        }

        private async Task<string> CompanyMailFormat()
        {
            try
            {
                //user = ViewState["user"].ToString();
                //compl = ViewState["complaint"].ToString();
                //compType = ViewState["compType"].ToString();
                //Tel_Phno = ViewState["Tel_Phno"].ToString();
                //recvdBy = ViewState["recvdBy"].ToString();
                ComplainDTO _ComplainDTO = new ComplainDTO();

                _ComplainDTO = await Task.Run(() => (from a in _TimesheetEntities.complaints
                                                     join b in _TimesheetEntities.customers on a.ac_code equals b.AC_CODE
                                                     join c in _TimesheetEntities.complainttypes on new { com_type = a.com_type } equals new { com_type = (System.Data.Entity.SqlServer.SqlFunctions.StringConvert((decimal)c.Id)).Trim() }
                                                     join d in _TimesheetEntities.contacts on ((decimal)a.ac_sub) equals d.ac_sub
                                                     where a.com_no == _com_no
                                                     select new ComplainDTO
                                                     {
                                                         #region
                                                         //complaint
                                                         com_no = a.com_no,
                                                         com_date = a.com_date,//
                                                         com_job = a.com_job,//
                                                         com_name = a.com_name,//
                                                         com_narration = a.com_narration,//
                                                         com_person = a.com_person,//
                                                         com_phone = a.com_phone,//
                                                         com_status = a.com_status,//
                                                         Priority = a.Priority,//
                                                         Job_name = a.Job_name,//
                                                         com_iss_to = a.com_iss_to,
                                                         com_rec_by = a.com_rec_by,

                                                         //Customer
                                                         AC_NAME = b.AC_NAME,
                                                         AC_TELE1 = b.AC_TELE1,
                                                         AC_MOBNO = b.AC_MOBNO,
                                                         AC_EMAIL = b.AC_EMAIL,
                                                         //complainttype
                                                         complainttype_ac_name = c.ac_name,
                                                         //contact

                                                         contact_Telno = d.Telno,
                                                         contact_Empname = d.Empname,
                                                         contact_Mobno = d.Mobno,
                                                         contact_Email = d.Email

                                                         #endregion

                                                     }).FirstOrDefault());

                TimesheetEntities db = new TimesheetEntities();
                var compGroup = (from cn in db.complaintnames
                                 where cn.ac_name == _ComplainDTO.com_name
                                 select cn).FirstOrDefault();
                string comp_Group = compGroup.ac_group.ToString();
                var com_Name = (from comN in db.ac_system
                                where comN.ac_code == comp_Group
                                select comN).FirstOrDefault();

                decimal ac_sub = Convert.ToDecimal(_ComplainDTO.ac_sub);
                var cont = (from con in db.contacts
                            where con.ac_sub == ac_sub
                            select con).FirstOrDefault();
                string Email = cont.Email.ToString();

                StringBuilder sbbody = new StringBuilder();
                sbbody.AppendFormat("<div>");
                sbbody.AppendLine("<table style='width:100%;font-family:Corbel'>");
                sbbody.AppendLine("<tr>");
                sbbody.AppendFormat("<td  style='font-weight:bold'> Dear Mr " + _ComplainDTO.com_iss_to + "\n Please Find Details of the Complaint:</td>");
                sbbody.AppendLine("</tr>");
                sbbody.AppendLine("<tr style='height:10px'><td></td></tr>");
                sbbody.AppendFormat("\r\n");
                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td > <span style='font-weight: bold;'> Complaint No:</span>");
                sbbody.AppendFormat("" + _com_no + "</td>");
                sbbody.AppendLine(" </tr>");

                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td><span style='font-weight: bold;'> Job No:</span>");

                sbbody.AppendFormat("" + _ComplainDTO.com_job + "</td>");
                sbbody.AppendLine(" </tr>");



                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td > <span style='font-weight: bold;'>Customer Name:</span>");
                sbbody.AppendFormat("" + _ComplainDTO.AC_NAME + "</td>");
                sbbody.AppendLine(" </tr>");


                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td ><span style='font-weight: bold;'> Description:</span>");
                sbbody.AppendFormat("" + _ComplainDTO.com_narration + "</td>");
                sbbody.AppendLine(" </tr>");


                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td ><span style='font-weight: bold;'> Complaint:</span>"); //
                sbbody.AppendFormat("" + com_Name.ac_name.ToString() + "</td>");
                sbbody.AppendLine(" </tr>");

                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td > <span style='font-weight: bold;'>Complaint Name:</span>");
                sbbody.AppendFormat("" + _ComplainDTO.com_name + "</td>");
                sbbody.AppendLine(" </tr>");

                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td > <span style='font-weight: bold;'> Complaint Type:</span>");
                sbbody.AppendFormat("" + _ComplainDTO.complainttype_ac_name + "</td>");
                sbbody.AppendLine(" </tr>");

                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td > <span style='font-weight: bold;'>Not Solved Reason:</span>");
                sbbody.AppendFormat("" + _reason + "</td>");
                sbbody.AppendLine(" </tr>");


                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td><span style='font-weight: bold;'> Contact Person:</span>");
                sbbody.AppendFormat("" + _ComplainDTO.com_person + "</td>");
                sbbody.AppendLine(" </tr>");


                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td><span style='font-weight: bold;'> Phone: </span>");
                sbbody.AppendFormat("" + _ComplainDTO.contact_Telno + "</td>");
                sbbody.AppendLine(" </tr>");


                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td ><span style='font-weight: bold;'> Received By: </span>");
                sbbody.AppendFormat("" + _ComplainDTO.com_rec_by + "</td>");
                sbbody.AppendLine("</tr>");

                sbbody.AppendLine("<tr style='height:10px'><td></td></tr>");

                sbbody.AppendLine("<tr>");
                sbbody.AppendLine("<td> This is a system-generated e-mail. Please do not reply. </td>");
                sbbody.AppendLine("</tr>");

                sbbody.AppendLine("</table>");
                sbbody.AppendLine("</div>");

                return sbbody.ToString();
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        #endregion Complain

        #region Others

        public async Task<LabourOtherDetailDTO> getEnquiryTimesheetData(string user_id)
        {
            LabourOtherDetailDTO _LabourOtherDetailDTO = new LabourOtherDetailDTO();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();

            _TimesheetEnqGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                             from tu in ps.DefaultIfEmpty()
                                                             where ts.user_id == user_id
                                                             && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                             && ts.is_other == true
                                                             select new TimesheetEnqGridPostDTO
                                                             {
                                                                 id = ts.id,
                                                                 enq_no = ts.job_no,
                                                                 cust_name = ts.cust_name,
                                                                 department = ts.department,
                                                                 purpose = ts.purpose,
                                                                 checkin = ts.checkin,
                                                                 checkout = ts.checkout,
                                                                 emp_id = ts.emp_id.ToString(),
                                                                 time = ts.time,
                                                                 empname = em.empname,
                                                                 is_checkout = ts.is_checkout,
                                                                 groupid = ts.groupid

                                                             }).OrderByDescending(x => x.checkin).ToList());


            _LabourOtherDetailDTO._TimesheetEnqGridPostDTO = _TimesheetEnqGridPostDTO;



            return _LabourOtherDetailDTO;
        }

        public async Task<LabourOtherDataBindDTO> getAllEnquiry()
        {
            LabourOtherDataBindDTO _LabourOtherDataBindDTO = new LabourOtherDataBindDTO();
            List<enq_mast> _enq_mastList = new List<enq_mast>();
            List<ts_employee> _ts_employee = new List<ts_employee>();
            List<ac_dept> _ac_deptList = new List<ac_dept>();

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

                _enq_mastList = await Task.Run(() => _InterfutureEntities.enq_mast
                                                .Where(x => x.enq_status != "Inactive"
                                                && x.enq_status != "Job Closed"
                                                && x.enq_status != "Quote Declined"
                                                && x.enq_status != "20"
                                                && x.enq_status != "Enquiry Dropped"
                                                && x.enq_status != "Job Opened"
                                                && x.doc_date.Value.Year > 2015)
                                                .OrderByDescending(x => x.doc_date)
                                                .ToList());


                #region

                _ts_employee = await Task.Run(() => _TimesheetEntities.ts_employee
                                                .Where(x => x.workType == 1 && x.Inactive == false && x.jobend == 0 && x.is_outsourced == false)
                                                .OrderBy(x => x.empname)
                                                .ToList());

                _ac_deptList = await Task.Run(() => _TimesheetEntities.ac_dept.ToList());
                _ac_deptList.Add(new ac_dept { Id = 999, ac_name = "Others" });


                _LabourOtherDataBindDTO._ts_employee = _ts_employee;
                _LabourOtherDataBindDTO._enq_mastList = _enq_mastList;
                _LabourOtherDataBindDTO._ac_deptList = _ac_deptList;

                #endregion

            }
            catch (Exception e)
            {
                throw e;
            }

            return _LabourOtherDataBindDTO;
        }

        public async Task<List<activitysub>> getDepartmentActivity(int? Id)
        {
            List<activitysub> _activitysubList = new List<activitysub>();
            try
            {
                _activitysubList = await Task.Run(() => _TimesheetEntities.activitysubs
                                                .Where(x => x.ac_group == Id)
                                                .ToList());
            }
            catch (Exception e)
            {
                throw e;
            }

            return _activitysubList;
        }

        public async Task<List<TimesheetEnqGridPostDTO>> setTimesheetEnqData(TimesheetENQPostDTO _TimesheetENQPostDTO, ts_user _ts_user)
        {
            List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();

            try
            {
                string _groupId = Guid.NewGuid().ToString();
                //interfuture employee
                #region  interfuture employee
                if (_TimesheetENQPostDTO.Team.Count > 0)
                {
                    for (int i = 0; i < _TimesheetENQPostDTO.Team.Count; i++)
                    {
                        if (_TimesheetENQPostDTO.Team[i] != "")
                        {
                            ts_timesheet _ts_timesheet_enq = new ts_timesheet();
                            _ts_timesheet_enq.id = Guid.NewGuid().ToString();
                            _ts_timesheet_enq.user_id = _ts_user.id;
                            _ts_timesheet_enq.emp_id = Convert.ToInt32(_TimesheetENQPostDTO.Team[i]);
                            _ts_timesheet_enq.job_no = _TimesheetENQPostDTO.EnquiryNo;
                            _ts_timesheet_enq.cust_name = _TimesheetENQPostDTO.Customer;
                            _ts_timesheet_enq.project_name = _TimesheetENQPostDTO.ProjectName;
                            _ts_timesheet_enq.department = _TimesheetENQPostDTO.Department;
                            _ts_timesheet_enq.purpose = _TimesheetENQPostDTO.Purpose;
                            _ts_timesheet_enq.checkin = _TimesheetENQPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                            //DateTime _checkout = Convert.ToDateTime("05:00 PM");
                            _ts_timesheet_enq.checkout = null;// _checkout.ToString("HH:mm tt", CultureInfo.InvariantCulture);
                            _ts_timesheet_enq.time = null;
                            _ts_timesheet_enq.place = null;
                            _ts_timesheet_enq.is_checkout = false;
                            _ts_timesheet_enq.is_other = true;

                            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                            var yourTime1 = TimeZoneInfo.ConvertTime(currentDate, cet);

                            _ts_timesheet_enq.ondate_created = yourTime1;
                            _ts_timesheet_enq.createdby = _ts_user.name;
                            _ts_timesheet_enq.groupid = _groupId;

                            _TimesheetEntities.ts_timesheet.Add(_ts_timesheet_enq);
                            _TimesheetEntities.SaveChanges();
                        }
                    }
                }
                #endregion  interfuture employee

                //login user
                #region login user
                ts_timesheet _ts_timesheet_enq_user_log = new ts_timesheet();
                _ts_timesheet_enq_user_log.id = Guid.NewGuid().ToString();
                _ts_timesheet_enq_user_log.user_id = _ts_user.id;
                _ts_timesheet_enq_user_log.emp_id = _ts_user.emp_id;
                _ts_timesheet_enq_user_log.job_no = _TimesheetENQPostDTO.EnquiryNo;
                _ts_timesheet_enq_user_log.cust_name = _TimesheetENQPostDTO.Customer;
                _ts_timesheet_enq_user_log.project_name = _TimesheetENQPostDTO.ProjectName;
                _ts_timesheet_enq_user_log.department = _TimesheetENQPostDTO.Department;
                _ts_timesheet_enq_user_log.purpose = _TimesheetENQPostDTO.Purpose;
                _ts_timesheet_enq_user_log.checkin = _TimesheetENQPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                //DateTime _xcheckout = Convert.ToDateTime("05:00 PM");
                _ts_timesheet_enq_user_log.checkout = null;// _xcheckout.ToString("HH:mm tt", CultureInfo.InvariantCulture);
                _ts_timesheet_enq_user_log.time = null;
                _ts_timesheet_enq_user_log.place = null;
                _ts_timesheet_enq_user_log.is_checkout = false;
                _ts_timesheet_enq_user_log.is_other = true;


                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

                _ts_timesheet_enq_user_log.ondate_created = swedishTime;
                _ts_timesheet_enq_user_log.createdby = _ts_user.name;
                _ts_timesheet_enq_user_log.groupid = _groupId;

                _TimesheetEntities.ts_timesheet.Add(_ts_timesheet_enq_user_log);
                _TimesheetEntities.SaveChanges();

                #endregion login user

                //location
                ts_place _ts_place = new ts_place();
                _ts_place.ts_timesheet_groupid = _groupId;
                _ts_place.latlang = _TimesheetENQPostDTO.location.latlng;
                _ts_place.name = _TimesheetENQPostDTO.location.name;
                _ts_place.formatted_address = _TimesheetENQPostDTO.location.formatted_address;

                _TimesheetEntities.ts_place.Add(_ts_place);
                _TimesheetEntities.SaveChanges();

                //Return Value
                #region ReturnValue

                _TimesheetEnqGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                 join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                 join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                 from tu in ps.DefaultIfEmpty()
                                                                 where ts.user_id == _ts_user.id
                                                                 && ts.is_other == true
                                                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                                 select new TimesheetEnqGridPostDTO
                                                                 {
                                                                     id = ts.id,
                                                                     enq_no = ts.job_no,
                                                                     cust_name = ts.cust_name,
                                                                     department = ts.department,
                                                                     purpose = ts.purpose,
                                                                     checkin = ts.checkin,
                                                                     checkout = ts.checkout,
                                                                     emp_id = ts.emp_id.ToString(),
                                                                     time = ts.time,
                                                                     empname = em.Firstname,
                                                                     is_checkout = ts.is_checkout,
                                                                     groupid = ts.groupid

                                                                 }).OrderByDescending(x => x.checkin).ToList());
                #endregion ReturnValue

            }
            catch (Exception e)
            {
                ////throw e;
                //_TimesheetGridDTO[0].empname = e.ToString();
            }

            return _TimesheetEnqGridPostDTO;
        }


        public async Task<List<TimesheetEnqGridPostDTO>> IndividualOtherCheckOut(ts_user _ts_user, int? emp_id, string endtime)
        {
            List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

            try
            {

                var eQuery = (from ts in _TimesheetEntities.ts_timesheet
                              where ts.emp_id == emp_id
                              && ts.is_checkout == false
                              && ts.is_other == true
                              && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                              select new { ts }).FirstOrDefault();


                if (eQuery != null)
                {
                    TimeSpan duration = DateTime.Parse(endtime).Subtract(DateTime.Parse(eQuery.ts.checkin));

                    eQuery.ts.is_checkout = true;
                    eQuery.ts.checkout = endtime;
                    eQuery.ts.time = duration.ToString(@"hh\:mm");

                    _TimesheetEntities.SaveChanges();
                }

                _TimesheetEnqGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                 join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                 join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                 from tu in ps.DefaultIfEmpty()
                                                                 where ts.user_id == _ts_user.id
                                                                  && ts.is_other == true
                                                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                                                                 select new TimesheetEnqGridPostDTO
                                                                 {
                                                                     id = ts.id,
                                                                     enq_no = ts.job_no,
                                                                     cust_name = ts.cust_name,
                                                                     department = ts.department,
                                                                     purpose = ts.purpose,
                                                                     checkin = ts.checkin,
                                                                     checkout = ts.checkout,
                                                                     emp_id = ts.emp_id.ToString(),
                                                                     time = ts.time,
                                                                     empname = em.Firstname,
                                                                     is_checkout = ts.is_checkout,
                                                                     groupid = ts.groupid

                                                                 }).OrderByDescending(x => x.checkin).ToList());

            }
            catch (Exception e)
            {

            }

            return _TimesheetEnqGridPostDTO;
        }

        public async Task<List<TimesheetEnqGridPostDTO>> AllOtherCheckOut(ts_user _ts_user, string endtime)
        {
            List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var yourTime = TimeZoneInfo.ConvertTime(currentDate, cet);

            try
            {
                var eQuery = (from ts in _TimesheetEntities.ts_timesheet
                              where ts.user_id == _ts_user.id
                              && ts.is_checkout == false
                              && ts.is_other == true
                              && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                              select new { ts }).ToList();

                foreach (var value in eQuery)
                {
                    TimeSpan duration = DateTime.Parse(endtime).Subtract(DateTime.Parse(value.ts.checkin));
                    value.ts.is_checkout = true;
                    value.ts.checkout = endtime;
                    value.ts.time = duration.ToString(@"hh\:mm");
                    _TimesheetEntities.SaveChanges();
                }

                _TimesheetEnqGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                 join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                 join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                 from tu in ps.DefaultIfEmpty()
                                                                 where ts.user_id == _ts_user.id
                                                                  && ts.is_other == true
                                                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(yourTime)
                                                                 select new TimesheetEnqGridPostDTO
                                                                 {
                                                                     id = ts.id,
                                                                     enq_no = ts.job_no,
                                                                     cust_name = ts.cust_name,
                                                                     department = ts.department,
                                                                     purpose = ts.purpose,
                                                                     checkin = ts.checkin,
                                                                     checkout = ts.checkout,
                                                                     emp_id = ts.emp_id.ToString(),
                                                                     time = ts.time,
                                                                     empname = em.Firstname,
                                                                     is_checkout = ts.is_checkout,
                                                                     groupid = ts.groupid

                                                                 }).OrderByDescending(x => x.checkin).ToList());

            }
            catch (Exception e)
            {

            }

            return _TimesheetEnqGridPostDTO;
        }

        public async Task<ts_timesheet> getActivityDetailsForOther(string enq_no, string groupid)
        {
            ts_timesheet _ts_timesheet_enq = new ts_timesheet();
            _ts_timesheet_enq = await Task.Run(() => _TimesheetEntities.ts_timesheet.Where(e => e.job_no == enq_no && e.groupid == groupid).FirstOrDefault());
            return _ts_timesheet_enq;
        }

        public async Task<ts_timesheet> setActivityDetailsForOther(ts_timesheet _ts_timesheet_enq)
        {
            TimesheetEntities _TimesheetEntities = new TimesheetEntities();
            ts_timesheet _obj_ts_timesheet_enq = new ts_timesheet();
            _obj_ts_timesheet_enq = await Task.Run(() => _TimesheetEntities.ts_timesheet
                                                    .Where(e => e.job_no == _ts_timesheet_enq.job_no && e.groupid == _ts_timesheet_enq.groupid)
                                                    .FirstOrDefault());

            _obj_ts_timesheet_enq.remarks = _ts_timesheet_enq.remarks;
            _TimesheetEntities.SaveChanges();
            return _obj_ts_timesheet_enq;
        }

        #endregion Others

        #region Attandance Dashboard

        public List<TimesheetGridDTO> getTimesheetAllData()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();
            //Attandance
            var query = (from ts in _TimesheetEntities.ts_timesheet
                         join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                         join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                         from tu in ps.DefaultIfEmpty()
                         where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                         select new TimesheetGridDTO
                         {
                             id = ts.id,
                             job_no = ts.job_no,
                             date = ts.ondate_created,
                             checkin = ts.checkin,
                             checkout = ts.checkout,
                             emp_id = ts.emp_id,
                             Time = ts.time,
                             empname = em.Firstname,
                             is_checkout = ts.is_checkout,
                             groupid = ts.groupid

                         }).OrderByDescending(x => x.checkin).ToList();

            return _TimesheetGridDTO = query;
        }

        public List<TimesheetGridDTO> DashboardAttandanceForProjectEngineers(DateTime fromdate, DateTime todate, string Employee)
        {
            List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();

            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);

            try
            {
                if (Employee != null && Employee != "")
                {
                    int _Employee = Convert.ToInt32(Employee);

                    var query = (from ts in _TimesheetEntities.ts_timesheet
                                 join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                 where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(_fromdate)
                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(_todate)
                                && (ts.emp_id == _Employee)
                                && (ts.ondate_created != null)
                                 select new TimesheetGridDTO
                                 {
                                     id = ts.id,
                                     job_no = ts.job_no,
                                     date = ts.ondate_created,
                                     checkin = ts.checkin,
                                     checkout = ts.checkout,
                                     emp_id = ts.emp_id,
                                     Time = ts.time,
                                     empname = em.Firstname,
                                     is_checkout = ts.is_checkout,
                                     groupid = ts.groupid

                                 }).OrderBy(x => x.date).ToList();
                    _TimesheetGridDTO = query;

                }
                else
                {
                    var query = (from ts in _TimesheetEntities.ts_timesheet
                                 join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                 where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(_fromdate)
                                 && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(_todate)
                                 && (ts.ondate_created != null)
                                 select new TimesheetGridDTO
                                 {
                                     id = ts.id,
                                     job_no = ts.job_no,
                                     date = ts.ondate_created,
                                     checkin = ts.checkin,
                                     checkout = ts.checkout,
                                     emp_id = ts.emp_id,
                                     Time = ts.time,
                                     empname = em.Firstname,
                                     is_checkout = ts.is_checkout,
                                     groupid = ts.groupid

                                 }).OrderBy(x => x.date).ToList();
                    _TimesheetGridDTO = query;
                }

            }
            catch (Exception e)
            {
                //_joblabwork.Message = e.Message.ToString();
            }
            return _TimesheetGridDTO;
        }

        #endregion Attandance Dashboard

        #region Dashboard Complain
        public async Task<List<TimesheetComGridPostDTO>> getComplaintDashboard(DateTime fromdate, DateTime todate, string jobno)
        {
            List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO = new List<TimesheetComGridPostDTO>();
            _TimesheetComGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                             join cm in _TimesheetEntities.complaints on ts.job_no equals cm.com_no
                                                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                             from tu in ps.DefaultIfEmpty()
                                                             where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate)
                                                             && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate)
                                                             && ts.is_complain == true
                                                             select new TimesheetComGridPostDTO
                                                             {
                                                                 id = ts.id,
                                                                 com_no = ts.job_no,
                                                                 checkin = ts.checkin,
                                                                 checkout = ts.checkout,
                                                                 date = ts.ondate_created,
                                                                 remarks = cm.remarks,
                                                                 emp_id = ts.emp_id.ToString(),
                                                                 time = ts.time,
                                                                 empname = em.Firstname,
                                                                 is_checkout = ts.is_checkout,
                                                                 groupid = ts.groupid

                                                             }).OrderByDescending(x => x.checkin).ToList());
            return _TimesheetComGridPostDTO;
        }

        public async Task<List<TimesheetComGridPostDTO>> DashboardComplainForProjectEngineers(DateTime fromdate, DateTime todate, string com_no)
        {
            List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO = new List<TimesheetComGridPostDTO>();

            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);

            try
            {
                if (com_no != null && com_no != "")
                {
                    _TimesheetComGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                     join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                     join cm in _TimesheetEntities.complaints on ts.job_no.Trim() equals cm.com_no.Trim()
                                                                     join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                     from tu in ps.DefaultIfEmpty()
                                                                     where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate)
                                                                     && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate)
                                                                     && ts.job_no == com_no
                                                                     && ts.is_complain == true
                                                                     select new TimesheetComGridPostDTO
                                                                     {
                                                                         id = ts.id,
                                                                         com_no = ts.job_no,
                                                                         checkin = ts.checkin,
                                                                         checkout = ts.checkout,
                                                                         date = ts.ondate_created,
                                                                         remarks = cm.remarks,
                                                                         emp_id = ts.emp_id.ToString(),
                                                                         time = ts.time,
                                                                         empname = em.Firstname,
                                                                         is_checkout = ts.is_checkout,
                                                                         groupid = ts.groupid

                                                                     }).OrderBy(x => x.date).ToList());
                }
                else
                {

                    _TimesheetComGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                     join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                     join cm in _TimesheetEntities.complaints on ts.job_no.Trim() equals cm.com_no.Trim()
                                                                     join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                     from tu in ps.DefaultIfEmpty()
                                                                     where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate)
                                                                     && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate)
                                                                     && ts.is_complain == true
                                                                     select new TimesheetComGridPostDTO
                                                                     {
                                                                         id = ts.id,
                                                                         com_no = ts.job_no,
                                                                         checkin = ts.checkin,
                                                                         checkout = ts.checkout,
                                                                         date = ts.ondate_created,
                                                                         remarks = cm.remarks,
                                                                         emp_id = ts.emp_id.ToString(),
                                                                         time = ts.time,
                                                                         empname = em.Firstname,
                                                                         is_checkout = ts.is_checkout,
                                                                         groupid = ts.groupid

                                                                     }).OrderBy(x => x.date).ToList());

                }
            }
            catch (Exception e)
            {
                //_joblabwork.Message = e.Message.ToString();
            }
            return _TimesheetComGridPostDTO;
        }
        #endregion Dashboard Complain

        #region Dashboard Other

        public async Task<LabourOtherDetailDTO> getOtherDashboardData(DateTime fromdate, DateTime todate, string jobno)
        {
            LabourOtherDetailDTO _LabourOtherDetailDTO = new LabourOtherDetailDTO();

            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);

            List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();

            _TimesheetEnqGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                             from tu in ps.DefaultIfEmpty()
                                                             where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate)
                                                             && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate)
                                                             && ts.is_other == true
                                                             select new TimesheetEnqGridPostDTO
                                                             {
                                                                 id = ts.id,
                                                                 enq_no = ts.job_no,
                                                                 cust_name = ts.cust_name,
                                                                 department = ts.department,
                                                                 purpose = ts.purpose,
                                                                 checkin = ts.checkin,
                                                                 checkout = ts.checkout,
                                                                 emp_id = ts.emp_id.ToString(),
                                                                 time = ts.time,
                                                                 date = ts.ondate_created,
                                                                 remarks = ts.remarks,
                                                                 empname = em.empname,
                                                                 is_checkout = ts.is_checkout,
                                                                 groupid = ts.groupid

                                                             }).OrderByDescending(x => x.checkin).ToList());


            _LabourOtherDetailDTO._TimesheetEnqGridPostDTO = _TimesheetEnqGridPostDTO;
            return _LabourOtherDetailDTO;
        }

        public async Task<List<TimesheetEnqGridPostDTO>> DashboardOtherForProjectEngineers(DateTime fromdate, DateTime todate, string com_no)
        {
            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);

            List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();

            try
            {
                if (com_no != null && com_no != "")
                {
                    _TimesheetEnqGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                     join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                     join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                     from tu in ps.DefaultIfEmpty()
                                                                     where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate)
                                                                     && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate)
                                                                     && ts.job_no == com_no
                                                                     && ts.is_other == true
                                                                     select new TimesheetEnqGridPostDTO
                                                                     {
                                                                         id = ts.id,
                                                                         enq_no = ts.job_no,
                                                                         cust_name = ts.cust_name,
                                                                         department = ts.department,
                                                                         purpose = ts.purpose,
                                                                         checkin = ts.checkin,
                                                                         checkout = ts.checkout,
                                                                         emp_id = ts.emp_id.ToString(),
                                                                         time = ts.time,
                                                                         date = ts.ondate_created,
                                                                         remarks = ts.remarks,
                                                                         empname = em.empname,
                                                                         is_checkout = ts.is_checkout,
                                                                         groupid = ts.groupid

                                                                     }).OrderByDescending(x => x.checkin).ToList());

                }
                else
                {

                    _TimesheetEnqGridPostDTO = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                                     join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                                     join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                                     from tu in ps.DefaultIfEmpty()
                                                                     where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate)
                                                                     && System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate)
                                                                     && ts.is_other == true
                                                                     select new TimesheetEnqGridPostDTO
                                                                     {
                                                                         id = ts.id,
                                                                         enq_no = ts.job_no,
                                                                         cust_name = ts.cust_name,
                                                                         department = ts.department,
                                                                         purpose = ts.purpose,
                                                                         checkin = ts.checkin,
                                                                         checkout = ts.checkout,
                                                                         emp_id = ts.emp_id.ToString(),
                                                                         time = ts.time,
                                                                         date = ts.ondate_created,
                                                                         remarks = ts.remarks,
                                                                         empname = em.empname,
                                                                         is_checkout = ts.is_checkout,
                                                                         groupid = ts.groupid

                                                                     }).OrderByDescending(x => x.checkin).ToList());

                }
            }
            catch (Exception e)
            {
                //_joblabwork.Message = e.Message.ToString();
            }
            return _TimesheetEnqGridPostDTO;
        }

        #endregion Dashboard Other

        #region Dashboard Exception

        public List<ExceptionModelDTO> getExceptionAttendance()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now.AddDays(0), DateTimeKind.Local);
            var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            List<ExceptionModelDTO> _ExceptionModelDTOList = new List<ExceptionModelDTO>();

            var query = @"select distinct( ts_employee.firstname),
                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                        from [intersa].[ts_timesheet]  
                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + swedishTime + "', 3) " +
                        "group by  ts_employee.firstname " +
                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) < 32400)";


            DataTable _DataTable = GetDataTable(query);

            for (int i = 0; i < _DataTable.Rows.Count; i++)
            {
                string _firstname = _DataTable.Rows[i]["firstname"].ToString();

                TimesheetGridDTO _obj = new TimesheetGridDTO();
                _obj.Time = _DataTable.Rows[i]["time"].ToString();
                _obj.empname = _DataTable.Rows[i]["firstname"].ToString();

                var xquery = (from ts in _TimesheetEntities.ts_timesheet
                              join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                              join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                              from tu in ps.DefaultIfEmpty()
                              where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                              && em.Firstname == _firstname
                              select new TimesheetGridDTO
                              {
                                  id = ts.id,
                                  job_no = ts.job_no,
                                  date = ts.ondate_created,
                                  checkin = ts.checkin,
                                  checkout = ts.checkout,
                                  emp_id = ts.emp_id,
                                  Time = ts.time,
                                  empname = em.Firstname,
                                  is_checkout = ts.is_checkout,
                                  groupid = ts.groupid

                              }).OrderByDescending(x => x.checkin).ToList();

                _ExceptionModelDTOList.Add(new ExceptionModelDTO { _TimesheetGridDTO = _obj, _TimesheetGridDTOList = xquery });
            }

            return _ExceptionModelDTOList;

        }

        public List<ExceptionModelDTO> DashboardExceptionForProjectEngineers(DateTime fromdate, DateTime todate, string jobno)
        {
            List<ExceptionModelDTO> _ExceptionModelDTOList = new List<ExceptionModelDTO>();
            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);

            try
            {
                if (jobno != null && jobno != "")
                {
                    var dates = new List<DateTime>();
                    for (var dt = _fromdate; dt <= todate; dt = dt.AddDays(1))
                    {
                        dates.Add(dt);
                    }

                    for (int c = 0; c < dates.Count; c++)
                    {
                        var datedd = dates[c].Date;

                        var query = @"select distinct( ts_employee.firstname),
                                RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                                RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                                from [intersa].[ts_timesheet]  
                                inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                                where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + dates[c].Date + "', 3) " +
                                "and ts_timesheet.job_no ='" + jobno + "' " +
                                "group by  ts_employee.firstname " +
                                "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) < 32400)";

                        DataTable _DataTable = GetDataTable(query);

                        for (int i = 0; i < _DataTable.Rows.Count; i++)
                        {
                            string _firstname = _DataTable.Rows[i]["firstname"].ToString();

                            TimesheetGridDTO _obj = new TimesheetGridDTO();
                            _obj.Time = _DataTable.Rows[i]["time"].ToString();
                            _obj.empname = _DataTable.Rows[i]["firstname"].ToString();

                            var xquery = (from ts in _TimesheetEntities.ts_timesheet
                                          join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                          join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                          from tu in ps.DefaultIfEmpty()
                                          where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                          && em.Firstname == _firstname
                                          select new TimesheetGridDTO
                                          {
                                              id = ts.id,
                                              job_no = ts.job_no,
                                              date = ts.ondate_created,
                                              checkin = ts.checkin,
                                              checkout = ts.checkout,
                                              emp_id = ts.emp_id,
                                              Time = ts.time,
                                              empname = em.Firstname,
                                              is_checkout = ts.is_checkout,
                                              groupid = ts.groupid

                                          }).OrderByDescending(x => x.checkin).ToList();

                            _ExceptionModelDTOList.Add(new ExceptionModelDTO { _TimesheetGridDTO = _obj, _TimesheetGridDTOList = xquery });

                        }
                    }
                }
                else
                {
                    var dates = new List<DateTime>();
                    for (var dt = _fromdate; dt <= todate; dt = dt.AddDays(1))
                    {
                        dates.Add(dt);
                    }

                    for (int c = 0; c < dates.Count; c++)
                    {
                        var datedd = dates[c].Date;

                        var query = @"select distinct( ts_employee.firstname),
                                RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                                RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                                from [intersa].[ts_timesheet]  
                                inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                                where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + dates[c].Date + "', 3) " +
                                    //                                where  CONVERT(VARCHAR(10), ondate_created, 3) >=  CONVERT(VARCHAR(9), '" + _fromdate + "', 3) " +
                                    //                                "and CONVERT(VARCHAR(10), ondate_created, 3) <= CONVERT(VARCHAR(9), '" + _todate + "', 3) " +
                                    "group by  ts_employee.firstname " +
                                    "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) < 32400)";

                        DataTable _DataTable = GetDataTable(query);

                        for (int i = 0; i < _DataTable.Rows.Count; i++)
                        {
                            string _firstname = _DataTable.Rows[i]["firstname"].ToString();

                            TimesheetGridDTO _obj = new TimesheetGridDTO();
                            _obj.Time = _DataTable.Rows[i]["time"].ToString();
                            _obj.empname = _DataTable.Rows[i]["firstname"].ToString();

                            var xquery = (from ts in _TimesheetEntities.ts_timesheet
                                          join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                          join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                          from tu in ps.DefaultIfEmpty()
                                          where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                          && em.Firstname == _firstname
                                          select new TimesheetGridDTO
                                          {
                                              id = ts.id,
                                              job_no = ts.job_no,
                                              date = ts.ondate_created,
                                              checkin = ts.checkin,
                                              checkout = ts.checkout,
                                              emp_id = ts.emp_id,
                                              Time = ts.time,
                                              empname = em.Firstname,
                                              is_checkout = ts.is_checkout,
                                              groupid = ts.groupid

                                          }).OrderByDescending(x => x.checkin).ToList();

                            _ExceptionModelDTOList.Add(new ExceptionModelDTO { _TimesheetGridDTO = _obj, _TimesheetGridDTOList = xquery });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //_joblabwork.Message = e.Message.ToString();
            }
            return _ExceptionModelDTOList;
        }

        public List<TimesheetExceptionDTO> LaborWorkedLessThanNineHourExceptionGrid()
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            List<TimesheetExceptionDTO> _objList = new List<TimesheetExceptionDTO>();

            var singleJob = @"select distinct( ts_employee.firstname),COUNT(*) ,
                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                        from [intersa].[ts_timesheet]  
                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + swedishTime + "', 3) " +
                        "group by  ts_employee.firstname " +
                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) < 32400) and COUNT(*) = 1";


            DataTable _xDataTable = GetDataTable(singleJob);
            if (_xDataTable.Rows.Count > 0)
            {
                for (int i = 0; i < _xDataTable.Rows.Count; i++)
                {
                    TimesheetExceptionDTO _obj = new TimesheetExceptionDTO();
                    string _firstname = _xDataTable.Rows[i]["firstname"].ToString();
                    _obj = (from ts in _TimesheetEntities.ts_timesheet
                            join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                            join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                            from tu in ps.DefaultIfEmpty()
                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                            && em.Firstname == _firstname
                            select new TimesheetExceptionDTO
                            {
                                id = ts.id,
                                job_no = ts.job_no,
                                date = ts.ondate_created,
                                checkin = ts.checkin,
                                checkout = ts.checkout,
                                traveltime = "00:00 hrs",
                                emp_id = ts.emp_id,
                                Time = ts.time + " hrs",
                                empname = em.Firstname,
                                is_checkout = ts.is_checkout,
                                groupid = ts.groupid

                            }).OrderByDescending(x => x.checkin).FirstOrDefault();

                    _objList.Add(_obj);
                }
            }

            //with multiple jobs 
            var Exception = @"SELECT emp_id , COUNT(*) FROM
                            ts_timesheet  where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + swedishTime + "', 3) " +
                            "GROUP BY emp_id HAVING COUNT(*) > 1";

            DataTable _DataTable = GetDataTable(Exception);
            if (_DataTable.Rows.Count > 0)
            {
                for (int p = 0; p < _DataTable.Rows.Count; p++)
                {
                    int? empid = Convert.ToInt32(_DataTable.Rows[p]["emp_id"]);

                    List<TimesheetExceptionDTO> _objTimesheetGridDTOList = new List<TimesheetExceptionDTO>();

                    _objTimesheetGridDTOList = (from ts in _TimesheetEntities.ts_timesheet
                                                join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                from tu in ps.DefaultIfEmpty()
                                                where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                && ts.emp_id == empid
                                                select new TimesheetExceptionDTO
                                                {
                                                    id = ts.id,
                                                    job_no = ts.job_no,
                                                    date = ts.ondate_created,
                                                    checkin = ts.checkin,
                                                    checkout = ts.checkout,
                                                    emp_id = ts.emp_id,
                                                    Time = ts.time,
                                                    empname = em.Firstname,
                                                    is_checkout = ts.is_checkout,
                                                    groupid = ts.groupid

                                                }).OrderBy(x => x.checkin).ToList();

                    string firstCheckin = "";
                    string lastCheckout = "";
                    string lastJob = "";
                    TimeSpan duration = TimeSpan.Zero;
                    TimeSpan xduration = TimeSpan.Zero;
                    TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                    for (int b = 0; b < _objTimesheetGridDTOList.Count; b++)
                    {
                        if (_objTimesheetGridDTOList[b].Time == null || _objTimesheetGridDTOList[b].Time == "00:00")
                        {
                            _objTimesheetGridDTOList[b].Time = TimeSpan.Zero.ToString();
                            TotalWorkedHrs += TimeSpan.Zero;
                        }
                        else
                        {
                            TotalWorkedHrs += new TimeSpan(DateTime.Parse(_objTimesheetGridDTOList[b].Time).Ticks - TimeSpan.Zero.Ticks);
                        }
                        if (b == 0)
                        {
                            firstCheckin = _objTimesheetGridDTOList[b].checkin;
                        }

                        if (b == _objTimesheetGridDTOList.Count - 1)
                        {
                            if (_objTimesheetGridDTOList[b].checkout == null)
                            {
                                _objTimesheetGridDTOList[b].checkout = TimeSpan.Zero.ToString();
                                lastJob = _objTimesheetGridDTOList[b].job_no;
                                lastCheckout = "";
                                duration = TimeSpan.Zero;
                                xduration = TimeSpan.Zero;
                            }
                            else
                            {
                                lastCheckout = _objTimesheetGridDTOList[b].checkout;
                                lastJob = _objTimesheetGridDTOList[b].job_no;

                                //total Hours in The Site
                                duration = DateTime.Parse(lastCheckout).Subtract(DateTime.Parse(firstCheckin));

                                //Total Duration Transport
                                xduration = DateTime.Parse(duration.ToString()).Subtract(DateTime.Parse(TotalWorkedHrs.ToString(@"hh\:mm")));
                            }

                            TimesheetExceptionDTO xTimesheetGridDTO = new TimesheetExceptionDTO();
                            xTimesheetGridDTO.id = _objTimesheetGridDTOList[b].id;
                            xTimesheetGridDTO.job_no = lastJob;
                            xTimesheetGridDTO.date = _objTimesheetGridDTOList[b].date;
                            xTimesheetGridDTO.checkin = string.Format("{0:h:mm tt}", Convert.ToDateTime(firstCheckin));
                            if (lastCheckout != null && lastCheckout != "")
                            {
                                xTimesheetGridDTO.checkout = string.Format("{0:h:mm tt}", Convert.ToDateTime(lastCheckout));
                            }
                            xTimesheetGridDTO.checkout = lastCheckout;
                            xTimesheetGridDTO.traveltime = xduration.ToString(@"hh\:mm") + " hrs";
                            xTimesheetGridDTO.emp_id = _objTimesheetGridDTOList[b].emp_id;
                            xTimesheetGridDTO.Time = TotalWorkedHrs.ToString(@"hh\:mm") + " hrs";
                            xTimesheetGridDTO.empname = _objTimesheetGridDTOList[b].empname;
                            xTimesheetGridDTO.is_checkout = _objTimesheetGridDTOList[b].is_checkout;
                            xTimesheetGridDTO.groupid = _objTimesheetGridDTOList[b].groupid;

                            TimeSpan t1 = TimeSpan.FromHours(9);

                            if (t1 > duration)
                            {
                                _objList.Add(xTimesheetGridDTO);
                            }

                        }
                    }
                }
            }

            return _objList;

        }

        //filter
        public List<TimesheetExceptionDTO> DashboardLaborWorkedLessThanNineHourExceptionGrid(DateTime fromdate, DateTime todate, string Employee)
        {
            List<TimesheetExceptionDTO> _objList = new List<TimesheetExceptionDTO>();
            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);

            try
            {

                var dates = new List<DateTime>();
                for (var dt = _fromdate; dt <= todate; dt = dt.AddDays(1))
                {
                    dates.Add(dt);
                }

                for (int c = 0; c < dates.Count; c++)
                {
                    var datedd = dates[c].Date;
                    if (Employee != null && Employee != "")
                    {
                        #region withemployee

                        var singleJob = @"select distinct( ts_employee.firstname),COUNT(*) ,
                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                        from [intersa].[ts_timesheet]  
                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + datedd + "', 3) " +
                        "and  [ts_timesheet] .emp_id='" + Employee + "' " +
                        "group by  ts_employee.firstname " +
                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) < 32400) and COUNT(*) = 1";

                        DataTable _xDataTable = GetDataTable(singleJob);
                        if (_xDataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < _xDataTable.Rows.Count; i++)
                            {
                                TimesheetExceptionDTO _obj = new TimesheetExceptionDTO();
                                string _firstname = _xDataTable.Rows[i]["firstname"].ToString();
                                _obj = (from ts in _TimesheetEntities.ts_timesheet
                                        join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                        join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                        from tu in ps.DefaultIfEmpty()
                                        where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                        && em.Firstname == _firstname
                                        select new TimesheetExceptionDTO
                                        {
                                            id = ts.id,
                                            job_no = ts.job_no,
                                            date = ts.ondate_created,
                                            checkin = ts.checkin,
                                            checkout = ts.checkout,
                                            traveltime = "00:00 hrs",
                                            emp_id = ts.emp_id,
                                            Time = ts.time,
                                            empname = em.Firstname,
                                            is_checkout = ts.is_checkout,
                                            groupid = ts.groupid

                                        }).OrderByDescending(x => x.checkin).FirstOrDefault();

                                _objList.Add(_obj);
                            }
                        }

                        //with multiple jobs 
                        var Exception = @"SELECT emp_id , COUNT(*) FROM
                                        ts_timesheet  where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + datedd + "', 3) " +
                                        "and  [ts_timesheet] .emp_id='" + Employee + "' " +
                                        "GROUP BY emp_id HAVING COUNT(*) > 1";

                        DataTable _DataTable = GetDataTable(Exception);
                        if (_DataTable.Rows.Count > 0)
                        {
                            for (int p = 0; p < _DataTable.Rows.Count; p++)
                            {
                                int? empid = Convert.ToInt32(_DataTable.Rows[p]["emp_id"]);

                                List<TimesheetExceptionDTO> _objTimesheetGridDTOList = new List<TimesheetExceptionDTO>();

                                _objTimesheetGridDTOList = (from ts in _TimesheetEntities.ts_timesheet
                                                            join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                            join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                            from tu in ps.DefaultIfEmpty()
                                                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                                            && ts.emp_id == empid
                                                            select new TimesheetExceptionDTO
                                                            {
                                                                id = ts.id,
                                                                job_no = ts.job_no,
                                                                date = ts.ondate_created,
                                                                checkin = ts.checkin,
                                                                checkout = ts.checkout,
                                                                emp_id = ts.emp_id,
                                                                Time = ts.time,
                                                                empname = em.Firstname,
                                                                is_checkout = ts.is_checkout,
                                                                groupid = ts.groupid

                                                            }).OrderBy(x => x.checkin).ToList();


                                string firstCheckin = "";
                                string lastCheckout = "";
                                string lastJob = "";
                                TimeSpan duration = TimeSpan.Zero;
                                TimeSpan xduration = TimeSpan.Zero;
                                TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                                for (int b = 0; b < _objTimesheetGridDTOList.Count; b++)
                                {
                                    if (_objTimesheetGridDTOList[b].Time == null || _objTimesheetGridDTOList[b].Time == "00:00")
                                    {
                                        _objTimesheetGridDTOList[b].Time = TimeSpan.Zero.ToString();
                                        TotalWorkedHrs += TimeSpan.Zero;
                                    }
                                    else
                                    {
                                        TotalWorkedHrs += new TimeSpan(DateTime.Parse(_objTimesheetGridDTOList[b].Time).Ticks - TimeSpan.Zero.Ticks);
                                    }

                                    if (b == 0)
                                    {
                                        firstCheckin = _objTimesheetGridDTOList[b].checkin;
                                    }

                                    if (b == _objTimesheetGridDTOList.Count - 1)
                                    {
                                        if (_objTimesheetGridDTOList[b].checkout == null)
                                        {
                                            _objTimesheetGridDTOList[b].checkout = TimeSpan.Zero.ToString();
                                            lastJob = _objTimesheetGridDTOList[b].job_no;
                                            lastCheckout = "";
                                            duration = TimeSpan.Zero;
                                            xduration = TimeSpan.Zero;
                                        }
                                        else
                                        {
                                            lastCheckout = _objTimesheetGridDTOList[b].checkout;
                                            lastJob = _objTimesheetGridDTOList[b].job_no;

                                            //total Hours in The Site
                                            duration = DateTime.Parse(lastCheckout).Subtract(DateTime.Parse(firstCheckin));

                                            //Total Duration Transport
                                            xduration = DateTime.Parse(duration.ToString()).Subtract(DateTime.Parse(TotalWorkedHrs.ToString(@"hh\:mm")));
                                        }

                                        TimesheetExceptionDTO xTimesheetGridDTO = new TimesheetExceptionDTO();
                                        xTimesheetGridDTO.id = _objTimesheetGridDTOList[b].id;
                                        xTimesheetGridDTO.job_no = lastJob;
                                        xTimesheetGridDTO.date = _objTimesheetGridDTOList[b].date;
                                        xTimesheetGridDTO.checkin = string.Format("{0:h:mm tt}", Convert.ToDateTime(firstCheckin));
                                        if (lastCheckout != null && lastCheckout != "")
                                        {
                                            xTimesheetGridDTO.checkout = string.Format("{0:h:mm tt}", Convert.ToDateTime(lastCheckout));
                                        }
                                        xTimesheetGridDTO.checkout = lastCheckout;
                                        xTimesheetGridDTO.traveltime = xduration.ToString(@"hh\:mm") + " hrs";
                                        xTimesheetGridDTO.emp_id = _objTimesheetGridDTOList[b].emp_id;
                                        xTimesheetGridDTO.Time = TotalWorkedHrs.ToString(@"hh\:mm");
                                        xTimesheetGridDTO.empname = _objTimesheetGridDTOList[b].empname;
                                        xTimesheetGridDTO.is_checkout = _objTimesheetGridDTOList[b].is_checkout;
                                        xTimesheetGridDTO.groupid = _objTimesheetGridDTOList[b].groupid;

                                        TimeSpan t1 = TimeSpan.FromHours(9);

                                        if (t1 > duration)
                                        {
                                            _objList.Add(xTimesheetGridDTO);
                                        }

                                    }
                                }
                            }
                        }
                        #endregion withemployee
                    }
                    else
                    {
                        #region withoutemployee

                        var singleJob = @"select distinct( ts_employee.firstname),COUNT(*) ,
                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                        from [intersa].[ts_timesheet]  
                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + datedd + "', 3) " +
                        "group by  ts_employee.firstname " +
                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) < 32400) and COUNT(*) = 1";

                        DataTable _xDataTable = GetDataTable(singleJob);
                        if (_xDataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < _xDataTable.Rows.Count; i++)
                            {
                                TimesheetExceptionDTO _obj = new TimesheetExceptionDTO();
                                string _firstname = _xDataTable.Rows[i]["firstname"].ToString();
                                _obj = (from ts in _TimesheetEntities.ts_timesheet
                                        join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                        join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                        from tu in ps.DefaultIfEmpty()
                                        where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                        && em.Firstname == _firstname
                                        select new TimesheetExceptionDTO
                                        {
                                            id = ts.id,
                                            job_no = ts.job_no,
                                            date = ts.ondate_created,
                                            checkin = ts.checkin,
                                            checkout = ts.checkout,
                                            traveltime = "00:00 hrs",
                                            emp_id = ts.emp_id,
                                            Time = ts.time,
                                            empname = em.Firstname,
                                            is_checkout = ts.is_checkout,
                                            groupid = ts.groupid

                                        }).OrderByDescending(x => x.checkin).FirstOrDefault();

                                _objList.Add(_obj);
                            }
                        }

                        //with multiple jobs 
                        var Exception = @"SELECT emp_id , COUNT(*) FROM
                                        ts_timesheet  where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + datedd + "', 3) " +
                                        "GROUP BY emp_id HAVING COUNT(*) > 1";

                        DataTable _DataTable = GetDataTable(Exception);
                        if (_DataTable.Rows.Count > 0)
                        {
                            for (int p = 0; p < _DataTable.Rows.Count; p++)
                            {
                                int? empid = Convert.ToInt32(_DataTable.Rows[p]["emp_id"]);

                                List<TimesheetExceptionDTO> _objTimesheetGridDTOList = new List<TimesheetExceptionDTO>();
                                _objTimesheetGridDTOList = (from ts in _TimesheetEntities.ts_timesheet
                                                            join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                            join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                            from tu in ps.DefaultIfEmpty()
                                                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                                            && ts.emp_id == empid
                                                            select new TimesheetExceptionDTO
                                                            {
                                                                id = ts.id,
                                                                job_no = ts.job_no,
                                                                date = ts.ondate_created,
                                                                checkin = ts.checkin,
                                                                checkout = ts.checkout,
                                                                emp_id = ts.emp_id,
                                                                Time = ts.time,
                                                                empname = em.Firstname,
                                                                is_checkout = ts.is_checkout,
                                                                groupid = ts.groupid

                                                            }).OrderBy(x => x.checkin).ToList();


                                string firstCheckin = "";
                                string lastCheckout = "";
                                string lastJob = "";
                                TimeSpan duration = TimeSpan.Zero;
                                TimeSpan xduration = TimeSpan.Zero;
                                TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                                for (int b = 0; b < _objTimesheetGridDTOList.Count; b++)
                                {
                                    if (_objTimesheetGridDTOList[b].Time == null || _objTimesheetGridDTOList[b].Time == "00:00")
                                    {
                                        _objTimesheetGridDTOList[b].Time = TimeSpan.Zero.ToString();
                                        TotalWorkedHrs += TimeSpan.Zero;
                                    }
                                    else
                                    {
                                        TotalWorkedHrs += new TimeSpan(DateTime.Parse(_objTimesheetGridDTOList[b].Time).Ticks - TimeSpan.Zero.Ticks);
                                    }

                                    if (b == 0)
                                    {
                                        firstCheckin = _objTimesheetGridDTOList[b].checkin;
                                    }

                                    if (b == _objTimesheetGridDTOList.Count - 1)
                                    {
                                        if (_objTimesheetGridDTOList[b].checkout == null)
                                        {
                                            _objTimesheetGridDTOList[b].checkout = TimeSpan.Zero.ToString();
                                            lastJob = _objTimesheetGridDTOList[b].job_no;
                                            lastCheckout = "";
                                            duration = TimeSpan.Zero;
                                            xduration = TimeSpan.Zero;
                                        }
                                        else
                                        {
                                            lastCheckout = _objTimesheetGridDTOList[b].checkout;
                                            lastJob = _objTimesheetGridDTOList[b].job_no;

                                            //total Hours in The Site
                                            duration = DateTime.Parse(lastCheckout).Subtract(DateTime.Parse(firstCheckin));

                                            //Total Duration Transport
                                            xduration = DateTime.Parse(duration.ToString()).Subtract(DateTime.Parse(TotalWorkedHrs.ToString(@"hh\:mm")));
                                        }
                                        TimesheetExceptionDTO xTimesheetGridDTO = new TimesheetExceptionDTO();
                                        xTimesheetGridDTO.id = _objTimesheetGridDTOList[b].id;
                                        xTimesheetGridDTO.job_no = lastJob;
                                        xTimesheetGridDTO.date = _objTimesheetGridDTOList[b].date;
                                        xTimesheetGridDTO.checkin = string.Format("{0:h:mm tt}", Convert.ToDateTime(firstCheckin));
                                        if (lastCheckout != null && lastCheckout != "")
                                        {
                                            xTimesheetGridDTO.checkout = string.Format("{0:h:mm tt}", Convert.ToDateTime(lastCheckout));
                                        }
                                        xTimesheetGridDTO.checkout = lastCheckout;
                                        xTimesheetGridDTO.traveltime = xduration.ToString(@"hh\:mm") + " hrs";
                                        xTimesheetGridDTO.emp_id = _objTimesheetGridDTOList[b].emp_id;
                                        xTimesheetGridDTO.Time = TotalWorkedHrs.ToString(@"hh\:mm") + " hrs";
                                        xTimesheetGridDTO.empname = _objTimesheetGridDTOList[b].empname;
                                        xTimesheetGridDTO.is_checkout = _objTimesheetGridDTOList[b].is_checkout;
                                        xTimesheetGridDTO.groupid = _objTimesheetGridDTOList[b].groupid;

                                        TimeSpan t1 = TimeSpan.FromHours(9);

                                        if (t1 > duration)
                                        {
                                            _objList.Add(xTimesheetGridDTO);
                                        }

                                    }
                                }
                            }
                        }
                        #endregion withoutemployee
                    }
                }
            }
            catch (Exception e)
            {
                //_joblabwork.Message = e.Message.ToString();
            }
            return _objList;
        }

        public List<TimesheetExceptionDTO> LaborWorkedMoreThanNineHourExceptionGrid()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            List<TimesheetExceptionDTO> _objList = new List<TimesheetExceptionDTO>();

            var singleJob = @"select distinct( ts_employee.firstname),COUNT(*) ,
                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                        from [intersa].[ts_timesheet]  
                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + swedishTime + "', 3) " +
                        "group by  ts_employee.firstname " +
                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) > 32400) and COUNT(*) = 1";

            DataTable _xDataTable = GetDataTable(singleJob);

            if (_xDataTable.Rows.Count > 0)
            {
                for (int i = 0; i < _xDataTable.Rows.Count; i++)
                {
                    TimesheetExceptionDTO _obj = new TimesheetExceptionDTO();
                    string _firstname = _xDataTable.Rows[i]["firstname"].ToString();
                    _obj = (from ts in _TimesheetEntities.ts_timesheet
                            join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                            join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                            from tu in ps.DefaultIfEmpty()
                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                            && em.Firstname == _firstname
                            select new TimesheetExceptionDTO
                            {
                                id = ts.id,
                                job_no = ts.job_no,
                                date = ts.ondate_created,
                                checkin = ts.checkin,
                                checkout = ts.checkout,
                                traveltime = "00:00 hrs",
                                emp_id = ts.emp_id,
                                Time = ts.time,
                                empname = em.Firstname,
                                is_checkout = ts.is_checkout,
                                groupid = ts.groupid

                            }).OrderByDescending(x => x.checkin).FirstOrDefault();

                    _objList.Add(_obj);
                }
            }


            //with multiple jobs 
            var Exception = @"SELECT emp_id , COUNT(*) FROM
                            ts_timesheet  where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9),  '" + swedishTime + "', 3) " +
                            "GROUP BY emp_id HAVING COUNT(*) > 1";

            DataTable _DataTable = GetDataTable(Exception);

            if (_DataTable.Rows.Count > 0)
            {
                for (int p = 0; p < _DataTable.Rows.Count; p++)
                {
                    int? empid = Convert.ToInt32(_DataTable.Rows[p]["emp_id"]);

                    List<TimesheetExceptionDTO> _objTimesheetGridDTOList = new List<TimesheetExceptionDTO>();

                    _objTimesheetGridDTOList = (from ts in _TimesheetEntities.ts_timesheet
                                                join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                from tu in ps.DefaultIfEmpty()
                                                where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                && ts.emp_id == empid
                                                select new TimesheetExceptionDTO
                                                {
                                                    id = ts.id,
                                                    job_no = ts.job_no,
                                                    date = ts.ondate_created,
                                                    checkin = ts.checkin,
                                                    checkout = ts.checkout,
                                                    emp_id = ts.emp_id,
                                                    Time = ts.time,
                                                    empname = em.Firstname,
                                                    is_checkout = ts.is_checkout,
                                                    groupid = ts.groupid

                                                }).OrderBy(x => x.checkin).ToList();

                    string firstCheckin = "";
                    string lastCheckout = "";
                    string lastJob = "";
                    TimeSpan duration = TimeSpan.Zero;
                    TimeSpan xduration = TimeSpan.Zero;
                    TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                    for (int b = 0; b < _objTimesheetGridDTOList.Count; b++)
                    {
                        if (_objTimesheetGridDTOList[b].Time == null || _objTimesheetGridDTOList[b].Time == "00:00")
                        {
                            _objTimesheetGridDTOList[b].Time = TimeSpan.Zero.ToString();
                            TotalWorkedHrs += TimeSpan.Zero;
                        }
                        else
                        {
                            TotalWorkedHrs += new TimeSpan(DateTime.Parse(_objTimesheetGridDTOList[b].Time).Ticks - TimeSpan.Zero.Ticks);
                        }
                        if (b == 0)
                        {
                            firstCheckin = _objTimesheetGridDTOList[b].checkin;
                        }

                        if (b == _objTimesheetGridDTOList.Count - 1)
                        {
                            if (_objTimesheetGridDTOList[b].checkout == null)
                            {
                                _objTimesheetGridDTOList[b].checkout = TimeSpan.Zero.ToString();
                                lastJob = _objTimesheetGridDTOList[b].job_no;
                                lastCheckout = "";
                                duration = TimeSpan.Zero;
                                xduration = TimeSpan.Zero;
                            }
                            else
                            {
                                lastCheckout = _objTimesheetGridDTOList[b].checkout;
                                lastJob = _objTimesheetGridDTOList[b].job_no;

                                //total Hours in The Site
                                duration = DateTime.Parse(lastCheckout).Subtract(DateTime.Parse(firstCheckin));

                                //Total Duration Transport
                                xduration = DateTime.Parse(duration.ToString()).Subtract(DateTime.Parse(TotalWorkedHrs.ToString(@"hh\:mm")));
                            }

                            TimesheetExceptionDTO xTimesheetGridDTO = new TimesheetExceptionDTO();
                            xTimesheetGridDTO.id = _objTimesheetGridDTOList[b].id;
                            xTimesheetGridDTO.job_no = lastJob;
                            xTimesheetGridDTO.date = _objTimesheetGridDTOList[b].date;
                            xTimesheetGridDTO.checkin = string.Format("{0:h:mm tt}", Convert.ToDateTime(firstCheckin));
                            if (lastCheckout != null && lastCheckout != "")
                            {
                                xTimesheetGridDTO.checkout = string.Format("{0:h:mm tt}", Convert.ToDateTime(lastCheckout));
                            }
                            xTimesheetGridDTO.checkout = lastCheckout;
                            xTimesheetGridDTO.traveltime = xduration.ToString(@"hh\:mm") + " hrs";
                            xTimesheetGridDTO.emp_id = _objTimesheetGridDTOList[b].emp_id;
                            xTimesheetGridDTO.Time = TotalWorkedHrs.ToString(@"hh\:mm") + " hrs";
                            xTimesheetGridDTO.empname = _objTimesheetGridDTOList[b].empname;
                            xTimesheetGridDTO.is_checkout = _objTimesheetGridDTOList[b].is_checkout;
                            xTimesheetGridDTO.groupid = _objTimesheetGridDTOList[b].groupid;

                            TimeSpan t1 = TimeSpan.FromHours(9);

                            if (duration > t1)
                            {
                                _objList.Add(xTimesheetGridDTO);
                            }

                        }
                    }
                }
            }

            return _objList;

        }

        //filter
        public List<TimesheetExceptionDTO> DashboardLaborWorkedMoreThanNineHourExceptionGrid(DateTime fromdate, DateTime todate, string Employee)
        {
            List<TimesheetExceptionDTO> _objList = new List<TimesheetExceptionDTO>();
            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);

            try
            {

                var dates = new List<DateTime>();
                for (var dt = _fromdate; dt <= todate; dt = dt.AddDays(1))
                {
                    dates.Add(dt);
                }

                for (int c = 0; c < dates.Count; c++)
                {
                    var datedd = dates[c].Date;
                    if (Employee != null && Employee != "")
                    {
                        #region withemployee

                        var singleJob = @"select distinct( ts_employee.firstname),COUNT(*) ,
                                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                                        from [intersa].[ts_timesheet]  
                                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + datedd + "', 3) " +
                                        "and  [ts_timesheet] .emp_id='" + Employee + "' " +
                                        "group by  ts_employee.firstname " +
                                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) > 32400) and COUNT(*) = 1";

                        DataTable _xDataTable = GetDataTable(singleJob);
                        if (_xDataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < _xDataTable.Rows.Count; i++)
                            {
                                TimesheetExceptionDTO _obj = new TimesheetExceptionDTO();
                                string _firstname = _xDataTable.Rows[i]["firstname"].ToString();
                                _obj = (from ts in _TimesheetEntities.ts_timesheet
                                        join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                        join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                        from tu in ps.DefaultIfEmpty()
                                        where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                        && em.Firstname == _firstname
                                        select new TimesheetExceptionDTO
                                        {
                                            id = ts.id,
                                            job_no = ts.job_no,
                                            date = ts.ondate_created,
                                            checkin = ts.checkin,
                                            checkout = ts.checkout,
                                            traveltime = "00:00 hrs",
                                            emp_id = ts.emp_id,
                                            Time = ts.time,
                                            empname = em.Firstname,
                                            is_checkout = ts.is_checkout,
                                            groupid = ts.groupid

                                        }).OrderByDescending(x => x.checkin).FirstOrDefault();

                                _objList.Add(_obj);
                            }
                        }

                        //with multiple jobs 
                        var Exception = @"SELECT emp_id , COUNT(*) FROM
                                        ts_timesheet  where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + datedd + "', 3) " +
                                        "and  [ts_timesheet] .emp_id='" + Employee + "' " +
                                        "GROUP BY emp_id HAVING COUNT(*) > 1";

                        DataTable _DataTable = GetDataTable(Exception);
                        if (_DataTable.Rows.Count > 0)
                        {
                            for (int p = 0; p < _DataTable.Rows.Count; p++)
                            {
                                int? empid = Convert.ToInt32(_DataTable.Rows[p]["emp_id"]);

                                List<TimesheetExceptionDTO> _objTimesheetGridDTOList = new List<TimesheetExceptionDTO>();

                                _objTimesheetGridDTOList = (from ts in _TimesheetEntities.ts_timesheet
                                                            join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                            join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                            from tu in ps.DefaultIfEmpty()
                                                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                                            && ts.emp_id == empid
                                                            select new TimesheetExceptionDTO
                                                            {
                                                                id = ts.id,
                                                                job_no = ts.job_no,
                                                                date = ts.ondate_created,
                                                                checkin = ts.checkin,
                                                                checkout = ts.checkout,
                                                                emp_id = ts.emp_id,
                                                                Time = ts.time,
                                                                empname = em.Firstname,
                                                                is_checkout = ts.is_checkout,
                                                                groupid = ts.groupid

                                                            }).OrderBy(x => x.checkin).ToList();


                                string firstCheckin = "";
                                string lastCheckout = "";
                                string lastJob = "";
                                TimeSpan duration = TimeSpan.Zero;
                                TimeSpan xduration = TimeSpan.Zero;
                                TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                                for (int b = 0; b < _objTimesheetGridDTOList.Count; b++)
                                {
                                    if (_objTimesheetGridDTOList[b].Time == null || _objTimesheetGridDTOList[b].Time == "00:00")
                                    {
                                        _objTimesheetGridDTOList[b].Time = TimeSpan.Zero.ToString();
                                        TotalWorkedHrs += TimeSpan.Zero;
                                    }
                                    else
                                    {
                                        TotalWorkedHrs += new TimeSpan(DateTime.Parse(_objTimesheetGridDTOList[b].Time).Ticks - TimeSpan.Zero.Ticks);
                                    }

                                    if (b == 0)
                                    {
                                        firstCheckin = _objTimesheetGridDTOList[b].checkin;
                                    }

                                    if (b == _objTimesheetGridDTOList.Count - 1)
                                    {
                                        if (_objTimesheetGridDTOList[b].checkout == null)
                                        {
                                            _objTimesheetGridDTOList[b].checkout = TimeSpan.Zero.ToString();
                                            lastJob = _objTimesheetGridDTOList[b].job_no;
                                            lastCheckout = "";
                                            duration = TimeSpan.Zero;
                                            xduration = TimeSpan.Zero;
                                        }
                                        else
                                        {
                                            lastCheckout = _objTimesheetGridDTOList[b].checkout;
                                            lastJob = _objTimesheetGridDTOList[b].job_no;

                                            //total Hours in The Site
                                            duration = DateTime.Parse(lastCheckout).Subtract(DateTime.Parse(firstCheckin));

                                            //Total Duration Transport
                                            xduration = DateTime.Parse(duration.ToString()).Subtract(DateTime.Parse(TotalWorkedHrs.ToString(@"hh\:mm")));
                                        }

                                        TimesheetExceptionDTO xTimesheetGridDTO = new TimesheetExceptionDTO();
                                        xTimesheetGridDTO.id = _objTimesheetGridDTOList[b].id;
                                        xTimesheetGridDTO.job_no = lastJob;
                                        xTimesheetGridDTO.date = _objTimesheetGridDTOList[b].date;
                                        xTimesheetGridDTO.checkin = string.Format("{0:h:mm tt}", Convert.ToDateTime(firstCheckin));
                                        if (lastCheckout != null && lastCheckout != "")
                                        {
                                            xTimesheetGridDTO.checkout = string.Format("{0:h:mm tt}", Convert.ToDateTime(lastCheckout));
                                        }
                                        xTimesheetGridDTO.checkout = lastCheckout;
                                        xTimesheetGridDTO.traveltime = xduration.ToString(@"hh\:mm") + " hrs";
                                        xTimesheetGridDTO.emp_id = _objTimesheetGridDTOList[b].emp_id;
                                        xTimesheetGridDTO.Time = TotalWorkedHrs.ToString(@"hh\:mm");
                                        xTimesheetGridDTO.empname = _objTimesheetGridDTOList[b].empname;
                                        xTimesheetGridDTO.is_checkout = _objTimesheetGridDTOList[b].is_checkout;
                                        xTimesheetGridDTO.groupid = _objTimesheetGridDTOList[b].groupid;

                                        TimeSpan t1 = TimeSpan.FromHours(9);

                                        if (duration > t1)
                                        {
                                            _objList.Add(xTimesheetGridDTO);
                                        }

                                    }
                                }
                            }
                        }
                        #endregion withemployee
                    }
                    else
                    {
                        #region withoutemployee

                        var singleJob = @"select distinct( ts_employee.firstname),COUNT(*) ,
                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                        from [intersa].[ts_timesheet]  
                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + datedd + "', 3) " +
                        "group by  ts_employee.firstname " +
                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) > 32400) and COUNT(*) = 1";

                        DataTable _xDataTable = GetDataTable(singleJob);
                        if (_xDataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < _xDataTable.Rows.Count; i++)
                            {
                                TimesheetExceptionDTO _obj = new TimesheetExceptionDTO();
                                string _firstname = _xDataTable.Rows[i]["firstname"].ToString();
                                _obj = (from ts in _TimesheetEntities.ts_timesheet
                                        join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                        join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                        from tu in ps.DefaultIfEmpty()
                                        where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                        && em.Firstname == _firstname
                                        select new TimesheetExceptionDTO
                                        {
                                            id = ts.id,
                                            job_no = ts.job_no,
                                            date = ts.ondate_created,
                                            checkin = ts.checkin,
                                            checkout = ts.checkout,
                                            traveltime = "00:00 hrs",
                                            emp_id = ts.emp_id,
                                            Time = ts.time,
                                            empname = em.Firstname,
                                            is_checkout = ts.is_checkout,
                                            groupid = ts.groupid

                                        }).OrderByDescending(x => x.checkin).FirstOrDefault();

                                _objList.Add(_obj);
                            }
                        }

                        //with multiple jobs 
                        var Exception = @"SELECT emp_id , COUNT(*) FROM
                                        ts_timesheet  where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + datedd + "', 3) " +
                                        "GROUP BY emp_id HAVING COUNT(*) > 1";

                        DataTable _DataTable = GetDataTable(Exception);
                        if (_DataTable.Rows.Count > 0)
                        {
                            for (int p = 0; p < _DataTable.Rows.Count; p++)
                            {
                                int? empid = Convert.ToInt32(_DataTable.Rows[p]["emp_id"]);

                                List<TimesheetExceptionDTO> _objTimesheetGridDTOList = new List<TimesheetExceptionDTO>();
                                _objTimesheetGridDTOList = (from ts in _TimesheetEntities.ts_timesheet
                                                            join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                            join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                            from tu in ps.DefaultIfEmpty()
                                                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(datedd)
                                                            && ts.emp_id == empid
                                                            select new TimesheetExceptionDTO
                                                            {
                                                                id = ts.id,
                                                                job_no = ts.job_no,
                                                                date = ts.ondate_created,
                                                                checkin = ts.checkin,
                                                                checkout = ts.checkout,
                                                                emp_id = ts.emp_id,
                                                                Time = ts.time,
                                                                empname = em.Firstname,
                                                                is_checkout = ts.is_checkout,
                                                                groupid = ts.groupid

                                                            }).OrderBy(x => x.checkin).ToList();


                                string firstCheckin = "";
                                string lastCheckout = "";
                                string lastJob = "";
                                TimeSpan duration = TimeSpan.Zero;
                                TimeSpan xduration = TimeSpan.Zero;
                                TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                                for (int b = 0; b < _objTimesheetGridDTOList.Count; b++)
                                {
                                    if (_objTimesheetGridDTOList[b].Time == null || _objTimesheetGridDTOList[b].Time == "00:00")
                                    {
                                        _objTimesheetGridDTOList[b].Time = TimeSpan.Zero.ToString();
                                        TotalWorkedHrs += TimeSpan.Zero;
                                    }
                                    else
                                    {
                                        TotalWorkedHrs += new TimeSpan(DateTime.Parse(_objTimesheetGridDTOList[b].Time).Ticks - TimeSpan.Zero.Ticks);
                                    }

                                    if (b == 0)
                                    {
                                        firstCheckin = _objTimesheetGridDTOList[b].checkin;
                                    }

                                    if (b == _objTimesheetGridDTOList.Count - 1)
                                    {
                                        if (_objTimesheetGridDTOList[b].checkout == null)
                                        {
                                            _objTimesheetGridDTOList[b].checkout = TimeSpan.Zero.ToString();
                                            lastJob = _objTimesheetGridDTOList[b].job_no;
                                            lastCheckout = "";
                                            duration = TimeSpan.Zero;
                                            xduration = TimeSpan.Zero;
                                        }
                                        else
                                        {
                                            lastCheckout = _objTimesheetGridDTOList[b].checkout;
                                            lastJob = _objTimesheetGridDTOList[b].job_no;

                                            //total Hours in The Site
                                            duration = DateTime.Parse(lastCheckout).Subtract(DateTime.Parse(firstCheckin));

                                            //Total Duration Transport
                                            xduration = DateTime.Parse(duration.ToString()).Subtract(DateTime.Parse(TotalWorkedHrs.ToString(@"hh\:mm")));
                                        }
                                        TimesheetExceptionDTO xTimesheetGridDTO = new TimesheetExceptionDTO();
                                        xTimesheetGridDTO.id = _objTimesheetGridDTOList[b].id;
                                        xTimesheetGridDTO.job_no = lastJob;
                                        xTimesheetGridDTO.date = _objTimesheetGridDTOList[b].date;
                                        xTimesheetGridDTO.checkin = string.Format("{0:h:mm tt}", Convert.ToDateTime(firstCheckin));
                                        if (lastCheckout != null && lastCheckout != "")
                                        {
                                            xTimesheetGridDTO.checkout = string.Format("{0:h:mm tt}", Convert.ToDateTime(lastCheckout));
                                        }
                                        xTimesheetGridDTO.checkout = lastCheckout;
                                        xTimesheetGridDTO.traveltime = xduration.ToString(@"hh\:mm") + " hrs";
                                        xTimesheetGridDTO.emp_id = _objTimesheetGridDTOList[b].emp_id;
                                        xTimesheetGridDTO.Time = TotalWorkedHrs.ToString(@"hh\:mm") + " hrs";
                                        xTimesheetGridDTO.empname = _objTimesheetGridDTOList[b].empname;
                                        xTimesheetGridDTO.is_checkout = _objTimesheetGridDTOList[b].is_checkout;
                                        xTimesheetGridDTO.groupid = _objTimesheetGridDTOList[b].groupid;

                                        TimeSpan t1 = TimeSpan.FromHours(9);

                                        if (duration > t1)
                                        {
                                            _objList.Add(xTimesheetGridDTO);
                                        }

                                    }
                                }
                            }
                        }
                        #endregion withoutemployee
                    }
                }
            }
            catch (Exception e)
            {
                //_joblabwork.Message = e.Message.ToString();
            }
            return _objList;
        }

        #endregion Dashboard Exception

        #region Chart

        public DataTable GetChartMonthly()
        {
            DataTable dtres = new DataTable();
            try
            {
                dtres = GetDataTable("select jobno,sum(qty_item*time_min) as totTime from joblabwork where jobdate between  '2018-08-25' and  '2018-10-25' and joblabcostId not like '%A%' and joblabcostId not like '%B%' group by jobno order by totTime desc");

            }
            catch (Exception ex)
            {
                //Logger.Log(ex);
            }
            return dtres;
        }

        #endregion Chart

        #region DashboardCount

        public async Task<DashboardCounter> getDashboardCount(DateTime? _startDate, DateTime? endDate)
        {
            DashboardCounter _DashboardCounter = new DashboardCounter();
            string WorkedLessThanNineHour = "";
            string WorkedMoreThanNineHour = "";

            DateTime startDate = Convert.ToDateTime(_startDate);
            List<DateTime> dates = new List<DateTime>();

            if (endDate != null)
            {
                DateTime ObjEndDate = Convert.ToDateTime(endDate);
                for (var dt = ObjEndDate; dt <= startDate; dt = dt.AddDays(1))
                {
                    dates.Add(dt);
                }
            }
            else
            {
                dates.Add(startDate);
            }

            int absent = 0;

            if (endDate == null)
            {
                endDate = _startDate;
            }

            #region Attandance

            //Project
            var resultsProject = await Task.Run(() => _TimesheetEntities.ts_timesheet
                                                         .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(endDate))
                                                         .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(_startDate))
                                                         .GroupBy(c => System.Data.Entity.DbFunctions.TruncateTime(c.ondate_created))
                                                         .Select(g => new { emp = g.Select(z => z.emp_id).Distinct().Count() }).ToList());
          
            _DashboardCounter.count_attandace = resultsProject.Sum(d => d.emp).ToString();

            #endregion Attandance

            #region  Absent
            var NotOutsourced = await Task.Run(() => _TimesheetEntities.ts_timesheet
                                                .Join(_TimesheetEntities.ts_employee,
                                                        timesheet => timesheet.emp_id,
                                                        employee => employee.empID,
                                                        (timesheet, employee) => new { TIMESHEET = timesheet, EMPLOYEE = employee })
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.TIMESHEET.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(endDate))
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.TIMESHEET.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(_startDate))
                                                .Where(x => x.EMPLOYEE.is_outsourced == false)
                                                .GroupBy(c => System.Data.Entity.DbFunctions.TruncateTime(c.TIMESHEET.ondate_created))
                                                .Select(g => new { g.Key.Value, MyCount = g.Select(z => z.TIMESHEET.emp_id).Distinct().Count() }).ToList());


            var queryemp = await Task.Run(() => _TimesheetEntities.ts_employee
                                                    .Where(x => x.workType == 1 && x.Inactive == false && x.jobend == 0 && x.is_outsourced == false)
                                                    .OrderBy(x => x.empname).Distinct().Count());


            for (int r = 0; r < NotOutsourced.Count; r++)
            {
                if (NotOutsourced[r].Value.DayOfWeek != DayOfWeek.Friday)
                {
                    if ((queryemp - NotOutsourced[r].MyCount) < 0)
                    {
                        absent += 0;
                        _DashboardCounter.count_absent = absent.ToString();
                    }
                    else
                    {
                        absent += (queryemp - NotOutsourced[r].MyCount);
                        _DashboardCounter.count_absent = absent.ToString();
                    }
                }
                else { _DashboardCounter.count_absent = absent.ToString(); }
            }

            //Default Value
            if (NotOutsourced.Count == 0)
            {
                _DashboardCounter.count_absent = absent.ToString();
            }

            #endregion  Absent

            _DashboardCounter._ChartDataDTOList = await Task.Run(() => ChartTaskAsync(endDate, _startDate));

            //for (int i = 0; i < dates.Count; i++)
            //{
            //    DateTime _dateTime = dates[i];

            //    //Exception 
            //    WorkedLessThanNineHour += Convert.ToInt32(await Task.Run(() => LaborWorkedLessThanNineHourExceptionCountAsync(_dateTime)));
            //    _DashboardCounter.count_exception = WorkedLessThanNineHour.ToString();

            //    //overtime 
            //    WorkedMoreThanNineHour += Convert.ToInt32(await Task.Run(() => LaborWorkedMoreThanNineHourExceptionCount(_dateTime)));
            //    _DashboardCounter.count_overtime = WorkedMoreThanNineHour.ToString();
            //}

            return _DashboardCounter;
        }

        public async Task<string> LaborWorkedLessThanNineHourExceptionCountAsync(DateTime swedishTime)
        {
            List<TimesheetExceptionDTO> _objList = new List<TimesheetExceptionDTO>();

            var singleJob = @"select distinct( ts_employee.firstname),COUNT(*) ,
                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                        from [intersa].[ts_timesheet]  
                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + swedishTime + "', 3) " +
                        "group by  ts_employee.firstname " +
                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) < 32400) and COUNT(*) = 1";

            DateTime dateTime = swedishTime.AddDays(-5);
            var NotOutsourced = await Task.Run(() => _TimesheetEntities.ts_timesheet
                                              .Join(_TimesheetEntities.ts_employee,
                                                      timesheet => timesheet.emp_id,
                                                      employee => employee.empID,
                                                      (timesheet, employee) => new { TIMESHEET = timesheet, EMPLOYEE = employee })
                                              .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.TIMESHEET.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(dateTime))
                                              .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.TIMESHEET.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(swedishTime))
                                              .GroupBy(c => System.Data.Entity.DbFunctions.TruncateTime(c.TIMESHEET.ondate_created))
                                              .Select(g => new
                                              {
                                                  EmpId = g.Select(z => z.TIMESHEET.emp_id).Distinct(),
                                                  Time = (g.Select(x => x.TIMESHEET.time))

                                              }).ToList());


            DataTable _xDataTable = GetDataTable(singleJob);
            if (_xDataTable.Rows.Count > 0)
            {
                for (int i = 0; i < _xDataTable.Rows.Count; i++)
                {
                    TimesheetExceptionDTO _obj = new TimesheetExceptionDTO();
                    string _firstname = _xDataTable.Rows[i]["firstname"].ToString();
                    _obj = (from ts in _TimesheetEntities.ts_timesheet
                            join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                            join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                            from tu in ps.DefaultIfEmpty()
                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                            && em.Firstname == _firstname
                            select new TimesheetExceptionDTO
                            {
                                id = ts.id,
                                job_no = ts.job_no,
                                date = ts.ondate_created,
                                checkin = ts.checkin,
                                checkout = ts.checkout,
                                traveltime = "00:00 hrs",
                                emp_id = ts.emp_id,
                                Time = ts.time,
                                empname = em.Firstname,
                                is_checkout = ts.is_checkout,
                                groupid = ts.groupid

                            }).OrderByDescending(x => x.checkin).FirstOrDefault();

                    _objList.Add(_obj);
                }
            }


            //with multiple jobs 
            var Exception = @"SELECT emp_id , COUNT(*) FROM
                            ts_timesheet  where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + swedishTime + "', 3) " +
                            "GROUP BY emp_id HAVING COUNT(*) > 1";

            DataTable _DataTable = GetDataTable(Exception);
            if (_DataTable.Rows.Count > 0)
            {
                for (int p = 0; p < _DataTable.Rows.Count; p++)
                {
                    int? empid = Convert.ToInt32(_DataTable.Rows[p]["emp_id"]);

                    List<TimesheetExceptionDTO> _objTimesheetGridDTOList = new List<TimesheetExceptionDTO>();

                    _objTimesheetGridDTOList = (from ts in _TimesheetEntities.ts_timesheet
                                                join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                from tu in ps.DefaultIfEmpty()
                                                where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                && ts.emp_id == empid
                                                select new TimesheetExceptionDTO
                                                {
                                                    id = ts.id,
                                                    job_no = ts.job_no,
                                                    date = ts.ondate_created,
                                                    checkin = ts.checkin,
                                                    checkout = ts.checkout,
                                                    emp_id = ts.emp_id,
                                                    Time = ts.time,
                                                    empname = em.Firstname,
                                                    is_checkout = ts.is_checkout,
                                                    groupid = ts.groupid

                                                }).OrderBy(x => x.checkin).ToList();

                    string firstCheckin = "";
                    string lastCheckout = "";
                    string lastJob = "";
                    TimeSpan duration = TimeSpan.Zero;
                    TimeSpan xduration = TimeSpan.Zero;
                    TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                    for (int b = 0; b < _objTimesheetGridDTOList.Count; b++)
                    {
                        if (_objTimesheetGridDTOList[b].Time == null || _objTimesheetGridDTOList[b].Time == "00:00")
                        {
                            _objTimesheetGridDTOList[b].Time = TimeSpan.Zero.ToString();
                            TotalWorkedHrs += TimeSpan.Zero;
                        }
                        else
                        {
                            TotalWorkedHrs += new TimeSpan(DateTime.Parse(_objTimesheetGridDTOList[b].Time).Ticks - TimeSpan.Zero.Ticks);
                        }
                        if (b == 0)
                        {
                            firstCheckin = _objTimesheetGridDTOList[b].checkin;
                        }

                        if (b == _objTimesheetGridDTOList.Count - 1)
                        {
                            if (_objTimesheetGridDTOList[b].checkout == null)
                            {
                                _objTimesheetGridDTOList[b].checkout = TimeSpan.Zero.ToString();
                                lastJob = _objTimesheetGridDTOList[b].job_no;
                                lastCheckout = "";
                                duration = TimeSpan.Zero;
                                xduration = TimeSpan.Zero;
                            }
                            else
                            {
                                lastCheckout = _objTimesheetGridDTOList[b].checkout;
                                lastJob = _objTimesheetGridDTOList[b].job_no;

                                //total Hours in The Site
                                duration = DateTime.Parse(lastCheckout).Subtract(DateTime.Parse(firstCheckin));

                                //Total Duration Transport
                                xduration = DateTime.Parse(duration.ToString()).Subtract(DateTime.Parse(TotalWorkedHrs.ToString(@"hh\:mm")));
                            }

                            TimesheetExceptionDTO xTimesheetGridDTO = new TimesheetExceptionDTO();
                            xTimesheetGridDTO.id = _objTimesheetGridDTOList[b].id;
                            xTimesheetGridDTO.job_no = lastJob;
                            xTimesheetGridDTO.date = _objTimesheetGridDTOList[b].date;
                            xTimesheetGridDTO.checkin = string.Format("{0:h:mm tt}", Convert.ToDateTime(firstCheckin));
                            if (lastCheckout != null && lastCheckout != "")
                            {
                                xTimesheetGridDTO.checkout = string.Format("{0:h:mm tt}", Convert.ToDateTime(lastCheckout));
                            }
                            xTimesheetGridDTO.checkout = lastCheckout;
                            xTimesheetGridDTO.traveltime = xduration.ToString(@"hh\:mm") + " hrs";
                            xTimesheetGridDTO.emp_id = _objTimesheetGridDTOList[b].emp_id;
                            xTimesheetGridDTO.Time = TotalWorkedHrs.ToString(@"hh\:mm") + " hrs";
                            xTimesheetGridDTO.empname = _objTimesheetGridDTOList[b].empname;
                            xTimesheetGridDTO.is_checkout = _objTimesheetGridDTOList[b].is_checkout;
                            xTimesheetGridDTO.groupid = _objTimesheetGridDTOList[b].groupid;

                            TimeSpan t1 = TimeSpan.FromHours(9);

                            if (t1 > duration)
                            {
                                _objList.Add(xTimesheetGridDTO);
                            }

                        }
                    }
                }
            }
            string count = _objList.Count.ToString();
            return count;
        }

        public string LaborWorkedMoreThanNineHourExceptionCount(DateTime swedishTime)
        {
            List<TimesheetExceptionDTO> _objList = new List<TimesheetExceptionDTO>();

            var singleJob = @"select distinct( ts_employee.firstname),COUNT(*) ,
                        RIGHT('0' + CAST(SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 3600 AS VARCHAR),2) + ':' +
                        RIGHT('0' + CAST((SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) / 60) % 60 AS VARCHAR),2) as time
                        from [intersa].[ts_timesheet]  
                        inner join ts_employee on  [intersa].[ts_timesheet] .emp_id = ts_employee.empID
                        where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9), '" + swedishTime + "', 3) " +
                        "group by  ts_employee.firstname " +
                        "having( SUM(( DATEPART(hh, ts_timesheet.time) * 3600 ) + ( DATEPART(mi, ts_timesheet.time) * 60 )) > 32400) and COUNT(*) = 1";

            DataTable _xDataTable = GetDataTable(singleJob);

            if (_xDataTable.Rows.Count > 0)
            {
                for (int i = 0; i < _xDataTable.Rows.Count; i++)
                {
                    TimesheetExceptionDTO _obj = new TimesheetExceptionDTO();
                    string _firstname = _xDataTable.Rows[i]["firstname"].ToString();
                    _obj = (from ts in _TimesheetEntities.ts_timesheet
                            join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                            join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                            from tu in ps.DefaultIfEmpty()
                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                            && em.Firstname == _firstname
                            select new TimesheetExceptionDTO
                            {
                                id = ts.id,
                                job_no = ts.job_no,
                                date = ts.ondate_created,
                                checkin = ts.checkin,
                                checkout = ts.checkout,
                                traveltime = "00:00 hrs",
                                emp_id = ts.emp_id,
                                Time = ts.time,
                                empname = em.Firstname,
                                is_checkout = ts.is_checkout,
                                groupid = ts.groupid

                            }).OrderByDescending(x => x.checkin).FirstOrDefault();

                    _objList.Add(_obj);
                }
            }


            //with multiple jobs 
            var Exception = @"SELECT emp_id , COUNT(*) FROM
                            ts_timesheet  where  CONVERT(VARCHAR(10), ondate_created, 3) =  CONVERT(VARCHAR(9),  '" + swedishTime + "', 3) " +
                            "GROUP BY emp_id HAVING COUNT(*) > 1";

            DataTable _DataTable = GetDataTable(Exception);

            if (_DataTable.Rows.Count > 0)
            {
                for (int p = 0; p < _DataTable.Rows.Count; p++)
                {
                    int? empid = Convert.ToInt32(_DataTable.Rows[p]["emp_id"]);

                    List<TimesheetExceptionDTO> _objTimesheetGridDTOList = new List<TimesheetExceptionDTO>();

                    _objTimesheetGridDTOList = (from ts in _TimesheetEntities.ts_timesheet
                                                join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                                                join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                                                from tu in ps.DefaultIfEmpty()
                                                where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                && ts.emp_id == empid
                                                select new TimesheetExceptionDTO
                                                {
                                                    id = ts.id,
                                                    job_no = ts.job_no,
                                                    date = ts.ondate_created,
                                                    checkin = ts.checkin,
                                                    checkout = ts.checkout,
                                                    emp_id = ts.emp_id,
                                                    Time = ts.time,
                                                    empname = em.Firstname,
                                                    is_checkout = ts.is_checkout,
                                                    groupid = ts.groupid

                                                }).OrderBy(x => x.checkin).ToList();

                    string firstCheckin = "";
                    string lastCheckout = "";
                    string lastJob = "";
                    TimeSpan duration = TimeSpan.Zero;
                    TimeSpan xduration = TimeSpan.Zero;
                    TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                    for (int b = 0; b < _objTimesheetGridDTOList.Count; b++)
                    {
                        if (_objTimesheetGridDTOList[b].Time == null || _objTimesheetGridDTOList[b].Time == "00:00")
                        {
                            _objTimesheetGridDTOList[b].Time = TimeSpan.Zero.ToString();
                            TotalWorkedHrs += TimeSpan.Zero;
                        }
                        else
                        {
                            TotalWorkedHrs += new TimeSpan(DateTime.Parse(_objTimesheetGridDTOList[b].Time).Ticks - TimeSpan.Zero.Ticks);
                        }
                        if (b == 0)
                        {
                            firstCheckin = _objTimesheetGridDTOList[b].checkin;
                        }

                        if (b == _objTimesheetGridDTOList.Count - 1)
                        {
                            if (_objTimesheetGridDTOList[b].checkout == null)
                            {
                                _objTimesheetGridDTOList[b].checkout = TimeSpan.Zero.ToString();
                                lastJob = _objTimesheetGridDTOList[b].job_no;
                                lastCheckout = "";
                                duration = TimeSpan.Zero;
                                xduration = TimeSpan.Zero;
                            }
                            else
                            {
                                lastCheckout = _objTimesheetGridDTOList[b].checkout;
                                lastJob = _objTimesheetGridDTOList[b].job_no;

                                //total Hours in The Site
                                duration = DateTime.Parse(lastCheckout).Subtract(DateTime.Parse(firstCheckin));

                                //Total Duration Transport
                                xduration = DateTime.Parse(duration.ToString()).Subtract(DateTime.Parse(TotalWorkedHrs.ToString(@"hh\:mm")));
                            }

                            TimesheetExceptionDTO xTimesheetGridDTO = new TimesheetExceptionDTO();
                            xTimesheetGridDTO.id = _objTimesheetGridDTOList[b].id;
                            xTimesheetGridDTO.job_no = lastJob;
                            xTimesheetGridDTO.date = _objTimesheetGridDTOList[b].date;
                            xTimesheetGridDTO.checkin = string.Format("{0:h:mm tt}", Convert.ToDateTime(firstCheckin));
                            if (lastCheckout != null && lastCheckout != "")
                            {
                                xTimesheetGridDTO.checkout = string.Format("{0:h:mm tt}", Convert.ToDateTime(lastCheckout));
                            }
                            xTimesheetGridDTO.checkout = lastCheckout;
                            xTimesheetGridDTO.traveltime = xduration.ToString(@"hh\:mm") + " hrs";
                            xTimesheetGridDTO.emp_id = _objTimesheetGridDTOList[b].emp_id;
                            xTimesheetGridDTO.Time = TotalWorkedHrs.ToString(@"hh\:mm") + " hrs";
                            xTimesheetGridDTO.empname = _objTimesheetGridDTOList[b].empname;
                            xTimesheetGridDTO.is_checkout = _objTimesheetGridDTOList[b].is_checkout;
                            xTimesheetGridDTO.groupid = _objTimesheetGridDTOList[b].groupid;

                            TimeSpan t1 = TimeSpan.FromHours(9);

                            if (duration > t1)
                            {
                                _objList.Add(xTimesheetGridDTO);
                            }

                        }
                    }
                }
            }


            string count = _objList.Count.ToString();
            return count;
        }

        #endregion DashboardCount

        #region AbsentDashboard

        public async Task<List<DashboardAbsent>> AbsentEmployees()
        {

            List<DashboardAbsent> _ts_employeeList = new List<DashboardAbsent>();


            //IEnumerable<ts_employee> 
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var swedishTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            if (swedishTime.DayOfWeek != DayOfWeek.Friday)
            {
                List<int?> attended = await Task.Run(() => (from ts in _TimesheetEntities.ts_timesheet
                                                            where System.Data.Entity.DbFunctions.TruncateTime(ts.ondate_created) == System.Data.Entity.DbFunctions.TruncateTime(swedishTime)
                                                            select ts.emp_id).Distinct().ToList());

                List<int> attended1 = await Task.Run(() => (from ts in _TimesheetEntities.ts_employee
                                                            where ts.workType == 1 && ts.Inactive == false && ts.jobend == 0 && ts.is_outsourced == false
                                                            select ts.empID).Distinct().ToList());

                List<int?> second = attended1.Cast<int?>().ToList();
                var result = second.Except(attended).ToList();

                for (int i = 0; i < result.Count; i++)
                {
                    DashboardAbsent _ts_employee = new DashboardAbsent();

                    int? xEmpId = result[i];
                    var rest = _TimesheetEntities.ts_employee.Where(x => x.empID == xEmpId).FirstOrDefault();
                    _ts_employee.Firstname = rest.Firstname;
                    _ts_employee.Date = DateTime.Now;

                    _ts_employeeList.Add(_ts_employee);
                }

            }


            return _ts_employeeList;
        }

        public async Task<List<DashboardAbsent>> AbsentEmployeesForGridViewRefresh(DateTime fromdate, DateTime todate, string employee)
        {
            var _fromdate = Convert.ToDateTime(fromdate);
            var _todate = Convert.ToDateTime(todate);
            List<DashboardAbsent> _ts_employeeList = new List<DashboardAbsent>();

            int? empID = 0;
            if (employee != "")
            {
                empID = Convert.ToInt32(employee);
            }

            List<int> attended1 = await Task.Run(() => (from ts in _TimesheetEntities.ts_employee
                                                        where ts.workType == 1 && ts.Inactive == false
                                                        && ts.jobend == 0 && ts.is_outsourced == false
                                                        select ts.empID).Distinct().ToList());

            if (employee != null && employee != "")
            {
                var NotOutsourced = await Task.Run(() => _TimesheetEntities.ts_timesheet
                                                .Join(_TimesheetEntities.ts_employee,
                                                        timesheet => timesheet.emp_id,
                                                        emp => emp.empID,
                                                        (timesheet, emp) => new { TIMESHEET = timesheet, EMPLOYEE = emp })
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.TIMESHEET.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate))
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.TIMESHEET.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate))
                                                .Where(x => x.EMPLOYEE.is_outsourced == false)
                                                .GroupBy(c => System.Data.Entity.DbFunctions.TruncateTime(c.TIMESHEET.ondate_created))
                                                .Select(g => new { g.Key.Value, MyCount = g.Select(z => z.TIMESHEET.emp_id).Distinct() }).ToList());

                for (int r = 0; r < NotOutsourced.Count; r++)
                {
                    if (NotOutsourced[r].Value.DayOfWeek != DayOfWeek.Friday)
                    {
                        List<int?> zsecond = attended1.Cast<int?>().ToList();
                        var rezsult = zsecond.Except(NotOutsourced[r].MyCount).ToList();

                        for (int i = 0; i < rezsult.Count; i++)
                        {
                            DashboardAbsent _ts_employee = new DashboardAbsent();

                            int? xEmpId = rezsult[i];
                            if (xEmpId == empID)
                            {
                                var rest = _TimesheetEntities.ts_employee.Where(x => x.empID == xEmpId).FirstOrDefault();
                                _ts_employee.Firstname = rest.Firstname;
                                _ts_employee.Date = NotOutsourced[r].Value;

                                _ts_employeeList.Add(_ts_employee);
                            }

                        }
                    }
                }
            }
            else
            {
                var NotOutsourced = await Task.Run(() => _TimesheetEntities.ts_timesheet
                                                .Join(_TimesheetEntities.ts_employee,
                                                        timesheet => timesheet.emp_id,
                                                        emp => emp.empID,
                                                        (timesheet, emp) => new { TIMESHEET = timesheet, EMPLOYEE = emp })
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.TIMESHEET.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate))
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.TIMESHEET.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate))
                                                .Where(x => x.EMPLOYEE.is_outsourced == false)
                                                .GroupBy(c => System.Data.Entity.DbFunctions.TruncateTime(c.TIMESHEET.ondate_created))
                                                .Select(g => new { g.Key.Value, MyCount = g.Select(z => z.TIMESHEET.emp_id).Distinct() }).ToList());

                for (int r = 0; r < NotOutsourced.Count; r++)
                {
                    if (NotOutsourced[r].Value.DayOfWeek != DayOfWeek.Friday)
                    {
                        List<int?> zsecond = attended1.Cast<int?>().ToList();
                        var rezsult = zsecond.Except(NotOutsourced[r].MyCount).ToList();

                        for (int i = 0; i < rezsult.Count; i++)
                        {
                            DashboardAbsent _ts_employee = new DashboardAbsent();

                            int? xEmpId = rezsult[i];
                            var rest = _TimesheetEntities.ts_employee.Where(x => x.empID == xEmpId).FirstOrDefault();
                            _ts_employee.Firstname = rest.Firstname;
                            _ts_employee.Date = NotOutsourced[r].Value;

                            _ts_employeeList.Add(_ts_employee);
                        }
                    }
                }
            }

            return _ts_employeeList;
        }

        #endregion AbsentDashboard


        #region Dashboard Chart
        public async Task<List<ChartDataDTO>> ChartTaskAsync(DateTime? fromdate, DateTime? todate)
        {
            List<ChartDataDTO> chartDataDTOList = new List<ChartDataDTO>();
            try
            {
                var rest = await Task.Run(() => _TimesheetEntities.ts_timesheet
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.ondate_created) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate))
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.ondate_created) <= System.Data.Entity.DbFunctions.TruncateTime(todate))
                                                .GroupBy(c => c.job_no)
                                                .Select(g => new { job = g.Select(z => z.job_no), person = g.Select(z => z.job_no).Count() }).ToList());

      
                for (int i = 0; i < rest.Count; i++)
                {
                    string job = "";
                    
                    foreach (var item in rest[i].job)
                    {
                        job = item;
                    }

                    var person = rest[i].person;

                    var result = await Task.Run(() => _TimesheetEntities.joblabworks
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.fromtime) >= System.Data.Entity.DbFunctions.TruncateTime(fromdate))
                                                .Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.fromtime) <= System.Data.Entity.DbFunctions.TruncateTime(todate))
                                                .Where(x => x.jobno == job)
                                                .Select(g => new { qty = g.qty_item, time = g.time_min, note = g.Notes }).ToList());

                    double time = 0;
                    double qty = 0;
                    for (int t = 0; t < result.Count; t++)
                    {
                        ChartDataDTO _ChartDataDTO = new ChartDataDTO();

                        if (result[t].time != null && result[t].qty!= null)
                        {
                            time += Convert.ToDouble(result[t].qty * result[t].time);
                        }

                        if (result[t].qty != null)
                        {
                            qty += Convert.ToDouble(result[t].qty);
                        }
                        else
                        {
                            qty += 0;
                        }

                        if (result[t].note != null)
                        {
                            _ChartDataDTO.comment += result[t].note + System.Environment.NewLine;
                        }

                        if (t == result.Count - 1)
                        {
                            _ChartDataDTO.job = job;

                            var totalMinutes = time;
                            var ztime = TimeSpan.FromMinutes(totalMinutes);
                            string workHours = string.Format("{0}.{1:00}", (int)ztime.TotalHours, ztime.Minutes);
                            _ChartDataDTO.workedhours = workHours;

                            var TotalManHours = (480 * person);
                            var xtotalMinutes = TotalManHours;
                            var xtime = TimeSpan.FromMinutes(xtotalMinutes);
                            string workHours1 = string.Format("{0}.{1:00}", (int)xtime.TotalHours, xtime.Minutes);

                            _ChartDataDTO.targethours = workHours1;
                            _ChartDataDTO.person = person.ToString();
                            chartDataDTOList.Add(_ChartDataDTO);
                        }
                    }

                }
                return chartDataDTOList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion Dashboard Chart


    }

}




