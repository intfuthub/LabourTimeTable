using LabourTimeTable.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Echovoice.JSON;
using ClosedXML.Excel;
using System.IO;
using System.Data;
using System.Text;


namespace LabourTimeTable.Controllers
{
    public class DashboardController : Controller
    {
        //
        // GET: /Dashboard/

        Repository _Repository = new Repository();
        UtilitySession _UtilitySession = new UtilitySession();

        public async Task<ActionResult> Home()
        {
            try
            {

                if (_UtilitySession.Session != null)
                {
                    if (_UtilitySession.Session.Status == true)
                    {
                        DateTime dt = _Repository.GetNetworkTime();
                        DateTime? dtEnd = new DateTime();
                        //HttpContext.Session["DefaultDate"] = dt;

                        ViewBag.Name = _UtilitySession.Session.user.name;
                        DashboardCounter _DashboardCounter = new DashboardCounter();
                        _DashboardCounter = await Task.Run(() => _Repository.getDashboardCount(dt, null));

                        return View(_DashboardCounter);
                    }
                    else
                    {
                        return RedirectToAction("Detail", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<ActionResult> Production()
        {
            ViewBag.Name = _UtilitySession.Session.user.name;
            //try
            //{
            //    if (_UtilitySession.Session != null)
            //    {
            //        TimesheetEntities _TimesheetEntities = new TimesheetEntities();

            //        ViewBag.Name = _UtilitySession.Session.user.name;
            //        string User_id = _UtilitySession.Session.user.id;
            //        LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();
            //        _LabourDetailDTO._TimesheetGridDTO = await Task.Run(() => _Repository.getTimesheetAllData());
            //        _LabourDetailDTO.PageSize = 5;
            //        _LabourDetailDTO.TotalCount = _LabourDetailDTO._TimesheetGridDTO.Count;

            //        return View(_LabourDetailDTO);
            //    }
            //    else
            //    {
            //        return RedirectToAction("Index", "Home");
            //    }
            //}
            //catch (Exception e)
            //{
            //    ViewBag.Error = e.ToString();
            //    return View();
            //}

            return View();
        }

        public JsonResult CheckIfUserIsAdmin()
        {
            int? empid = _UtilitySession.Session.user.emp_id;
            List<ts_timesheet> _ts_timesheet = new List<ts_timesheet>();
            if (_UtilitySession.Session != null)
            {
                if (empid == _UtilitySession.Session.user.emp_id)
                {
                    _ts_timesheet = _Repository.CheckUserIsAdmin(_UtilitySession.Session.user.id);
                }
                return Json(_ts_timesheet, JsonRequestBehavior.AllowGet);
            }
            else
            {
                RedirectToAction("Index", "Home");
                return Json(null);
                //return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        public async Task<ActionResult> GetChartList()
        {
            ViewBag.Name = _UtilitySession.Session.user.name;
            int? EmpId = _UtilitySession.Session.user.emp_id;

            var rest = await Task.Run(() => _Repository.GetChartMonthly());

            MapOneModel[] _MapOneModel = new MapOneModel[rest.Rows.Count].ToArray();
            for (int i = 0; i < rest.Rows.Count; i++)
            {
                try
                {
                    _MapOneModel[i] = new MapOneModel(rest.Rows[i]["jobno"].ToString(), rest.Rows[i]["totTime"].ToString());
                }
                catch (Exception)
                {

                    throw;
                }
            }



            var result = JSONEncoders.EncodeJsObjectArray(_MapOneModel);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region JobProgressDashboard

        public async Task<ActionResult> Dashboard()
        {
            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    DashboardDTO _DashboardDTO = new DashboardDTO();
                    List<DashboardGridDTO> _DashboardGridDTO = new List<DashboardGridDTO>();

                    _DashboardGridDTO = await Task.Run(() => _Repository.DashboardDetailsForProjectEngineers(DateTime.Now.AddDays(-10), DateTime.Now, ""));
                    _DashboardDTO._DashboardGridDTO = _DashboardGridDTO;
                    return View(_DashboardDTO);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }

        [HttpGet]
        public async Task<ActionResult> GridViewDashboard(string datepicker, string datepicker2, string JobNo)
        {
            DashboardDTO _DashboardDTO = new DashboardDTO();
            List<DashboardGridDTO> _DashboardGridDTO = new List<DashboardGridDTO>();

            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;

                    try
                    {
                        DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy",
                                                CultureInfo.InvariantCulture);

                        DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        string _JobNo = JobNo;
                        _DashboardGridDTO = await Task.Run(() => _Repository.DashboardDetailsForProjectEngineers(_fromdate, _todate, _JobNo));
                        _DashboardDTO._DashboardGridDTO = _DashboardGridDTO;
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    return PartialView("_PartialViewForGridDashboard", _DashboardDTO);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> EmployeeWorked(string JobNo, DateTime? jobdate, string SystemName, string activity, string qty_item, DateTime? fromtime, DateTime? totime, string jobsystemId)
        {

            GridDashboardEmployeeDTO _GridDashboardEmployeeDTO = new GridDashboardEmployeeDTO();

            _GridDashboardEmployeeDTO = await Task.Run(() => _Repository.DashboardEmployeesFromDashboardGrid(JobNo, jobdate, SystemName, activity, qty_item, fromtime, totime, jobsystemId));

            return PartialView("_PartialViewForEmployeeGridDashboard", _GridDashboardEmployeeDTO);
        }

        #endregion JobProgressDashboard

        #region AttandanceDashboard

        public async Task<ActionResult> AttendanceDashboard()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    TimesheetEntities _TimesheetEntities = new TimesheetEntities();

                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();
                    _LabourDetailDTO._TimesheetGridDTO = await Task.Run(() => _Repository.getTimesheetAllData());
                    _LabourDetailDTO.PageSize = 5;
                    _LabourDetailDTO.TotalCount = _LabourDetailDTO._TimesheetGridDTO.Count;

                    return View(_LabourDetailDTO);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> GridViewAttandanceDashboard(string datepicker, string datepicker2, string Employee)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();

            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;

                    try
                    {
                        DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy",
                                                CultureInfo.InvariantCulture);

                        DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        //string _JobNo = JobNo;


                        ViewBag.Name = _UtilitySession.Session.user.name;
                        string User_id = _UtilitySession.Session.user.id;

                        _LabourDetailDTO._TimesheetGridDTO = await Task.Run(() => _Repository.DashboardAttandanceForProjectEngineers(_fromdate, _todate, Employee));
                        _LabourDetailDTO.PageSize = 5;
                        _LabourDetailDTO.TotalCount = _LabourDetailDTO._TimesheetGridDTO.Count;

                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    return PartialView("_PartialViewGridAttendanceDashboard", _LabourDetailDTO);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetEmployeeList()
        {
            ViewBag.Name = _UtilitySession.Session.user.name;
            int? EmpId = _UtilitySession.Session.user.emp_id;
            ProjectDetailsDTO _ProjectDetailsDTO = new ProjectDetailsDTO();
            _ProjectDetailsDTO = await Task.Run(() => _Repository.getAttandanceEmployeeList(EmpId));
            return Json(_ProjectDetailsDTO, JsonRequestBehavior.AllowGet);
        }

        #endregion AttandanceDashboard

        #region ComplainDashboard

        public async Task<ActionResult> ComplainDashboard()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    LabourComplaintDetailDTO _LabourComplaintDetailDTO = new LabourComplaintDetailDTO();
                    _LabourComplaintDetailDTO.user = _UtilitySession.Session.user;

                    _LabourComplaintDetailDTO._TimesheetComGridPostDTO = await Task.Run(() => _Repository.getComplaintDashboard(DateTime.Now.AddDays(-10), DateTime.Now, ""));
                    _LabourComplaintDetailDTO.PageSize = 5;
                    _LabourComplaintDetailDTO.TotalCount = _LabourComplaintDetailDTO._TimesheetComGridPostDTO.Count;

                    return View(_LabourComplaintDetailDTO);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }


        [HttpGet]
        public async Task<ActionResult> GridViewComplainDashboard(string datepicker, string datepicker2, string com_no)
        {
            DashboardDTO _DashboardDTO = new DashboardDTO();
            List<DashboardGridDTO> _DashboardGridDTO = new List<DashboardGridDTO>();
            LabourComplaintDetailDTO _LabourComplaintDetailDTO = new LabourComplaintDetailDTO();

            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    try
                    {
                        DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy",
                                                CultureInfo.InvariantCulture);

                        DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        ViewBag.Name = _UtilitySession.Session.user.name;
                        string User_id = _UtilitySession.Session.user.id;
                        _LabourComplaintDetailDTO.user = _UtilitySession.Session.user;
                        _LabourComplaintDetailDTO._TimesheetComGridPostDTO = await Task.Run(() => _Repository.DashboardComplainForProjectEngineers(_fromdate, _todate, com_no));
                        _LabourComplaintDetailDTO.PageSize = 5;
                        _LabourComplaintDetailDTO.TotalCount = _LabourComplaintDetailDTO._TimesheetComGridPostDTO.Count;
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    return PartialView("_PartialViewGridComplainDashboard", _LabourComplaintDetailDTO);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }

        #endregion ComplainDashboard

        #region OtherDashboard

        public async Task<ActionResult> OtherDashboard()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {


                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    LabourOtherDetailDTO _LabourOtherDetailDTO = new LabourOtherDetailDTO();
                    _LabourOtherDetailDTO = await Task.Run(() => _Repository.getOtherDashboardData(DateTime.Now.AddDays(-10), DateTime.Now, ""));
                    _LabourOtherDetailDTO.PageSize = 5;
                    _LabourOtherDetailDTO.TotalCount = _LabourOtherDetailDTO._TimesheetEnqGridPostDTO.Count;

                    return View(_LabourOtherDetailDTO);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> GridViewOtherDashboard(string datepicker, string datepicker2, string com_no)
        {
            LabourOtherDetailDTO _LabourOtherDetailDTO = new LabourOtherDetailDTO();

            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    try
                    {
                        DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy",
                                                CultureInfo.InvariantCulture);

                        DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        ViewBag.Name = _UtilitySession.Session.user.name;
                        string User_id = _UtilitySession.Session.user.id;
                        _LabourOtherDetailDTO.user = _UtilitySession.Session.user;
                        _LabourOtherDetailDTO._TimesheetEnqGridPostDTO = await Task.Run(() => _Repository.DashboardOtherForProjectEngineers(_fromdate, _todate, com_no));
                        _LabourOtherDetailDTO.PageSize = 5;
                        _LabourOtherDetailDTO.TotalCount = _LabourOtherDetailDTO._TimesheetEnqGridPostDTO.Count;
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    return PartialView("_PartialViewGridOtherDashboard", _LabourOtherDetailDTO);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetProjectDetailList()
        {
            ViewBag.Name = _UtilitySession.Session.user.name;
            LabourOtherDataBindDTO _LabourOtherDataBindDTO = new LabourOtherDataBindDTO();
            _LabourOtherDataBindDTO = await Task.Run(() => _Repository.getAllEnquiry());
            return Json(_LabourOtherDataBindDTO, JsonRequestBehavior.AllowGet);
        }

        #endregion OtherDashboard

        #region ExceptionDashboard

        public async Task<ActionResult> ExceptionDashboard()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;

                    List<ExceptionModelDTO> _ExceptionModelDTOList = new List<ExceptionModelDTO>();

                    _ExceptionModelDTOList = await Task.Run(() => _Repository.getExceptionAttendance());
                    return View(_ExceptionModelDTOList);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> GridViewExceptionDashboard(string datepicker, string datepicker2, string JobNo)
        {
            List<ExceptionModelDTO> _ExceptionModelDTOList = new List<ExceptionModelDTO>();

            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;

                    try
                    {
                        DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy",
                                                CultureInfo.InvariantCulture);

                        DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        string _JobNo = JobNo;


                        ViewBag.Name = _UtilitySession.Session.user.name;
                        string User_id = _UtilitySession.Session.user.id;

                        _ExceptionModelDTOList = await Task.Run(() => _Repository.DashboardExceptionForProjectEngineers(_fromdate, _todate, _JobNo));

                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    return PartialView("_PartialViewGridExceptionDashboard", _ExceptionModelDTOList);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }

        #endregion ExceptionDashboard

        #region AbsentDashboard
        public async Task<ActionResult> AbsentDashboard()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;

                    List<DashboardAbsent> _ts_employee = new List<DashboardAbsent>();
                    _ts_employee = await Task.Run(() => _Repository.AbsentEmployees());

                    return View(_ts_employee);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> GridViewAbsentDashboard(string datepicker, string datepicker2, string Employee)
        {
            List<DashboardAbsent> _ts_employee = new List<DashboardAbsent>();
            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;

                    try
                    {
                        DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        ViewBag.Name = _UtilitySession.Session.user.name;
                        string User_id = _UtilitySession.Session.user.id;

                        _ts_employee = await Task.Run(() => _Repository.AbsentEmployeesForGridViewRefresh(_fromdate, _todate, Employee));
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    return PartialView("_PartialViewForAbsentEmployeeGridDashboard", _ts_employee);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }

        #endregion AbsentDashboard

        #region WorkedLessThan9Hours

        public ActionResult WorkedLessThan9Hours()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;

                    List<TimesheetExceptionDTO> _TimesheetExceptionDTO = new List<TimesheetExceptionDTO>();
                    _TimesheetExceptionDTO = _Repository.LaborWorkedLessThanNineHourExceptionGrid();

                    return View(_TimesheetExceptionDTO);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> WorkedLessThan9HoursPartialView(string datepicker, string datepicker2, string Employee)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();

            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    List<TimesheetExceptionDTO> TimesheetExceptionDTOList = new List<TimesheetExceptionDTO>();
                    try
                    {
                        DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        ViewBag.Name = _UtilitySession.Session.user.name;
                        string User_id = _UtilitySession.Session.user.id;

                        TimesheetExceptionDTOList = await Task.Run(() => _Repository.DashboardLaborWorkedLessThanNineHourExceptionGrid(_fromdate, _todate, Employee));

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    return PartialView("_PartialViewGridWorkedLessThan9HoursDashboard", TimesheetExceptionDTOList);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }

        #endregion WorkedLessThan9Hours

        #region overtime
        public ActionResult Overtime()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;

                    List<TimesheetExceptionDTO> _TimesheetExceptionDTO = new List<TimesheetExceptionDTO>();
                    _TimesheetExceptionDTO = _Repository.LaborWorkedMoreThanNineHourExceptionGrid();

                    return View(_TimesheetExceptionDTO);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> WorkedMoreThan9HoursPartialView(string datepicker, string datepicker2, string Employee)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();

            if (_UtilitySession.Session != null)
            {
                if (_UtilitySession.Session.Status == true)
                {
                    List<TimesheetExceptionDTO> TimesheetExceptionDTOList = new List<TimesheetExceptionDTO>();
                    try
                    {
                        DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        ViewBag.Name = _UtilitySession.Session.user.name;
                        string User_id = _UtilitySession.Session.user.id;

                        TimesheetExceptionDTOList = await Task.Run(() => _Repository.DashboardLaborWorkedMoreThanNineHourExceptionGrid(_fromdate, _todate, Employee));

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    return PartialView("_PartialViewGridWorkedLessThan9HoursDashboard", TimesheetExceptionDTOList);
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            else
            {
                return RedirectToAction("Detail", "Home");
            }
        }


        #endregion overtime

        #region nextandprevious
        [HttpPost]

        public async Task<ActionResult> Next_Previous(DateTime? startdt, DateTime? enddt)
        {
            try
            {

                if (_UtilitySession.Session != null)
                {
                    if (_UtilitySession.Session.Status == true)
                    {
                        ViewBag.Name = _UtilitySession.Session.user.name;
                        DashboardCounter _DashboardCounter = new DashboardCounter();
                        _DashboardCounter = await Task.Run(() => _Repository.getDashboardCount(startdt, enddt));

                        return View("_PartialViewForDashboradHome", _DashboardCounter);
                    }
                    else
                    {
                        return RedirectToAction("Detail", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Detail", "Home");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion nextandprevious



        [HttpGet]
        public virtual async Task AttandanceExport(string datepicker, string datepicker2, string Employee)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();
            //(AttandanceFilter attandanceFilter)
            try
            {
                //AttandanceFilter attandance = new AttandanceFilter();
                //attandance.datepicker = "23/10/2018";
                //attandance.datepicker2 = "23/10/2018";
                DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime _todate = DateTime.ParseExact(datepicker2.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

                List<TimesheetGridDTO> TimesheetGridDTOList = new List<TimesheetGridDTO>();
                TimesheetGridDTOList = await Task.Run(() => _Repository.DashboardAttandanceForProjectEngineers(_fromdate, _todate, Employee));

                DataTable dt = new DataTable("Grid");
                dt.Columns.AddRange(new DataColumn[6] { new DataColumn("Name"),
                                            new DataColumn("Job No"),
                                            new DataColumn("Date"),
                                            new DataColumn("Checkin"),
                                            new DataColumn("Checkout"),
                                            new DataColumn("Hours")});

                foreach (var customer in TimesheetGridDTOList)
                {
                    dt.Rows.Add(customer.empname, customer.job_no, Convert.ToDateTime(customer.date).ToString("dd-MM-yyyy"), customer.checkin, customer.checkout, customer.Time);
                }

                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                folderPath += "\\EXPORT REPORT\\";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                //Codes for the Closed XML             
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var worksheetname = "" + _fromdate.ToString("dd-MM-yy") + " - " + "" + _todate.ToString("dd-MM-yy") + "";
                    wb.Worksheets.Add(dt, worksheetname);
                    var time = DateTime.Now.ToString("dd-MM-yy");

                    var filename = "ATTANDACE EXPORT " + "-" + worksheetname + ".xlsx";
                    wb.SaveAs(folderPath + filename, false);
                }

                //DataTable dtq = dt;
                //MemoryStream MyMemoryStream = new MemoryStream();
                //using (XLWorkbook wb = new XLWorkbook())
                //{
                //    wb.Worksheets.Add(dtq);
                //    Response.Clear();
                //    Response.Charset = "";
                //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //    Response.AddHeader("content-disposition", "attachment;filename=HelloWorld.xlsx");
                //    Response.Buffer = true; wb.SaveAs(MyMemoryStream);
                //    MyMemoryStream.WriteTo(Response.OutputStream);
                //    Response.Flush();
                //    Response.End();
                //}
            }
            catch (Exception ex)
            {
                throw;
            }
        }





        [HttpGet]
        public virtual async Task AbsentExport(string datepicker, string datepicker2, string Employee)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();

            try
            {
                List<DashboardAbsent> _ts_employeeList = new List<DashboardAbsent>();
                DateTime _fromdate = DateTime.ParseExact(datepicker.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime _todate = DateTime.ParseExact(datepicker2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                ViewBag.Name = _UtilitySession.Session.user.name;
                string User_id = _UtilitySession.Session.user.id;

                _ts_employeeList = await Task.Run(() => _Repository.AbsentEmployeesForGridViewRefresh(_fromdate, _todate, Employee));

                DataTable dt = new DataTable("Grid");
                dt.Columns.AddRange(columns: new DataColumn[2] { new DataColumn("Employee Name"), new DataColumn("Date") });

                foreach (var customer in _ts_employeeList)
                {
                    dt.Rows.Add(customer.Firstname, Convert.ToDateTime(customer.Date).ToString("dd-MM-yyyy"));
                }

                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                folderPath += "\\EXPORT REPORT\\";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                //Codes for the Closed XML             
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var worksheetname = "" + _fromdate.ToString("dd-MM-yy") + " - " + "" + _todate.ToString("dd-MM-yy") + "";
                    wb.Worksheets.Add(dt, worksheetname);
                    var time = DateTime.Now.ToString("dd-MM-yy");

                    var filename = "ABSENT EXPORT " + "-" + worksheetname + ".xlsx";
                    wb.SaveAs(folderPath + filename, false);
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public ActionResult chart()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetChartData()
        {

            DateTime _fromdate = DateTime.Now.AddDays(-2);
            DateTime _todate = DateTime.Now.AddDays(-1);

            List<ChartDataDTO> _ChartDataDTO = new List<ChartDataDTO>();

            _ChartDataDTO = await Task.Run(() => _Repository.ChartTaskAsync(_fromdate, _todate));

            return Json(_ChartDataDTO.ToArray(), JsonRequestBehavior.AllowGet);
        }

    }
}
