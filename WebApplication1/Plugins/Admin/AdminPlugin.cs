using System.Web.Mvc;
using System.Web.Routing;
using Plugin;
using System.Reflection;

namespace Admin
{
    /// <summary>
    /// 内容插件。
    /// </summary>
    public class AdminPlugin : IPlugin
    {
        public string Name
        {
            get
            {
                return "Admin";
            }
        }

        public void Initialize()
        {
            var route = RouteTable.Routes.MapRoute(
                name: "Plugin.Admin",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, pluginName = this.Name }
            );
            route.DataTokens["area"] = this.Name;//设置area的值为Plugin.Name
        }

        public virtual void Unload()
        {
            RouteTable.Routes.Remove(RouteTable.Routes["Plugin.Admin"]);
        }
    }
}
