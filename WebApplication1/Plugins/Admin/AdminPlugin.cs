namespace Admin
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using Plugin;
    using System.Reflection;

    /// <summary>
    /// 内容插件。
    /// </summary>
    public class AdminPlugin : IPlugin
    {
        public string Name
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name.Replace("Plugin.", "");
                //return "Admin";
            }
        }

        public void Initialize()
        {
            //RouteTable.Routes.MapRoute(
            //    "Default",                                              // Route name
            //    "{controller}/{action}/{id}",                           // URL with parameters
            //    new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            //);

            //RouteTable.Routes.MapRoute(
            //    name: this.Name,
            //    url: this.Name + "/{controller}/{action}/{id}",
            //    defaults: new { controller = "Content", action = "Index", id = UrlParameter.Optional, pluginName = this.Name }
            //);

            RouteTable.Routes.MapRoute(
                name: "Admin",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, pluginName = this.Name }
            );
        }

        public virtual void Unload()
        {
            RouteTable.Routes.Remove(RouteTable.Routes[this.Name]);
        }
    }
}