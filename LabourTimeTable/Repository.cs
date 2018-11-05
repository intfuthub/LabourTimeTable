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

        public LabourDetailDTO getLogin(LoginModel _LoginModel)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();
            var yourTime1 = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));
            _LabourDetailDTO.user = _TimesheetEntities.ts_user
                                                      .Where(e => e.username == _LoginModel.UserName && e.password == _LoginModel.Password)
                                                      .FirstOrDefault();
            if (_LabourDetailDTO.user != null)
            {
                _LabourDetailDTO.Status = true;

                var query = (from ts in _TimesheetEntities.ts_timesheet
                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                             from tu in ps.DefaultIfEmpty()
                             where ts.user_id == _LabourDetailDTO.user.id
                             //&& System.Data.Objects.EntityFunctions.TruncateTime(ts.ondate_created) == System.Data.Objects.EntityFunctions.TruncateTime(yourTime1)
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

                _LabourDetailDTO._TimesheetGridDTO = query;
            }
            else
            {
                _LabourDetailDTO.Status = false;
                _LabourDetailDTO.Message = "Looks like user or password does not match.";
            }

            return _LabourDetailDTO;
        }

        public ProjectDetailsDTO getProjectDetails()
        {
            ProjectDetailsDTO _ProjectDetailsDTO = new ProjectDetailsDTO();
            _ProjectDetailsDTO._enq_mast = _InterfutureEntities.enq_mast
                                            .Where(x => x.ord_no != null && x.enq_status == "Job Opened")
                                            .OrderByDescending(x => x.doc_date)
                                            .ToList();

            _ProjectDetailsDTO._employee = _TimesheetEntities.ts_employee
                                            .Where(x => x.workType == 1 && x.Inactive == false && x.jobend == 0 && x.is_outsourced == false)
                                            .OrderBy(x => x.empname)
                                            .ToList();

            _ProjectDetailsDTO._OutSourceDetail = _InterfutureEntities.OutSourceDetails
                                                                           .GroupBy(a => a.companyName)
                                                                           .Select(g => g.FirstOrDefault())
                                                                           .ToList();

            return _ProjectDetailsDTO;
        }

        public OutsourcedEmployeesDTO getOutSourcedEmployees(int outsourced_id)
        {
            OutsourcedEmployeesDTO _OutsourcedEmployeesDTO = new OutsourcedEmployeesDTO();

            _OutsourcedEmployeesDTO._OutsourcedEmployees = _TimesheetEntities.ts_employee
                                            .Where(x => x.workType == 1 && x.Inactive == false && x.jobend == 0 && x.is_outsourced == true && x.outsourced_company_id == outsourced_id)
                                            .OrderBy(x => x.empname)
                                            .ToList();

            return _OutsourcedEmployeesDTO;
        }

        public List<TimesheetGridDTO> setTimesheetData(TimesheetPostDTO _TimesheetPostDTO, ts_user _ts_user)
        {
            List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();

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


                            _ts_timesheet.checkout = null;
                            _ts_timesheet.time = null;
                            _ts_timesheet.place = null;
                            _ts_timesheet.is_checkout = false;

                            var yourTime1 = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));

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
                if (_TimesheetPostDTO.OutsourceTeam != null)
                {
                    #region outsourced team
                    for (int i = 0; i < _TimesheetPostDTO.OutsourceTeam.Count; i++)
                    {
                        ts_timesheet _ts_timesheet = new ts_timesheet();
                        _ts_timesheet.id = Guid.NewGuid().ToString();
                        _ts_timesheet.user_id = _ts_user.id;
                        _ts_timesheet.emp_id = _TimesheetPostDTO.OutsourceTeam[i];
                        _ts_timesheet.job_no = _TimesheetPostDTO.JobNo;
                        _ts_timesheet.project_name = _TimesheetPostDTO.ProjectName;
                        _ts_timesheet.checkin = _TimesheetPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                        _ts_timesheet.checkout = null;
                        _ts_timesheet.time = null;
                        _ts_timesheet.place = null;

                        var yourTime1 = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));

                        _ts_timesheet.ondate_created = yourTime1;
                        _ts_timesheet.createdby = _ts_user.name;
                        _ts_timesheet.is_outsourced = true;
                        _ts_timesheet.is_checkout = false;
                        _ts_timesheet.outsourced_company_id = _TimesheetPostDTO.OutsourceCompanyID;
                        _ts_timesheet.groupid = _groupId;

                        _TimesheetEntities.ts_timesheet.Add(_ts_timesheet);
                        _TimesheetEntities.SaveChanges();
                    }
                    #endregion outsourced team
                }

                //login user
                #region login user
                ts_timesheet _ts_timesheetlogin = new ts_timesheet();
                _ts_timesheetlogin.id = Guid.NewGuid().ToString();
                _ts_timesheetlogin.user_id = _ts_user.id;
                _ts_timesheetlogin.emp_id = _ts_user.emp_id;
                _ts_timesheetlogin.job_no = _TimesheetPostDTO.JobNo;
                _ts_timesheetlogin.project_name = _TimesheetPostDTO.ProjectName;
                _ts_timesheetlogin.checkin = _TimesheetPostDTO.Time.ToString("HH:mm tt", CultureInfo.InvariantCulture);

                _ts_timesheetlogin.checkout = null;
                _ts_timesheetlogin.time = null;
                _ts_timesheetlogin.place = null;
                _ts_timesheetlogin.is_checkout = false;

                var serverTime = DateTime.Now;
                DateTime cstTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "India Standard Time");

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                TimeZoneInfo cet = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime currentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);


                var swedishTime = TimeZoneInfo.ConvertTime(currentDate, cet);


                _ts_timesheetlogin.ondate_created = swedishTime;
                _ts_timesheetlogin.createdby = _ts_user.name;
                _ts_timesheetlogin.groupid = _groupId;

                _TimesheetEntities.ts_timesheet.Add(_ts_timesheetlogin);
                _TimesheetEntities.SaveChanges();

                #endregion login user


                var query = (from ts in _TimesheetEntities.ts_timesheet
                             join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                             join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                             from tu in ps.DefaultIfEmpty()
                             where ts.user_id == _ts_user.id
                             && System.Data.Objects.EntityFunctions.TruncateTime(ts.ondate_created) == System.Data.Objects.EntityFunctions.TruncateTime(cstTime)
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
                //throw e;
                _TimesheetGridDTO[0].empname = e.ToString();
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
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception("There was a problem saving this record: " + e.Message);
                }
            }

        }

        public int GenerateRandomNo()
        {
            int _min = 3000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        public List<TimesheetGridDTO> getTimesheetData(string user_id)
        {

            var yourTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));

            List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();
            var query = (from ts in _TimesheetEntities.ts_timesheet
                         join em in _TimesheetEntities.ts_employee on ts.emp_id equals em.empID
                         join tu in _TimesheetEntities.ts_user on ts.emp_id equals tu.emp_id into ps
                         from tu in ps.DefaultIfEmpty()
                         where ts.user_id == user_id
                         //&& System.Data.Objects.EntityFunctions.TruncateTime(ts.ondate) == System.Data.Objects.EntityFunctions.TruncateTime(yourTime)
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

            var yourTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));
            try
            {
                var eQuery = (from ts in _TimesheetEntities.ts_timesheet
                              where ts.user_id == _ts_user.id
                              && ts.is_checkout == false
                              && System.Data.Objects.EntityFunctions.TruncateTime(ts.ondate_created) == System.Data.Objects.EntityFunctions.TruncateTime(yourTime)
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
                             where ts.user_id == _ts_user.id && System.Data.Objects.EntityFunctions.TruncateTime(ts.ondate_created) == System.Data.Objects.EntityFunctions.TruncateTime(yourTime)
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
            var yourTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));
            try
            {
                var eQuery = (from ts in _TimesheetEntities.ts_timesheet
                              where ts.emp_id == emp_id
                              && ts.is_checkout == false
                              && System.Data.Objects.EntityFunctions.TruncateTime(ts.ondate_created) == System.Data.Objects.EntityFunctions.TruncateTime(yourTime)
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
                             where ts.user_id == _ts_user.id && System.Data.Objects.EntityFunctions.TruncateTime(ts.ondate_created) == System.Data.Objects.EntityFunctions.TruncateTime(yourTime)
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

                _ts_timesheetList = _TimesheetEntities.ts_timesheet
                                    .Where(x => x.user_id == userid
                                                        && x.is_checkout == false
                                                        && System.Data.Objects.EntityFunctions.TruncateTime(x.ondate_created) == System.Data.Objects.EntityFunctions.TruncateTime(DateTime.UtcNow)).ToList();
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

            //InterfutureEntities db1 = new InterfutureEntities();

            InterfutureEntities db = new InterfutureEntities();


            _JobDailyProductionList = await Task.Run(() => db.JobDailyProductions
                                                           .Where(x => x.jobno == _jobno
                                                            && x.jobsystemId == _jobsystemId)
                                                            .OrderBy(x => x.Id).ToList());
            try
            {
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
                            //  where lw.jobdate == Convert.ToDateTime(RadDatePicker1.SelectedDate)
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
                        db.Entry(shift).State = EntityState.Deleted;
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


                var yourTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));
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
                    //txtComments.Text = xolw.Notes;



                    _JobDailyProductionList = await Task.Run(() => db.JobDailyProductions
                                                        .Where(x => x.jobno == _jobno
                                                         && x.jobsystemId == _jobsystemId
                                                         && System.Data.Objects.EntityFunctions.TruncateTime(x.ondate) == System.Data.Objects.EntityFunctions.TruncateTime(yourTime))
                                                         .OrderBy(x => x.Id).ToList());
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _JobDailyProductionList;
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

        public List<JobDailyProduction> UpdateJobDailyProduction(string _doc_no, int Key, decimal Qty, string _jobno, DateTime? _jobdate, string _jobsystemId, string OutSourceCount, string OutSourceCompanyName, string _groupid, DateTime _ToTime, DateTime _FromTime, string SystemName, string Comments)
        {
            List<JobDailyProduction> _JobDailyProductionList = new List<JobDailyProduction>();
            InterfutureEntities db = new InterfutureEntities();
            TimesheetEntities dbx = new TimesheetEntities();

            var _ObjJobDailyProduction = db.JobDailyProductions.Where(x => x.Id == Key && x.groupid == _groupid && x.jobsystemId == _jobsystemId).FirstOrDefault();
            var xResult = dbx.ts_timesheet.Where(x => x.groupid == _groupid).ToList();



            for (int i = 0; i < xResult.Count; i++)
            {
                int? _empID = xResult[i].emp_id;

                ts_employee objEmp = (from q in dbx.ts_employee
                                      where q.empID == _empID
                                      select q).FirstOrDefault();

                empTmp objEmpTmp = new empTmp();
                objEmpTmp.EmpId = Convert.ToInt32(_empID);
                objEmpTmp.EmpName = objEmp.empname;
                db.empTmps.Add(objEmpTmp);
                db.SaveChanges();
            }

            UpdateEmpDetails(_doc_no, _jobno, _jobdate, _jobsystemId, Comments, _ToTime, _FromTime);



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
                DateTime dt = Convert.ToDateTime(DateTime.Now);
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
                            objdp.FromTime = Convert.ToDateTime(_FromTime);

                        if (_ToTime != null)
                            objdp.ToTime = Convert.ToDateTime(_ToTime);

                        objdp.Updatedate = DateTime.Now;
                        db.SaveChanges();
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
                        objlw1.Notes = Comments;
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
                            objlw1.Notes = Comments;
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
                            objlw1.Notes = Comments;
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
                            objlabWork.Notes = Comments;
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
                            //objlbwk12.activity = _taskname;
                            objlbwk12.time_min = Convert.ToDecimal(_time_min);
                            objlbwk12.SystemName = SystemName;
                            objlbwk12.Notes = Comments;
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

                UpdateEmpDetails(_doc_no, _jobno, _jobdate, _jobsystemid, Comments, _ToTime, _FromTime);
                UpdateOutSrcDetails(_doc_no, _jobno, _jobsystemid, _jobdate, OutSourceCount, OutSourceCompanyName, _ToTime, _FromTime);
                DateTime _varjobdate = Convert.ToDateTime(_jobdate);
                CheckCommentsOnly(_varjobdate, _jobno, _jobsystemId, _ToTime, _FromTime);
                ValidationMod(_jobno, _jobsystemid);

                _JobDailyProductionList = db.JobDailyProductions.Where(x => x.jobno == _jobno && x.groupid == _groupid && x.jobsystemId == _jobsystemId).ToList();


            }
            catch (System.Exception ex)
            {
                Logger.Log(ex);
            }

            return _JobDailyProductionList;
        }


        public List<JobDailyProduction> UpdateProductionComments(string _jobno, string _jobsystemId, string _groupid, string Comments)
        {
            List<JobDailyProduction> _JobDailyProductionList = new List<JobDailyProduction>();
            InterfutureEntities db = new InterfutureEntities();
            TimesheetEntities dbx = new TimesheetEntities();


            var _ObjJobDailyProduction = db.JobDailyProductions.Where(x => x.jobno == _jobno && x.groupid == _groupid && x.jobsystemId == _jobsystemId).FirstOrDefault();
            if (_ObjJobDailyProduction != null)
            {
                _ObjJobDailyProduction.Notes = Comments.ToString();
            }


            _JobDailyProductionList = db.JobDailyProductions.Where(x => x.jobno == _jobno && x.groupid == _groupid && x.jobsystemId == _jobsystemId).ToList();


            return _JobDailyProductionList;
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

        //private static void ConvertTime(string YourDate)
        //{
        //    DateTime parsedDate;
        //    string pattern = "hh:mm tt";
        //    DateTime.TryParseExact(YourDate, pattern, null,
        //                                   DateTimeStyles.None, out parsedDate);
        //}

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

        public List<JobDailyProduction> splitList(List<JobDailyProduction> _JobDailyProduction, int nSize)
        {
            var list = new List<JobDailyProduction>();

            for (int i = 0; i < nSize && i < _JobDailyProduction.Count; i++)
            {
                list.Add(_JobDailyProduction[i]);
            }

            return list;
        }

        protected bool SaveComments(string _jobno, string _jobsystemId, string Comments, string SystemName, DateTime _ToTime, DateTime _FromTime)
        {
            try
            {
                InterfutureEntities db = new InterfutureEntities();

                DateTime dt = Convert.ToDateTime(DateTime.Now);
                string DateSelected = dt.ToString("yyyy-MM-dd");

                DateTime _DateSelected = Convert.ToDateTime(DateSelected);
                DateTime? _FrmTime = GetFrmTime(_FromTime);
                DateTime? _xToTime = GetToTime(_ToTime);


                List<joblabwork> olw = (from lw in db.joblabworks
                                        where lw.jobno == _jobno && lw.jobsystemId == _jobsystemId && lw.jobdate == _DateSelected
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

        private void UpdateEmpDetails(string _doc_no, string _jobno, DateTime? _jobdate, string _jobsystemId, string Comments, DateTime _ToTime, DateTime _FromTime)
        {
            try
            {
                InterfutureEntities db = new InterfutureEntities();
                TimesheetEntities dbx = new TimesheetEntities();

                DateTime? fTime = GetFrmTime(_FromTime);
                DateTime? tTime = GetToTime(_ToTime);

                List<joblabworkemp> lstJLWrkEmp = null;
                List<string> lstJWrkEmp = null;
                List<joblabwork> lstJLW = null;
                if (tTime != null)
                {
                    lstJLWrkEmp = (from q in db.joblabworkemps
                                   where q.jobno == _jobno && q.jobdate == _jobdate && q.fromtime == fTime && q.totime == tTime
                                   select q).ToList();
                    lstJWrkEmp = (from q in db.joblabworkemps
                                  where q.jobno == _jobno && q.jobdate == _jobdate && q.fromtime == fTime && q.totime == tTime
                                  select q.empname).ToList();
                    lstJLW = (from q in db.joblabworks
                              where q.jobno == _jobno && q.jobsystemId == _jobsystemId && q.jobdate == _jobdate && q.fromtime == fTime && q.totime == tTime
                              select q).ToList();
                }
                else
                {
                    lstJLWrkEmp = (from q in db.joblabworkemps
                                   where q.jobno == _jobno && q.jobdate == _jobdate && q.fromtime == fTime
                                   select q).ToList();
                    lstJWrkEmp = (from q in db.joblabworkemps
                                  where q.jobno == _jobno && q.jobdate == _jobdate && q.fromtime == fTime
                                  select q.empname).ToList();
                    lstJLW = (from q in db.joblabworks
                              where q.jobno == _jobno && q.jobsystemId == _jobsystemId && q.jobdate == _jobdate && q.fromtime == fTime
                              select q).ToList();
                }


                if (lstJLW.Count <= 0)
                {
                    if (lstJLWrkEmp.Count > 0)
                    {
                        foreach (var shift in lstJLWrkEmp)
                        {
                            db.Entry(shift).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (lstJLWrkEmp.Count <= 0)
                    {
                        List<empTmp> lstEmpTmp = (from q in db.empTmps
                                                  select q).ToList();
                        foreach (empTmp objEmptmp in lstEmpTmp)
                        {
                            joblabworkemp objJLWrkEmp = new joblabworkemp();
                            objJLWrkEmp.doc_no = _doc_no;
                            objJLWrkEmp.jobno = _jobno;
                            //objJLWrkEmp.jobsystemId = CreditCards7.SelectedValue;
                            objJLWrkEmp.jobdate = _jobdate;
                            objJLWrkEmp.empid = objEmptmp.EmpId;
                            objJLWrkEmp.empname = objEmptmp.EmpName;
                            objJLWrkEmp.Notes = Comments;
                            objJLWrkEmp.fromtime = fTime;
                            objJLWrkEmp.totime = tTime;
                            db.joblabworkemps.Add(objJLWrkEmp);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        List<string> lstEmpTmp = (from q in db.empTmps
                                                  select q.EmpName).ToList();
                        List<string> lstRemainEmp = lstEmpTmp.Except(lstJWrkEmp).ToList();
                        List<string> lstRemovEmp = lstJWrkEmp.Except(lstEmpTmp).ToList();
                        if (lstRemainEmp.Count > 0)
                        {
                            List<empTmp> lstEmpTp = (from q in db.empTmps
                                                     where lstRemainEmp.Contains(q.EmpName)
                                                     select q).ToList();
                            foreach (empTmp obj in lstEmpTp)
                            {
                                joblabworkemp objJLWrkEmp = new joblabworkemp();
                                objJLWrkEmp.doc_no = _doc_no;
                                objJLWrkEmp.jobno = _jobno;
                                //objJLWrkEmp.jobsystemId = CreditCards7.SelectedValue;
                                objJLWrkEmp.jobdate = _jobdate;
                                objJLWrkEmp.empid = obj.EmpId;
                                objJLWrkEmp.empname = obj.EmpName;
                                objJLWrkEmp.Notes = Comments;
                                objJLWrkEmp.fromtime = fTime;
                                objJLWrkEmp.totime = tTime;
                                db.joblabworkemps.Add(objJLWrkEmp);
                                db.SaveChanges();
                            }
                        }
                        if (lstRemovEmp.Count > 0)
                        {
                            List<joblabworkemp> lstEmpTp1 = (from q in db.joblabworkemps
                                                             where lstRemovEmp.Contains(q.empname) && q.jobno == _jobno
                                                             && q.jobdate == _jobdate && q.fromtime == fTime
                                                             && q.totime == tTime && q.jobsystemId == _jobsystemId
                                                             select q).ToList();


                            if (lstEmpTp1.Count > 0)
                            {
                                foreach (var shift in lstEmpTp1)
                                {
                                    db.Entry(shift).State = EntityState.Deleted;
                                    db.SaveChanges();
                                }
                            }
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



    }

}




