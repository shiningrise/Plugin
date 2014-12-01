[assembly: System.Web.PreApplicationStartMethod(typeof(PluginMvc.Bootstrapper), "Initialize")]

namespace PluginMvc
{
    
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Linq;
    using System.Web;
    using System.Web.Compilation;

    /// <summary>
    /// 引导程序。
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        public static void Initialize()
        {
            //注册插件控制器工厂。
            //ControllerBuilder.Current.SetControllerFactory(new PluginControllerFactory());

            //注册插件模板引擎。
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new PluginRazorViewEngine());

            //初始化插件。
            PluginManager.Initialize();

            //启动插件检测器。
            //PluginWatcher.Start();
        }

    }
}