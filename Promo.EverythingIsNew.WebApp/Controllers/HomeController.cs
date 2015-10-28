using AltLanDS.AllNew.Core;
using AltLanDS.Beeline.DpcProxy.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Promo.EverythingIsNew.DAL;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Promo.EverythingIsNew.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public DpcProxyDbContext Db;

        public ActionResult Choose()
        {
            ViewBag.PersonalBeelineUrl = ConfigurationManager.AppSettings["PersonalBeelineUrl"];
            return View();
        }

        public ActionResult Vk()
        {
            var urlToGetCode = VkHelpers.GetCodeUrl(MvcApplication.VkAppId, MvcApplication.RedirectUri);
            return Redirect(urlToGetCode);
        }

        public ActionResult VkResult(string code)
        {

            EntryForm userProfile = GetUserData(code, MvcApplication.VkAppId, MvcApplication.VkAppSecretKey, MvcApplication.RedirectUri);
            Helpers.EncodeToCookies(userProfile, this.ControllerContext);
            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            ViewBag.Cities = Helpers.GetTestCities();
            return View(userProfile);
        }

        [HttpPost]
        public async Task<ActionResult> Index(EntryForm userProfile)
        {
            UpdateResult result = await CbnValidate(userProfile);
            return RedirectToAction("Offer");
        }

        public async Task<ActionResult> Offer()
        {
            OfferViewModel model = GetTariff();

            return View(model);
        }

        private OfferViewModel GetTariff()
        {
            var model = new OfferViewModel();
            Db = new DpcProxyDbContext(MvcApplication.dcpConnectionString); // unity per call
            //var targetTarif = Db.MobileTariffs.FirstOrDefault(t => t.SocName == "12_VSE4M" && t.Regions.Any(r => r.MarketCode == "MarketCode"));
            var targetTarif = Db.MobileTariffs.FirstOrDefault(t => t.SocName == MvcApplication.Soc);

            var groups = targetTarif.DpcProduct.Parameters
                    .GroupBy(g => g.Group.Id, (id, lines) => Helpers.MapTariffGroup(id, lines)).OrderBy(s => s.SortOrder).ToList();

            model = new OfferViewModel
            {
                UserName = "ххх",
                TariffName = targetTarif.DpcProduct.MarketingProduct.Title,
                Groups = groups
            };
            return model;
        }

        private async Task<UpdateResult> CbnValidate(EntryForm userProfile)
        {
            var model = new Update {
                birth_date = userProfile.Birthday.ToString(),
                ctn = userProfile.CTN,
                email = userProfile.Email,
                email_unsubscribe = (!userProfile.IsMailingAgree),
                name = userProfile.LastName,
                surname = userProfile.LastName,
                region = userProfile.SelectMyCity
            };
            var UpdateResult = await MvcApplication.CbnClient.Update(model);
            return UpdateResult;
        }

        private static EntryForm GetUserData(string code, string vkAppId, string vkAppSecretKey, string redirectUri)
        {
            VkModel userData;
            AccessData accessData;
            using (var client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                client.Headers["Content-Type"] = "application/json";
                client.Encoding = Encoding.UTF8;
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

                var urlToGetAccessData = VkHelpers.GetTokenUrl(code, vkAppId, vkAppSecretKey, redirectUri);
                var accessInfo = client.DownloadString(urlToGetAccessData);
                accessData = JsonConvert.DeserializeObject<AccessData>(accessInfo);

                var urlToGetInfo = VkHelpers.UserApiUrl(accessData);
                var userInfo = client.DownloadString(urlToGetInfo);
                userData = JsonConvert.DeserializeObject<VkModel>(userInfo, new IsoDateTimeConverter { Culture = new CultureInfo("ru-RU") });
            }
            var model = Helpers.MapToEntryForm(userData, accessData);
            return model;
        }








    }
}