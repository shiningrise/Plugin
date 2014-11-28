Plugin
======

软件插件化 
- nuget网址：http://www.nuget.org/packages/Plugin.Mvc
- mvc5程序nuget安装：Install-Package Plugin.Mvc 
- mvc5插件模块nuget安装：Install-Package Plugin.Interfaces  
- 插件项目只能放在主程序的Plugins目录下
- 插件名称 IPlugin.Name 必需为插件目录的名称

``` 插件类实例

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

```

mvc4请访问：https://github.com/shiningrise/PluginMvcWeb

