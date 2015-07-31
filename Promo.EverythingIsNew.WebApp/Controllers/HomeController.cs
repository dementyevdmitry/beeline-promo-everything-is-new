using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Promo.EverythingIsNew.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var vkAppId = ConfigurationManager.AppSettings["VkAppId"];
            var vkAppSecretKey = ConfigurationManager.AppSettings["VkAppSecretKey"];

            return View();
        }

       
    }
}