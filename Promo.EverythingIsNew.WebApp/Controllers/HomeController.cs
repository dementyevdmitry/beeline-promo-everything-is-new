﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
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
            var vkAppId = ConfigurationManager.AppSettings["VkAppId"];
            var vkAppSecretKey = ConfigurationManager.AppSettings["VkAppSecretKey"];
            var redirectUri = ConfigurationManager.AppSettings["Hostname"] + "/VkResult";

            var urlToGetCode = "https://oauth.vk.com/authorize?client_id=" + vkAppId + "&display=page&redirect_uri=" + redirectUri + "&scope=email&response_type=code&v=5.37";


            return Redirect(urlToGetCode);
        }
        public ActionResult VkResult()
        {
            var vkAppId = ConfigurationManager.AppSettings["VkAppId"];
            var vkAppSecretKey = ConfigurationManager.AppSettings["VkAppSecretKey"];
            var redirectUri = ConfigurationManager.AppSettings["Hostname"] + "/VkResult";

            using (var client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                client.Headers["Content-Type"] = "application/json";
                client.Encoding = Encoding.UTF8;

                var urlToGetAccessData = "https://oauth.vk.com/access_token?client_id=" + vkAppId + "&client_secret=" + vkAppSecretKey + "&redirect_uri=" + redirectUri + "&code=" + code;
                var accessInfo = client.DownloadString(urlToGetAccessData);
                var accessData = JsonConvert.DeserializeObject<AccessData>(accessInfo);

                var urlToGetInfo = "https://api.vk.com/method/users.get?user_id=" + accessData.UserId + "&fields=bdate,city,country,personal&v=5.37&access_token=" + accessData.AccessToken;
                var userInfo = client.DownloadString(urlToGetInfo);
                userData = JsonConvert.DeserializeObject<VkModel>(userInfo, new IsoDateTimeConverter { Culture = new CultureInfo("ru-RU") });
                userData.Response.FirstOrDefault().Email = accessData.Email;
            }

            return View();
        }

        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Offer()
        {
            return View();
        }

       
    }
}