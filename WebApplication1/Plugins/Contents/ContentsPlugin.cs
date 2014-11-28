using System.Web.Mvc;
using System.Web.Routing;

using System.Diagnostics;
using System;
using System.Reflection;

namespace Plugin.Contents
{
    /// <summary>
    /// 内容插件。
    /// </summary>
    public class ContentsPlugin : IPlugin
    {
        public string Name
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name.Replace("Plugin.", "");
                //return "Contents";
            }
        }

        public void Initialize()
        {

            var route = RouteTable.Routes.MapRoute(
                name: this.Name,
                url: this.Name + "/{controller}/{action}/{id}",
                defaults: new { controller = "Content", action = "Index", id = UrlParameter.Optional, pluginName = this.Name }
            );
            route.DataTokens["area"] = this.Name;//设置area的值为Plugin.Name

            //route.DataTokens["area"] = this.Name;
            //route.DataTokens["pluginName"] = this.Name; 
        }

        public void Unload()
        {
            RouteTable.Routes.Remove(RouteTable.Routes[this.Name]);
        }
    }
}