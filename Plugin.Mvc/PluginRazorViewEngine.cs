using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.WebPages.Razor;

namespace Plugin.Mvc
{

    /// <summary>
    /// 
    /// </summary>
    public class PluginRazorViewEngine : ThemeableVirtualPathProviderViewEngine
    {
        /// <summary>
        /// 定义区域视图页所在地址。
        /// </summary>
        private string[] _areaViewLocationFormats = new[]
        {
            "~/Plugins/{2}/Views/{1}/{0}.cshtml",
            "~/Plugins/{2}/Views/Shared/{0}.cshtml",
            "~/{2}/Views/{1}/{0}.cshtml",
            "~/{2}/Views/Shared/{0}.cshtml"
        };

        /// <summary>
        /// 定义视图页所在地址。
        /// </summary>
        private string[] _viewLocationFormats = new[]
        {
            "~/Views/{1}/{0}.cshtml",
            "~/Views/Shared/{0}.cshtml",
        };

        public PluginRazorViewEngine()
        {
            this.AreaViewLocationFormats = this._areaViewLocationFormats;
            this.AreaMasterLocationFormats = this._areaViewLocationFormats;
            this.AreaPartialViewLocationFormats = this._areaViewLocationFormats;

            this.ViewLocationFormats = this._viewLocationFormats;
            this.MasterLocationFormats = this._viewLocationFormats;
            this.PartialViewLocationFormats = this._viewLocationFormats;
        }


    }
}