﻿using System.Web.Mvc;
using System.Web.Routing;

namespace Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute("Account", "Account/{action}", new { controller = "Account" }
    );
            routes.MapRoute("Edit", "Edit", new {controller = "Edit", action = "Index", id = UrlParameter.Optional}
                );
            routes.MapRoute("Default", "{id}", new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
            routes.MapRoute("Pages", "Pages/{controller}/{action}/{id}",
                new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}
