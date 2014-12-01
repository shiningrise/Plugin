using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Plugin.Admin
{
    public class ContentsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Contents";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            var route = context.MapRoute(
                name: "Plugin_Contents",
                url: "Contents/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}