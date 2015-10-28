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
            EncodeToCookies(userProfile);
            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            var userProfile = DecodeFromCookies();
            ViewBag.Cities = GetCities();
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
            Db = new DpcProxyDbContext(MvcApplication.dcpConnectionString); // unity per call
            //var targetTarif = Db.MobileTariffs.FirstOrDefault(t => t.SocName == "12_VSE4M" && t.Regions.Any(r => r.MarketCode == "MarketCode"));
            var targetTarif = Db.MobileTariffs.FirstOrDefault(t => t.SocName == MvcApplication.Soc);
            //var webEntity = targetTarif.DpcProduct.ProductWebEntities.FirstOrDefault(x => x.WebEntity.SOC == MvcApplication.Soc);

            var groups = targetTarif.DpcProduct.Parameters
                    .GroupBy(g => g.Group.Id, (id, lines) => MapTariffGroup(id, lines)).OrderBy(s => s.SortOrder).ToList();

            var model = new OfferViewModel
            {
                UserName = "ххх",
                TariffName = targetTarif.DpcProduct.MarketingProduct.Title,
                Groups = groups
            };

            string json = JsonConvert.SerializeObject(targetTarif);
            var pattern = "\\r\\n";
            json = json.Replace(pattern, "<br />");

            string json2 = JsonConvert.SerializeObject(model);

            return View(model);
            //return Content(json);
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
            var model = MapToEntryForm(userData, accessData);
            return model;
        }

        private static EntryForm MapToEntryForm(VkModel userData, AccessData accessData)
        {
            var model = userData.Response.Select(x =>
                new EntryForm
                {
                    Academy = x.Academy,
                    Birthday = x.Birthday,
                    SelectMyCity = x.City.Title,
                    Email = accessData.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    CTN = x.Phone,
                }).FirstOrDefault();
            return model;
        }

        private void EncodeToCookies(EntryForm userProfile)
        {
            var cookie = new HttpCookie("UserProfile");
            var json = JsonConvert.SerializeObject(userProfile);
            var bytes = Encoding.Unicode.GetBytes(json);
            var encoded = MachineKey.Protect(bytes);
            var base64 = Convert.ToBase64String(encoded);
            cookie.Value = base64;
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        private EntryForm DecodeFromCookies()
        {
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("UserProfile"))
            {
                var cookie = this.ControllerContext.HttpContext.Request.Cookies["UserProfile"];
                var encoded = Convert.FromBase64String(cookie.Value);
                var decoded = MachineKey.Unprotect(encoded);
                var json = Encoding.Unicode.GetString(decoded);
                EntryForm userProfile = JsonConvert.DeserializeObject<EntryForm>(json);
                return userProfile;
            }
            return null;
        }

        private List<string> GetCities()
        {
            List<string> items = new List<string>();
            items.Add("Москва");
            items.Add("Санкт-Петербург");
            items.Add("Петропавловск-Камчатский");
            items.Add("Воронеж");
            return items;
        }

        private static TariffGroupViewModel MapTariffGroup(int id, IEnumerable<AltLanDS.Beeline.DpcProxy.Client.Dpc.ProductParameter> lines)
        {
            return new TariffGroupViewModel
            {
                Id = id,
                Name = lines.FirstOrDefault().Group.Title,
                SortOrder = lines.FirstOrDefault().Group.SortOrder,
                Lines = lines.Select(l => MapTariffLine(l)).OrderBy(s => s.SortOrder).ToList()
            };
        }

        private static TariffLineViewModel MapTariffLine(AltLanDS.Beeline.DpcProxy.Client.Dpc.ProductParameter l)
        {
            return new TariffLineViewModel
            {
                Title = l.Title,
                NumValue = l.NumValue.ToString(),
                UnitDisplay = (l.Unit != null) ? l.Unit.Display : null,
                Value =  l.Value,
                SortOrder = l.SortOrder
            };
        }
    }
}