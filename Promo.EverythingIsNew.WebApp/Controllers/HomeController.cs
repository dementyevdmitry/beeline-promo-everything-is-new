﻿using AltLanDS.Beeline.DpcProxy.Client;
using Promo.EverythingIsNew.DAL.Vk;
using Promo.EverythingIsNew.Domain;
using Promo.EverythingIsNew.WebApp.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Promo.EverythingIsNew.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public DpcProxyDbContext Db;

        public async Task<ActionResult> Choose()
        {
            ViewBag.PersonalBeelineUrl = MvcApplication.PersonalBeelineUrl;
            return View();
        }

        public async Task<ActionResult> Vk()
        {
            var urlToGetCode = VkHelpers.GetCodeUrl(MvcApplication.VkAppId, MvcApplication.RedirectUri);
            return Redirect(urlToGetCode);
        }

        public async Task<ActionResult> VkResult(string code)
        {
            EntryForm userProfile = Helpers.MapToEntryForm(
                await VkClient.GetUserData(code, MvcApplication.VkAppId, MvcApplication.VkAppSecretKey, MvcApplication.RedirectUri));
            Helpers.EncodeToCookies(userProfile, this.ControllerContext);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Index()
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            var cities = Helpers.GetMarketCodes().Values.ToList();
            ViewBag.Cities = cities;
            ViewBag.SelectedCity = cities.FirstOrDefault(x => x == userProfile.SelectMyCity);
            return View(userProfile);
        }

        [HttpPost]
        public async Task<ActionResult> Index(EntryForm userProfile)
        {
            var oldUserProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            userProfile.Uid = oldUserProfile.Uid;

            // var result = await MvcApplication.CbnClient.Update(Helpers.MapToUpdate(userProfile));
            // Add ModelState validation messages
            // return index page if ModelState is not valid

            // check cbn status and redirect if account is already used

            Helpers.EncodeToCookies(userProfile, this.ControllerContext);
            return RedirectToAction("Offer");
        }

        public async Task<ActionResult> Offer()
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            OfferViewModel model = Helpers.GetOfferViewModel(userProfile.FirstName);

            return View(model);
        }

        [HttpPost]
        [ActionName("Offer")]
        public async Task<ActionResult> OfferPost(string email)
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            if (!string.IsNullOrEmpty(email))
            {
                userProfile.Email = email;
            }
            var result = await MvcApplication.CbnClient.PostMessage(Helpers.MapToMessage(userProfile));
            if (result.is_message_sent == true)
            {
                return Content(userProfile.Email);
            }
            else
            {
                return new HttpStatusCodeResult(400);
            }
        }
    }
}