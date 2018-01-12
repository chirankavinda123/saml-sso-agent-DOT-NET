﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Configuration;
using Agent;
using NLog;

namespace sample
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {            
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles); 
        }

        public override void Init()
        {
            MapRequestHandler += MvcApplication_MapRequest;
            base.Init();
        }
        void MvcApplication_MapRequest(object sender, EventArgs e)
        {
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        }
    }
}