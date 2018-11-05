using LabourTimeTable.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LabourTimeTable.Controllers
{
    public class ComplaintController : Controller
    {
        //
        // GET: /Complaint/
        UtilitySession _UtilitySession = new UtilitySession();
        UtilityComplainSession _UtilityComplainSession = new UtilityComplainSession();
        Repository _Repository = new Repository();

        public async Task<ActionResult> Complaint()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    TimesheetEntities _TimesheetEntities = new TimesheetEntities();

                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    LabourComplaintDetailDTO _LabourComplaintDetailDTO = new LabourComplaintDetailDTO();
                    _LabourComplaintDetailDTO.user = _UtilitySession.Session.user;

                    _LabourComplaintDetailDTO._TimesheetComGridPostDTO = await Task.Run(() => _Repository.getComplaintTimesheetData(User_id));
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

        [HttpPost]
        public async Task<ActionResult> Complaint(TimesheetComPostDTO _TimesheetComPostDTO)
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    LabourComplaintDetailDTO _LabourComplaintDetailDTO = new LabourComplaintDetailDTO();
                    List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO = new List<TimesheetComGridPostDTO>();
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    List<ts_timesheet> _ts_timesheet_com = new List<ts_timesheet>();
                    ts_user _ts_user = new ts_user();
                    _ts_user = _UtilitySession.Session.user;
                    _TimesheetComGridPostDTO = await Task.Run(() => _Repository.setTimesheetComData(_TimesheetComPostDTO, _ts_user));
                    _LabourComplaintDetailDTO._TimesheetComGridPostDTO = _TimesheetComGridPostDTO;


                    return View(_LabourComplaintDetailDTO);
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

        public async Task<JsonResult> getComplainList()
        {

            List<ComplainDTO> _ComplainDTOList = new List<ComplainDTO>();

            try
            {
                _ComplainDTOList = await Task.Run(() => _Repository.getUnsolvedComplainList());

                return Json(_ComplainDTOList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }

        public async Task<ActionResult> getAllActivitiesList(string com_no)
        {
            List<WorkInProgress> _WorkInProgress = new List<WorkInProgress>();

            try
            {
                ActivityHistoryDTO _ActivityHistoryDTO = new ActivityHistoryDTO();
                _ActivityHistoryDTO._WorkInProgress = await Task.Run(() => _Repository.getAllActivitiesPerComplain(com_no));
                return PartialView("_PartialViewForGridAcivityHistory", _ActivityHistoryDTO);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View("Complaint", "Complaint");
            }
        }

        public async Task<ActionResult> AllCheckOut(string CheckoutTime)
        {
            LabourComplaintDetailDTO _LabourComplaintDetailDTO = new LabourComplaintDetailDTO();
            if (_UtilitySession.Session != null)
            {
                ViewBag.Name = _UtilitySession.Session.user.name;
                List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO = new List<TimesheetComGridPostDTO>();

                _UtilitySession.Session._TimesheetGridDTO = null;
                ts_user _ts_user = new ts_user();
                _ts_user = _UtilitySession.Session.user;
                _TimesheetComGridPostDTO = await Task.Run(() => _Repository.AllComplainCheckOut(_ts_user, CheckoutTime));
                _UtilityComplainSession.Session._TimesheetComGridPostDTO = _TimesheetComGridPostDTO;

                _LabourComplaintDetailDTO._TimesheetComGridPostDTO = _TimesheetComGridPostDTO;

                _LabourComplaintDetailDTO._TimesheetComGridPostDTO = _UtilityComplainSession.Session._TimesheetComGridPostDTO;
                _LabourComplaintDetailDTO.PageSize = 5;
                _LabourComplaintDetailDTO.TotalCount = _UtilitySession.Session._TimesheetGridDTO.Count;

                return View(_LabourComplaintDetailDTO);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public JsonResult CheckIfUserIsAdmin(int? empid)
        {
            List<ts_timesheet_com> _ts_timesheet_com = new List<ts_timesheet_com>();
            if (_UtilitySession.Session != null)
            {
                if (empid == _UtilitySession.Session.user.emp_id)
                {
                    _ts_timesheet_com = _Repository.CheckComplainUserIsAdmin(_UtilitySession.Session.user.id);
                }
                return Json(_ts_timesheet_com, JsonRequestBehavior.AllowGet);
            }
            else
            {
                RedirectToAction("Index", "Home");
                return Json(null);
                //return RedirectToAction("Index", "Home");
            }
        }

        public async Task<ActionResult> IndividualCheckOut(int? empid, string checkOutTime)
        {
            LabourComplaintDetailDTO _LabourComplaintDetailDTO = new LabourComplaintDetailDTO();

            if (_UtilitySession.Session != null)
            {
                ViewBag.Name = _UtilitySession.Session.user.name;
                List<TimesheetComGridPostDTO> _TimesheetComGridPostDTO = new List<TimesheetComGridPostDTO>();
                ts_user _ts_user = new ts_user();
                _ts_user = _UtilitySession.Session.user;
                _TimesheetComGridPostDTO = await Task.Run(() => _Repository.IndividualComplainCheckOut(_ts_user, empid, checkOutTime));

                return View(_LabourComplaintDetailDTO);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<ActionResult> Activity(string com_no, string groupid)
        {
            if (_UtilitySession.Session != null)
            {
                if (com_no == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ComplainDTO _complaint = new ComplainDTO();
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    _complaint = await Task.Run(() => _Repository.getComplainActivityDetails(com_no));
                    return View(_complaint);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Activity(string is_solved, string com_no, string techproblem, string techsolution, string remarks, string reason)
        {



            if (_UtilitySession.Session != null)
            {
                if (com_no == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ComplainDTO _complaint = new ComplainDTO();
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    string User_id = _UtilitySession.Session.user.id;
                    var sx = await Task.Run(() => _Repository.SubmitComplainActivity(is_solved, com_no, techproblem, techsolution, remarks, reason));
                    return View(_complaint);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
    }
}
