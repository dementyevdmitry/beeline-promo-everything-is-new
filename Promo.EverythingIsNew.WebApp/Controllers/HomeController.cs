using AltLanDS.AllNew.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Promo.EverythingIsNew.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Choose()
        {
            ViewBag.PersonalBeelineUrl = ConfigurationManager.AppSettings["PersonalBeelineUrl"];
            return View();
        }

        public ActionResult Vk()
        {
            string vkAppId;
            string vkAppSecretKey;
            string redirectUri;
            LoadParams(out vkAppId, out vkAppSecretKey, out redirectUri);
            var urlToGetCode = GetCodeUrl(vkAppId, redirectUri);
            return Redirect(urlToGetCode);
        }


        public ActionResult VkResult(string code)
        {
            string vkAppId;
            string vkAppSecretKey;
            string redirectUri;
            LoadParams(out vkAppId, out vkAppSecretKey, out redirectUri);
            TempData["userProfile"] = GetUserData(code, vkAppId, vkAppSecretKey, redirectUri);
            return RedirectToAction("Index");
        }




        public ActionResult Index()
        {
            var model = (EntryForm)TempData["userProfile"];
            return View(model);
        }

        public ActionResult Offer()
        {
            return View();
        }

        private static void LoadParams(out string vkAppId, out string vkAppSecretKey, out string redirectUri)
        {
            vkAppId = ConfigurationManager.AppSettings["VkAppId"];
            vkAppSecretKey = ConfigurationManager.AppSettings["VkAppSecretKey"];
            var hostname = ConfigurationManager.AppSettings["RedirectHostname"];
            redirectUri = hostname + (hostname.Substring(hostname.Length - 1, 1) == "/" ? "VkResult" : "/VkResult");
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

                var urlToGetAccessData = GetTokenUrl(code, vkAppId, vkAppSecretKey, redirectUri);
                var accessInfo = client.DownloadString(urlToGetAccessData);
                accessData = JsonConvert.DeserializeObject<AccessData>(accessInfo);

                var urlToGetInfo = UserApiUrl(accessData);
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
                    City = x.City.Title,
                    Email = accessData.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    CTN = x.Phone,
                }).FirstOrDefault();
            return model;
        }

        private static string UserApiUrl(AccessData accessData)
        {
            var urlToGetInfo = "https://api.vk.com/method/users.get?user_id=" + accessData.UserId + "&fields=bdate,city,education,contacts&v=5.37&access_token=" + accessData.AccessToken + "&x=" + DateTime.Now.Ticks;
            return urlToGetInfo;
        }

        private static string GetTokenUrl(string code, string vkAppId, string vkAppSecretKey, string redirectUri)
        {
            var urlToGetAccessData = "https://oauth.vk.com/access_token?client_id=" + vkAppId + "&client_secret=" + vkAppSecretKey + "&redirect_uri=" + redirectUri + "&code=" + code + "&x=" + DateTime.Now.Ticks;
            return urlToGetAccessData;
        }

        private static string GetCodeUrl(string vkAppId, string redirectUri)
        {
            var urlToGetCode = "https://oauth.vk.com/authorize?client_id=" + vkAppId + "&display=page&redirect_uri=" + redirectUri + "&scope=email&response_type=code&v=5.37" + "&x=" + DateTime.Now.Ticks;
            return urlToGetCode;
        }



    }
}