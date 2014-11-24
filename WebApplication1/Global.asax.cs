using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
#if DEBUG
            //var logfile = string.Format("{0}App_Data\\{1:yyMMdd}.log", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, DateTime.Now);
            var logfile = string.Format("{0}App_Data\\{1:yyMMdd_hhmmss}.log", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, DateTime.Now);
            var traceListener = new TextWriterTraceListener(logfile);
            Debug.Listeners.Add(traceListener);
            Debug.AutoFlush = true;
            Debug.IndentSize = 4;
#endif

            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinderProvider();

            // Register the EntLib classes.
            //builder.RegisterEnterpriseLibrary();

            // Set the MVC dependency resolver to use Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
