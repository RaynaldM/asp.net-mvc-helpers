﻿using System;
using System.Threading;
using System.Web.Mvc;

namespace aspnet_mvc_helpers
{
    /// <inheritdoc />
    /// <summary>
    /// A base controller with a culture thread setter 
    /// </summary>
    public abstract class GlobalController : Controller
    {
        // Thank to http://afana.me/post/aspnet-mvc-internationalization.aspx
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            // Attempt to read the culture cookie from Request
            var cultureCookie = Request.Cookies["_culture"];
            var cultureName = cultureCookie != null
                ? cultureCookie.Value
                : (Request.UserLanguages != null && Request.UserLanguages.Length > 0
                    ? Request.UserLanguages[0]
                    : // obtain it from HTTP header AcceptLanguages
                    null);

            // Validate culture name
            cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

            // Modify current thread's cultures            
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            return base.BeginExecuteCore(callback, state);
        }
    }
}
