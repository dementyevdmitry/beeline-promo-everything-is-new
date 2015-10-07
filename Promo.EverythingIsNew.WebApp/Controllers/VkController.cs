using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Promo.EverythingIsNew.WebApp.Controllers
{
    public class VkController : Controller
    {
        // GET: Vk
        public ActionResult VkInfo()
        {
            try
            {
                var vkAppId = ConfigurationManager.AppSettings["VkAppId"];
                var vkAppSecretKey = ConfigurationManager.AppSettings["VkAppSecretKey"];
                var redirectUri = ConfigurationManager.AppSettings["VkRedirectUri"];

                var userData = new VkModel();
                var code = Request.QueryString["code"];

                var urlToGetCode = "https://oauth.vk.com/authorize?client_id=" + vkAppId + "&display=page&redirect_uri=" + redirectUri + "&scope=email,photos&response_type=code&v=5.37";

                if (code == null)
                {
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Redirect(urlToGetCode);
                }
                else
                {
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
                }
                return View(userData);
            }

            catch (Exception e)
            {
                throw;
            }
        }
    }
}