using LabourTimeTable.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LabourTimeTable.Controllers
{
    public class OthersController : Controller
    {
        //
        // GET: /Others/
        UtilitySession _UtilitySession = new UtilitySession();
        Repository _Repository = new Repository();


        public async Task<ActionResult> Index()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    TimesheetEntities _TimesheetEntities = new TimesheetEntities();

                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    LabourOtherDetailDTO _LabourOtherDetailDTO = new LabourOtherDetailDTO();
                    _LabourOtherDetailDTO.user = _UtilitySession.Session.user;
                    _LabourOtherDetailDTO = await Task.Run(() => _Repository.getEnquiryTimesheetData(User_id));
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

        [HttpPost]
        public async Task<ActionResult> Index(TimesheetENQPostDTO _TimesheetENQPostDTO)
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    LabourOtherDetailDTO _LabourOtherDetailDTO = new LabourOtherDetailDTO();
                    List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;

                    ts_user _ts_user = new ts_user();
                    _ts_user = _UtilitySession.Session.user;
                    _TimesheetEnqGridPostDTO = await Task.Run(() => _Repository.setTimesheetEnqData(_TimesheetENQPostDTO, _ts_user));
                    _LabourOtherDetailDTO._TimesheetEnqGridPostDTO = _TimesheetEnqGridPostDTO;

                    return View(_LabourOtherDetailDTO);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (NullReferenceException e)
            {
                ViewBag.Error = e.ToString();
                return View();
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

        [HttpPost]
        public async Task<JsonResult> GetDepartmentActivity(int? Id)
        {
            ViewBag.Name = _UtilitySession.Session.user.name;
            List<activitysub> _activitysubList = new List<activitysub>();
            _activitysubList = await Task.Run(() => _Repository.getDepartmentActivity(Id));
            return Json(_activitysubList, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AllCheckOut(string CheckoutTime)
        {
            LabourOtherDetailDTO _LabourOtherDetailDTO = new LabourOtherDetailDTO();
            if (_UtilitySession.Session != null)
            {
                ViewBag.Name = _UtilitySession.Session.user.name;
                List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();

                _UtilitySession.Session._TimesheetGridDTO = null;
                ts_user _ts_user = new ts_user();
                _ts_user = _UtilitySession.Session.user;
                _TimesheetEnqGridPostDTO = await Task.Run(() => _Repository.AllOtherCheckOut(_ts_user, CheckoutTime));
                _LabourOtherDetailDTO._TimesheetEnqGridPostDTO = _TimesheetEnqGridPostDTO;
                _LabourOtherDetailDTO.PageSize = 5;
                _LabourOtherDetailDTO.TotalCount = _UtilitySession.Session._TimesheetGridDTO.Count;

                return View(_LabourOtherDetailDTO);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<ActionResult> IndividualCheckOut(int? empid, string checkOutTime)
        {
            LabourOtherDetailDTO _LabourOtherDetailDTO = new LabourOtherDetailDTO();

            if (_UtilitySession.Session != null)
            {
                ViewBag.Name = _UtilitySession.Session.user.name;
                List<TimesheetEnqGridPostDTO> _TimesheetEnqGridPostDTO = new List<TimesheetEnqGridPostDTO>();
                ts_user _ts_user = new ts_user();
                _ts_user = _UtilitySession.Session.user;
                _TimesheetEnqGridPostDTO = await Task.Run(() => _Repository.IndividualOtherCheckOut(_ts_user, empid, checkOutTime));

                return View(_TimesheetEnqGridPostDTO);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<ActionResult> Activity(string enq_no, string groupid)
        {
            if (_UtilitySession.Session != null)
            {
                if (enq_no == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    ts_timesheet _ts_timesheet_enq = new ts_timesheet();
                    _ts_timesheet_enq = await Task.Run(() => _Repository.getActivityDetailsForOther(enq_no, groupid));
                    return View(_ts_timesheet_enq);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Activity(FormCollection _form)
        {
            if (_UtilitySession.Session != null)
            {
                ViewBag.Name = _UtilitySession.Session.user.name;
                ts_timesheet _ts_timesheet_enq = new ts_timesheet();
                _ts_timesheet_enq.remarks = _form["txtRemarks"].ToString();
                _ts_timesheet_enq.job_no = _form["txtEnquairyNo"].ToString();
                _ts_timesheet_enq.groupid = _form["txtgroupId"].ToString();

                _ts_timesheet_enq = await Task.Run(() => _Repository.setActivityDetailsForOther(_ts_timesheet_enq));
                return View(_ts_timesheet_enq);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Map()
        {
            return View();
        }
    }
}
