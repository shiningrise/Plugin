namespace Plugin.Contents.Controllers
{
    using System.Web.Mvc;

    using Plugin.Contents.Models;
    using System;

    /// <summary>
    /// 内容控制器。
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult List()
        {
            ContentItem contentItem = new ContentItem { Id = 1, Title = "List" };

            return View(contentItem);
        }

        public ActionResult Index()
        {
            ContentItem contentItem = new ContentItem { Id = 1, Title = "Index" };

            return View(contentItem);
        }
    }
}