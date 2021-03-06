﻿using FullScale180.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Promo.EverythingIsNew.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
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
        }
    }
}
