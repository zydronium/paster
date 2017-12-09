using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Paster
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Paste",
                url: "p/{ident}",
                defaults: new { controller = "Pastes", action = "Show", ident = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Pastes", action = "Create" }
            );
        }
    }
}
