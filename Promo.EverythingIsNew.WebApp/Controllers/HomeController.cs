using AltLanDS.AllNew.Core;
using AltLanDS.Beeline.DpcProxy.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Promo.EverythingIsNew.DAL;
using Promo.EverythingIsNew.WebApp.Models;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
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

            EntryForm userProfile = await GetUserData(code, MvcApplication.VkAppId, MvcApplication.VkAppSecretKey, MvcApplication.RedirectUri);
            Helpers.EncodeToCookies(userProfile, this.ControllerContext);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Index()
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            ViewBag.Cities = Helpers.GetTestCities();
            return View(userProfile);
        }

        [HttpPost]
        public async Task<ActionResult> Index(EntryForm userProfile)
        {
            UpdateResult result = await MvcApplication.CbnClient.Update(Helpers.MapToUpdate(userProfile));

            //Add ModelState validation messages

            Helpers.EncodeToCookies(userProfile, this.ControllerContext);
            return RedirectToAction("Offer");
        }

        public async Task<ActionResult> Offer()
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            OfferViewModel model = GetTariff(userProfile.FirstName);

            return View(model);
        }

        [HttpPost]
        [ActionName("Offer")]
        public async Task<ActionResult> OfferPost()
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            var result = await MvcApplication.CbnClient.PostMessage(Helpers.MapToMessage(userProfile));

            return Content(userProfile.Email);
        }



        private OfferViewModel GetTariff(string userFirstName)
        {
            Db = new DpcProxyDbContext(MvcApplication.dcpConnectionString); // unity per call
            //var targetTarif = Db.MobileTariffs.FirstOrDefault(t => t.SocName == "12_VSE4M" && t.Regions.Any(r => r.MarketCode == "MarketCode"));
            var targetTarif = Db.MobileTariffs.FirstOrDefault(t => t.SocName == MvcApplication.Soc);

            var groups = targetTarif.DpcProduct.Parameters
                    .GroupBy(g => g.Group.Id, (id, lines) => Helpers.MapTariffGroup(id, lines))
                    .OrderBy(s => s.SortOrder).ToList();

            var model = new OfferViewModel
            {
                UserName = userFirstName,
                TariffName = targetTarif.DpcProduct.MarketingProduct.Title,
                Groups = groups
            };
            return model;
        }



        private static async Task<EntryForm> GetUserData(string code, string vkAppId, string vkAppSecretKey, string redirectUri)
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