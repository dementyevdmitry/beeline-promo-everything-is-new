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
            var urlToGetCode = "https://oauth.vk.com/authorize?client_id=" + vkAppId + "&display=page&redirect_uri=" + redirectUri + "&scope=email&response_type=code&v=5.37";
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
            var model = (UserProfile)TempData["userProfile"];

            return View();
        }

        public ActionResult Offer()
        {
            return View();
        }

        private static void LoadParams(out string vkAppId, out string vkAppSecretKey, out string redirectUri)
        {
            vkAppId = ConfigurationManager.AppSettings["VkAppId"];
            vkAppSecretKey = ConfigurationManager.AppSettings["VkAppSecretKey"];
            var hostname = ConfigurationManager.AppSettings["Hostname"];
            redirectUri = hostname + (hostname.Substring(hostname.Length - 1, 1) == "/" ? "VkResult" : "/VkResult");
        }

        private static UserProfile GetUserData(string code, string vkAppId, string vkAppSecretKey, string redirectUri)
        {
            VkModel userData;
            using (var client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                client.Headers["Content-Type"] = "application/json";
                client.Encoding = Encoding.UTF8;
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

                var urlToGetAccessData = "https://oauth.vk.com/access_token?client_id=" + vkAppId + "&client_secret=" + vkAppSecretKey + "&redirect_uri=" + redirectUri + "&code=" + code;
                var accessInfo = client.DownloadString(urlToGetAccessData);
                var accessData = JsonConvert.DeserializeObject<AccessData>(accessInfo);

                var urlToGetInfo = "https://api.vk.com/method/users.get?user_id=" + accessData.UserId + "&fields=bdate,city,education,contacts&v=5.37&access_token=" + accessData.AccessToken;
                var userInfo = client.DownloadString(urlToGetInfo);
                userData = JsonConvert.DeserializeObject<VkModel>(userInfo, new IsoDateTimeConverter { Culture = new CultureInfo("ru-RU") });
                userData.Response.FirstOrDefault().Email = accessData.Email;
            }
            

            var userData2 = userData.Response.FirstOrDefault();
            var model = new UserProfile { 
                //FIO  = userData2.FirstName + " " + userData2.LastName,
                //Academy =userData2.a
            };

            return model;
        }



    }
}