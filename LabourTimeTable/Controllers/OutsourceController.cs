using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LabourTimeTable.Models;

namespace LabourTimeTable.Controllers
{
    public class OutsourceController : Controller
    {
        //
        // GET: /Outsource/
        UtilitySession _UtilitySession = new UtilitySession();
        Repository _Repository = new Repository();

        public ActionResult RegisterOutsourceTeam()
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;


                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
            }
            return View();
        }

        [HttpPost]
        public ActionResult RegisterOutsourceTeam(OutsourcePostDTO _OutsourcePostDTO)
        {
            try
            {
                if (_UtilitySession.Session != null)
                {
                    ViewBag.Name = _UtilitySession.Session.user.name;
                    bool x = _Repository.saveOutsourcedEmployee(_OutsourcePostDTO);

                    if (x == true)
                    {
                        ViewBag.Error = "Added Successfully";
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
            }
            return View();
        }


        //[HttpGet]
        //public virtual async Task<ActionResult> ifEmiratesIdExist(string EmiratesID)
        //{
        //    try
        //    {
        //        ts_emirates_id _ts_emirates_id = new ts_emirates_id();

        //        if (_UtilitySession.Session != null)
        //        {
        //            ViewBag.Name = _UtilitySession.Session.user.name;
        //            _ts_emirates_id = await Task.Run(() => _Repository.CheckIfLaborWithEmiratesIDExist(EmiratesID));
        //        }
        //        else
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ViewBag.Error = e.Message;
        //    }
        //    return View();
        //}


        public virtual async Task<JsonResult> ifEmiratesIdExist(string EmiratesID)
        {
            ts_emirates_id _ts_emirates_id = new ts_emirates_id();
            if (_UtilitySession.Session != null)
            {
                _ts_emirates_id = await Task.Run(() => _Repository.CheckIfLaborWithEmiratesIDExist(EmiratesID));
                if (_ts_emirates_id != null)
                {
                    return Json(_ts_emirates_id, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("1", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                RedirectToAction("Index", "Home");
                return Json(null);
            }
        }



    }
}
