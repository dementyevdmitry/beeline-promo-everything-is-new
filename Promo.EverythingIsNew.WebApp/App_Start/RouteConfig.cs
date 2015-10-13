using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Promo.EverythingIsNew.WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "choose",
                url: "choose",
                defaults: new { controller = "home", action = "choose"}
            );

            routes.MapRoute(
                name: "index",
                url: "index",
                defaults: new { controller = "home", action = "index" }
            );

            routes.MapRoute(
                name: "offer",
                url: "offer",
                defaults: new { controller = "home", action = "offer" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "choose", id = UrlParameter.Optional }
            );
        }
    }
}
