using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Promo.EverythingIsNew.WebApp.Controllers
{
    public class WorkflowSampleController : Controller
    {
        // GET: WorkflowSample
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult ReturnUrl()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string token)
        {
            // GetUserProfile();

            // CheckIsAlreadyUidParticipation();
            // return RedirectToAction("Remind");

            // CheckUrlParams();
            // LoadRegionsDirectory();

            return RedirectToAction("EditForm");
        }

        
        public ActionResult EditForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditForm(string form)
        {
            // CheckAgeRestrictions();
            // CheckRegionRestrictions();
            // ValidateEmail();
            // ValidateRegion();

            // CheckCtnBelongsToBeeline();
            // return RedirectToAction("PersonalBeeline");

            // CheckIsAlreadyCtnParticipation();
            // return RedirectToAction("PersonalBeeline");

            return View("Offer");
        }


        public ActionResult Offer()
        {
            // LoadTariffByMarketCode();
            return View();
        }

        [HttpPost]
        public ActionResult Offer(string result)
        {
            // CheckAccordance();
            // return RedirectToAction("PersonalBeeline");

            return View("SendPromoCode");
        }

        public ActionResult SendPromoCode()
        {
            return View();
        }

        public ActionResult RepeatSendPromoCode()
        {
            return View();
        }

        public ActionResult LogOff()
        {
            return View();
        }




    }
}