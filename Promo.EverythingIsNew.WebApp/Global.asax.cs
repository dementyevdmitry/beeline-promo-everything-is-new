using FullScale180.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Promo.EverythingIsNew.DAL;
using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Promo.EverythingIsNew.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static string VkAppId = ConfigurationManager.AppSettings["VkAppId"];
        public static string VkAppSecretKey = ConfigurationManager.AppSettings["VkAppSecretKey"];
        public static string Hostname = ConfigurationManager.AppSettings["RedirectHostname"];
        public static string RedirectUri = Hostname + (Hostname.Substring(Hostname.Length - 1, 1) == "/" ? "VkResult" : "/VkResult");
        public static string PersonalBeelineUrl = ConfigurationManager.AppSettings["PersonalBeelineUrl"];

        public static string UsssUrl = ConfigurationManager.AppSettings["UsssUrl"];
        public static string UsssUser = ConfigurationManager.AppSettings["UsssUser"];
        public static string UsssPassword = ConfigurationManager.AppSettings["UsssPassword"];

        public static CbnClient CbnClient = new CbnClient(UsssUrl, UsssUser, UsssPassword);

        public static string Soc = ConfigurationManager.AppSettings["Soc"];
        public static string _siteUrlFormat = ConfigurationManager.AppSettings["altlands:dpc:site-url"];
        public static string dcpConnectionString = ConfigurationManager.AppSettings["DcpConnectionString"];

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            var listener = new ObservableEventListener();
            listener.EnableEvents(TestEvents.Log, EventLevel.LogAlways, Keywords.All);

            listener.LogToConsole();
            listener.LogToElasticsearch("SLABEL", "http://localhost:9200", "myindex", "mytype", bufferingCount: 1);

            TestEvents.Log.Critical("Hello world In-Process Critical");
            TestEvents.Log.Error("Hello world In-Process Error");
            TestEvents.Log.Informational("Hello world In-Process Informational");

#if DEBUG
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#endif
        }
    }
}
