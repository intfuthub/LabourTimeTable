using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LabourTimeTable.Models;
using WebMatrix.WebData;
using System.Threading.Tasks;
using System.Web.Security;
using System.Security.Cryptography;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Threading;
using Newtonsoft.Json;


namespace LabourTimeTable.Controllers
{
    public class HomeController : Controller
    {
        UtilitySession _UtilitySession = new UtilitySession();
        Repository _Repository = new Repository();
        public ActionResult Index()
        {
            LoginModel _UserModel = new LoginModel();
            try
            {
                HttpCookie existingCookie = Request.Cookies["MMATaxINTLXMLNPL"];
                HttpCookie existingCookiePass = Request.Cookies["MMBTaxINTLXMLCSL"];
                if (existingCookie != null && existingCookiePass != null)
                {
                    HttpCookie cookie = (HttpCookie)Request.Cookies["MMATaxINTLXMLNPL"];
                    HttpCookie cookiePass = (HttpCookie)Request.Cookies["MMBTaxINTLXMLCSL"];

                    // Get the cookie's payload == Enrypted FormsAuthenticationTicket 
                    string encryptedTicket = cookie.Value;
                    string encryptedTicketPass = cookiePass.Value;

                    // Decrypt the cookie's payload 
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(encryptedTicket);
                    FormsAuthenticationTicket ticketPass = FormsAuthentication.Decrypt(encryptedTicketPass);

                    string[] strRole = ticket.UserData.Split(',');
                    string[] strRolePass = ticketPass.UserData.Split(',');
                    _UserModel.UserName = strRole[0].ToString();
                    _UserModel.Password = strRolePass[0].ToString();
                    return View(_UserModel);
                }
                return View();
            }
            catch (CryptographicException cex)
            {
                //_UtilitySession.Session._TimesheetGridDTO = null;
                //_UtilitySession.Session.user = null;

                //Request.Cookies["MMATaxINTLXMLNPL"].Expires = DateTime.Now.AddDays(-1);
                //Request.Cookies["MMBTaxINTLXMLCSL"].Expires = DateTime.Now.AddDays(-1);

                ExpireAllCookies();
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Index(FormCollection _FormCollection)
        {

            LoginModel model = new LoginModel();

            try
            {
                model.UserName = _FormCollection["username"].ToString();
                model.Password = _FormCollection["password"].ToString();


                _UtilitySession.Session = await Task.Run(() => _Repository.getLogin(model));
                if (_UtilitySession.Session.Status == true)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    _UtilitySession.Session.PageSize = 5;
                    _UtilitySession.Session.TotalCount = _UtilitySession.Session._TimesheetGridDTO.Count;

                    bool rememberMe = false;

                    if (_FormCollection["loginCheckbox1"] != null)
                    {
                        rememberMe = true;
                    }

                    if (rememberMe)
                    {
                        //user
                        FormsAuthenticationTicket objTicket = new FormsAuthenticationTicket(1, "timesheetUser", DateTime.Now, DateTime.Now.AddHours(1), false, model.UserName, FormsAuthentication.FormsCookiePath);

                        HttpCookie objCookie = new HttpCookie("MMATaxINTLXMLNPL", FormsAuthentication.Encrypt(objTicket));
                        objCookie.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Add(objCookie);

                        //pass
                        FormsAuthenticationTicket objTicketPass = new FormsAuthenticationTicket(1, "TimesheetPass", DateTime.Now, DateTime.Now.AddHours(1), false, model.Password, FormsAuthentication.FormsCookiePath);

                        HttpCookie objCookiePass = new HttpCookie("MMBTaxINTLXMLCSL", FormsAuthentication.Encrypt(objTicketPass));
                        objCookiePass.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Add(objCookiePass);
                    }

                    if (_UtilitySession.Session.user.name.ToUpper() == "MUHAMMAD ARIF" || _UtilitySession.Session.user.name.ToUpper() == "WAEL MOSTAFA"
                        || _UtilitySession.Session.user.name.ToUpper() == "NASER ALALI" || _UtilitySession.Session.user.name.ToUpper() == "MAHMOOD ISKANDARANI")
                    {
                        return RedirectToAction("Home", "Dashboard");
                    }

                    return RedirectToAction("Detail");
                }
                else
                {
                    ViewBag.Error = _UtilitySession.Session.Message;
                }
            }
            catch (Exception w)
            {
                ViewBag.Error = w.ToString();
            }
            return View();
        }

        public async Task<ActionResult> Detail()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    TimesheetEntities _TimesheetEntities = new TimesheetEntities();

                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();
                    _LabourDetailDTO._TimesheetGridDTO = await Task.Run(() => _Repository.getTimesheetData(User_id));
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

        [HttpPost]
        public ActionResult Detail(TimesheetPostDTO _TimesheetPostDTO)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();

            try
            {

                if (_UtilitySession.Session != null)
                {
                    try
                    {
                        ViewBag.Name = _UtilitySession.Session.user.name;
                        List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();
                        _UtilitySession.Session._TimesheetGridDTO = null;

                        _TimesheetGridDTO = _Repository.setTimesheetData(_TimesheetPostDTO, _UtilitySession.Session.user);
                        _UtilitySession.Session._TimesheetGridDTO = _TimesheetGridDTO;

                        _LabourDetailDTO._TimesheetGridDTO = _TimesheetGridDTO;
                        _LabourDetailDTO._TimesheetGridDTO = _UtilitySession.Session._TimesheetGridDTO;
                        _LabourDetailDTO.PageSize = 5;
                        _LabourDetailDTO.TotalCount = _UtilitySession.Session._TimesheetGridDTO.Count;

                    }
                    catch (Exception w)
                    {
                        ViewBag.Error = w.ToString();
                    }
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
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult LogOut()
        {
            if (_UtilitySession.Session != null)
            {
                _UtilitySession.Session._TimesheetGridDTO = null;
                _UtilitySession.Session.user = null;
            }

            return RedirectToAction("Index", "Home");

        }

        [HttpPost]
        public async Task<JsonResult> GetProjectDetailList()
        {
            ViewBag.Name = _UtilitySession.Session.user.name;
            int? EmpId = _UtilitySession.Session.user.emp_id;
            ProjectDetailsDTO _ProjectDetailsDTO = new ProjectDetailsDTO();
            _ProjectDetailsDTO = await Task.Run(() => _Repository.getProjectDetails(EmpId));
            return Json(_ProjectDetailsDTO, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetOutSourceEmployeeList(int outsourced_id)
        {
            OutsourcedEmployeesDTO _OutsourcedEmployeesDTO = new OutsourcedEmployeesDTO();
            _OutsourcedEmployeesDTO = await Task.Run(() => _Repository.getOutSourcedEmployees(outsourced_id));
            return Json(_OutsourcedEmployeesDTO, JsonRequestBehavior.AllowGet);
        }

        private void ExpireAllCookies()
        {
            if (HttpContext.Request != null)
            {
                int cookieCount = HttpContext.Request.Cookies.Count;
                for (var i = 0; i < cookieCount; i++)
                {
                    var cookie = HttpContext.Request.Cookies[i];
                    if (cookie != null)
                    {
                        var expiredCookie = new HttpCookie(cookie.Name)
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            Domain = cookie.Domain
                        };
                        HttpContext.Response.Cookies.Add(expiredCookie); // overwrite it
                    }
                }

                // clear cookies server side
                HttpContext.Request.Cookies.Clear();
            }
        }

        public ActionResult IndividualCheckOut(int? empid, string checkOutTime)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();
            if (_UtilitySession.Session != null)
            {
                ViewBag.Name = _UtilitySession.Session.user.name;
                List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();
                _UtilitySession.Session._TimesheetGridDTO = null;

                _TimesheetGridDTO = _Repository.IndividualCheckOut(_UtilitySession.Session.user, empid, checkOutTime);
                _UtilitySession.Session._TimesheetGridDTO = _TimesheetGridDTO;

                _LabourDetailDTO._TimesheetGridDTO = _TimesheetGridDTO;

                _LabourDetailDTO._TimesheetGridDTO = _UtilitySession.Session._TimesheetGridDTO;
                _LabourDetailDTO.PageSize = 5;
                _LabourDetailDTO.TotalCount = _UtilitySession.Session._TimesheetGridDTO.Count;

                return View(_LabourDetailDTO);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public JsonResult CheckIfUserIsAdmin(int? empid)
        {
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

        public ActionResult AllCheckOut(string CheckoutTime)
        {
            LabourDetailDTO _LabourDetailDTO = new LabourDetailDTO();
            if (_UtilitySession.Session != null)
            {
                ViewBag.Name = _UtilitySession.Session.user.name;
                List<TimesheetGridDTO> _TimesheetGridDTO = new List<TimesheetGridDTO>();
                _UtilitySession.Session._TimesheetGridDTO = null;

                _TimesheetGridDTO = _Repository.AllCheckOut(_UtilitySession.Session.user, CheckoutTime);
                _UtilitySession.Session._TimesheetGridDTO = _TimesheetGridDTO;

                _LabourDetailDTO._TimesheetGridDTO = _TimesheetGridDTO;

                _LabourDetailDTO._TimesheetGridDTO = _UtilitySession.Session._TimesheetGridDTO;
                _LabourDetailDTO.PageSize = 5;
                _LabourDetailDTO.TotalCount = _UtilitySession.Session._TimesheetGridDTO.Count;

                return View(_LabourDetailDTO);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #region Activity

        public async Task<ActionResult> Activity(string JobNo, string groupid)
        {
            if (_UtilitySession.Session != null)
            {
                if (JobNo == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    ActivityGetDTO _ActivityGetDTO = new ActivityGetDTO();
                    _ActivityGetDTO = await Task.Run(() => _Repository.getActivityDetails(JobNo));
                    return View(_ActivityGetDTO);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<ActionResult> ActivityPartialView(string JobNo, string jobsystemId, string EnquiryNo, string groupid)
        {
            List<JobDailyProduction> _JobDailyProductionList = new List<JobDailyProduction>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeZoneInfo xcet = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime xcurrentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var xyourTime = TimeZoneInfo.ConvertTime(xcurrentDate, xcet);

            DateTime xdt = xyourTime;
            string xDateSelected = xdt.ToString("yyyy-MM-dd");
            DateTime xdateTime = Convert.ToDateTime(xDateSelected);


            List<SystemTaskDTO> _SystemTaskDTOList = new List<SystemTaskDTO>();
            SystemTaskDTO _SystemTaskDTO = new SystemTaskDTO();
            if (_UtilitySession.Session != null)
            {
                try
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;

                    var xResult = _Repository.getEmployeePerGroup(_UtilitySession.Session.user.id, groupid);
                    var yourTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));

                    #region Dates
                    string DateSelected = xdt.ToString("yyyy-MM-dd");
                    var combo = DateSelected + " " + xResult[0].checkin;
                    DateTime Checkin = Convert.ToDateTime(combo);

                    DateTime dtx = yourTime;
                    string strChkOut = "";
                    if (xResult[0].checkout == null)
                        strChkOut = "05:00 PM";
                    else
                        strChkOut = xResult[0].checkout;

                    string zDateSelected = dtx.ToString("yyyy-MM-dd");
                    var xcombo = zDateSelected + " " + strChkOut;

                    DateTime Checkout = Convert.ToDateTime(xcombo);
                    #endregion Dates

                    _JobDailyProductionList = await Task.Run(() => _Repository.GoLabCost(EnquiryNo, JobNo, jobsystemId, xdateTime, groupid, Checkin, Checkout));

                    #region

                    List<int> xIndex = new List<int>();
                    for (int i = 0; i < _JobDailyProductionList.Count; i++)
                    {
                        if (_JobDailyProductionList[i].taskname == "")
                        {
                            xIndex.Add(i);
                        }
                    }

                    List<List<JobDailyProduction>> xJobDailyProduction = new List<List<JobDailyProduction>>();

                    for (int x = 0; x < xIndex.Count; x++)
                    {
                        List<JobDailyProduction> _ObjJobDailyProductionxList = new List<JobDailyProduction>();

                        if (x == 0)
                        {
                            _ObjJobDailyProductionxList = _Repository.splitList(_JobDailyProductionList, xIndex[x]);
                            _JobDailyProductionList.RemoveRange(0, xIndex[x]);
                        }
                        else
                        {
                            int range = (xIndex[x] - xIndex[x - 1]);
                            _ObjJobDailyProductionxList = _Repository.splitList(_JobDailyProductionList, range);
                            _JobDailyProductionList.RemoveRange(0, range);
                            _ObjJobDailyProductionxList.RemoveAt(0);
                        }
                        xJobDailyProduction.Add(_ObjJobDailyProductionxList);
                    }

                    for (int i = 0; i < xJobDailyProduction.Count; i++)
                    {
                        if (xJobDailyProduction[i].Count > 0)
                        {
                            if (xJobDailyProduction[i][0].taskname.Contains("Cable")
                                || xJobDailyProduction[i][0].taskname.Contains("Pulling"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionCBList = xJobDailyProduction[i];
                            }
                            else if (xJobDailyProduction[i][0].taskname.Contains("Termination"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionITAList = xJobDailyProduction[i];

                            }
                            else if (xJobDailyProduction[i][0].taskname.Contains("Testing")
                                || xJobDailyProduction[i][0].taskname.Contains("Commissioning"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionTCList = xJobDailyProduction[i];
                            }
                        }
                    }

                    #endregion
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.ToString();
                }
                return PartialView("_PartialViewForDailyProduction1", _SystemTaskDTO);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult ActivityPartialView(int TaskId, int Qty, string Floor, string JobNo, string jobsystemId, string EnquiryNo, string groupid, string systemname, string comments)
        {
            List<JobDailyProduction> _JobDailyProductionList = new List<JobDailyProduction>();
            SystemTaskDTO _SystemTaskDTO = new SystemTaskDTO();

            try
            {
                if (_UtilitySession.Session != null)
                {

                    var xResult = _Repository.getEmployeePerGroup(_UtilitySession.Session.user.id, groupid);
                    var xResultOutSourced = _Repository.getOutSourceEmployeePerGroup(_UtilitySession.Session.user.id, groupid);
                    var yourTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));

                    #region Dates

                    DateTime dt = yourTime;
                    string DateSelected = dt.ToString("yyyy-MM-dd");
                    var combo = DateSelected + " " + xResult[0].checkin;
                    DateTime Checkin = Convert.ToDateTime(combo);

                    DateTime dtx = yourTime;
                    string strChkOut = "";
                    if (xResult[0].checkout == null)
                        strChkOut = "05:00 PM";
                    else
                        strChkOut = xResult[0].checkout;

                    string zDateSelected = dtx.ToString("yyyy-MM-dd");
                    var xcombo = zDateSelected + " " + strChkOut;

                    DateTime Checkout = Convert.ToDateTime(xcombo);

                    DateTime xJobdate = yourTime;
                    string _xJobdate = xJobdate.ToString("yyyy-MM-dd");
                    xJobdate = Convert.ToDateTime(_xJobdate);

                    #endregion Dates

                    _JobDailyProductionList = _Repository.UpdateJobDailyProduction(EnquiryNo, TaskId, Qty, JobNo, xJobdate, jobsystemId, xResultOutSourced.Count().ToString(), "Outsource Comapany Name", groupid, Checkout, Checkin, systemname);

                    #region

                    List<int> xIndex = new List<int>();
                    for (int i = 0; i < _JobDailyProductionList.Count; i++)
                    {
                        if (_JobDailyProductionList[i].taskname == "")
                        {
                            xIndex.Add(i);
                        }
                    }

                    List<List<JobDailyProduction>> xJobDailyProduction = new List<List<JobDailyProduction>>();

                    for (int x = 0; x < xIndex.Count; x++)
                    {
                        List<JobDailyProduction> _ObjJobDailyProductionxList = new List<JobDailyProduction>();

                        if (x == 0)
                        {
                            _ObjJobDailyProductionxList = _Repository.splitList(_JobDailyProductionList, xIndex[x]);
                            _JobDailyProductionList.RemoveRange(0, xIndex[x]);
                        }
                        else
                        {
                            int range = (xIndex[x] - xIndex[x - 1]);
                            _ObjJobDailyProductionxList = _Repository.splitList(_JobDailyProductionList, range);
                            _JobDailyProductionList.RemoveRange(0, range);
                            _ObjJobDailyProductionxList.RemoveAt(0);
                        }
                        xJobDailyProduction.Add(_ObjJobDailyProductionxList);
                    }

                    for (int i = 0; i < xJobDailyProduction.Count; i++)
                    {
                        if (xJobDailyProduction[i].Count > 0)
                        {
                            if (xJobDailyProduction[i][0].taskname.Contains("Cable")
                                || xJobDailyProduction[i][0].taskname.Contains("Pulling"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionCBList = xJobDailyProduction[i];
                            }
                            else if (xJobDailyProduction[i][0].taskname.Contains("Termination"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionITAList = xJobDailyProduction[i];

                            }
                            else if (xJobDailyProduction[i][0].taskname.Contains("Testing")
                                || xJobDailyProduction[i][0].taskname.Contains("Commissioning"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionTCList = xJobDailyProduction[i];
                            }
                        }
                    }
                    #endregion

                    ViewBag.Error = "Updated Successfully";

                    return PartialView("_PartialViewForDailyProduction1", _SystemTaskDTO);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult ActivityPartialCommentView(string JobNo, string jobsystemId, string groupid, string comments)
        {
            List<JobDailyProduction> _JobDailyProductionList = new List<JobDailyProduction>();
            SystemTaskDTO _SystemTaskDTO = new SystemTaskDTO();

            try
            {
                if (_UtilitySession.Session != null)
                {


                    var xResult = _Repository.getEmployeePerGroup(_UtilitySession.Session.user.id, groupid);
                    var xResultOutSourced = _Repository.getOutSourceEmployeePerGroup(_UtilitySession.Session.user.id, groupid);
                    var yourTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));

                    #region Dates

                    DateTime dt = yourTime;
                    string DateSelected = dt.ToString("yyyy-MM-dd");
                    var combo = DateSelected + " " + xResult[0].checkin;
                    DateTime Checkin = Convert.ToDateTime(combo);

                    DateTime dtx = yourTime;
                    string strChkOut = "";
                    if (xResult[0].checkout == null)
                        strChkOut = "05:00 PM";
                    else
                        strChkOut = xResult[0].checkout;

                    string zDateSelected = dtx.ToString("yyyy-MM-dd");
                    var xcombo = zDateSelected + " " + strChkOut;

                    DateTime Checkout = Convert.ToDateTime(xcombo);

                    DateTime xJobdate = yourTime;
                    string _xJobdate = xJobdate.ToString("yyyy-MM-dd");
                    xJobdate = Convert.ToDateTime(_xJobdate);

                    #endregion Dates


                    _JobDailyProductionList = _Repository.UpdateProductionComments(JobNo, jobsystemId, groupid, comments, Checkin, Checkout);

                    #region

                    List<int> xIndex = new List<int>();
                    for (int i = 0; i < _JobDailyProductionList.Count; i++)
                    {
                        if (_JobDailyProductionList[i].taskname == "")
                        {
                            xIndex.Add(i);
                        }
                    }

                    List<List<JobDailyProduction>> xJobDailyProduction = new List<List<JobDailyProduction>>();

                    for (int x = 0; x < xIndex.Count; x++)
                    {
                        List<JobDailyProduction> _ObjJobDailyProductionxList = new List<JobDailyProduction>();

                        if (x == 0)
                        {
                            _ObjJobDailyProductionxList = _Repository.splitList(_JobDailyProductionList, xIndex[x]);
                            _JobDailyProductionList.RemoveRange(0, xIndex[x]);
                        }
                        else
                        {
                            int range = (xIndex[x] - xIndex[x - 1]);
                            _ObjJobDailyProductionxList = _Repository.splitList(_JobDailyProductionList, range);
                            _JobDailyProductionList.RemoveRange(0, range);
                            _ObjJobDailyProductionxList.RemoveAt(0);
                        }
                        xJobDailyProduction.Add(_ObjJobDailyProductionxList);
                    }

                    for (int i = 0; i < xJobDailyProduction.Count; i++)
                    {
                        if (xJobDailyProduction[i].Count > 0)
                        {
                            if (xJobDailyProduction[i][0].taskname.Contains("Cable")
                                || xJobDailyProduction[i][0].taskname.Contains("Pulling"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionCBList = xJobDailyProduction[i];
                            }
                            else if (xJobDailyProduction[i][0].taskname.Contains("Termination"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionITAList = xJobDailyProduction[i];

                            }
                            else if (xJobDailyProduction[i][0].taskname.Contains("Testing")
                                || xJobDailyProduction[i][0].taskname.Contains("Commissioning"))
                            {
                                xJobDailyProduction[i].RemoveAt(0);
                                _SystemTaskDTO._JobDailyProductionTCList = xJobDailyProduction[i];
                            }
                        }
                    }
                    #endregion

                    ViewBag.Error = "Updated Successfully";

                    return PartialView("_PartialViewForDailyProduction1", _SystemTaskDTO);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion Activity

        #region Handover

        public ActionResult Handover()
        {
            return View();

        }

        #endregion Handover

    }
}
