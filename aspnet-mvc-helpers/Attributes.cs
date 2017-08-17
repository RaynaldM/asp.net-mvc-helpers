using System;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace aspnet_mvc_helpers
{
    #region cache
    /// <inheritdoc />
    /// <summary>
    /// Used attribute to remove cache page for each call to server
    /// Place attribute on controller method
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NoCacheAttribute : ActionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoStore();
            base.OnResultExecuting(filterContext);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Attribut used to say to the browser (and server)
    /// this action is highly cacheable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OptimizedForCacheAttribute : ActionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(30));
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(true);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
            //filterContext.HttpContext.Response.Cache.SetETag(todo);
            base.OnResultExecuting(filterContext);
        }
    }
    #endregion

    #region Ajax
    /// <inheritdoc />
    /// <summary>
    /// Used attribute to filter Ajax call from client
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Format error and exception for a Json usage if it's an ajax request
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AjaxHandleErrorAttribute : HandleErrorAttribute
    {
        /// <inheritdoc />
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.Exception != null)
            {
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        filterContext.Exception.Message,
                        filterContext.Exception.StackTrace
                    }
                };
                filterContext.ExceptionHandled = true;
            }
            else
            {
                base.OnException(filterContext);
            }
        }
    }
    #endregion

    /// <inheritdoc />
    ///  <summary>
    ///  Decorates any MVC route that needs to have client requests limited by time.
    ///  http://stackoverflow.com/questions/33969/best-way-to-implement-request-throttling-in-asp-net-mvc
    ///  [Throttle(Name="TestThrottle", Message = "You must wait {0} seconds before accessing this url again.", Seconds = 5)]
    /// public ActionResult TestThrottle()
    ///  </summary>
    ///  <remarks>
    ///  Uses the current System.Web.Caching.Cache to store each client request to the decorated route.
    ///  </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class ThrottleAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// A unique name for this Throttle.
        /// </summary>
        /// <remarks>
        /// We'll be inserting a Cache record based on this name and client IP, e.g. "Name-192.168.0.1"
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// The number of seconds clients must wait before executing this decorated route again.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// A text message that will be sent to the client upon throttling.  You can include the token {n} to
        /// show this.Seconds in the message, e.g. "Wait {n} seconds before trying again".
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Use Session ID in cache key (if session exist)
        /// </summary>
        public bool UseSessionId { get; set; }

        /// <summary>
        /// Use Session ID in cache key (if session exist)
        /// </summary>
        public bool UseUserAgent { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Attribute contructor, set UseUserAgent=true
        /// </summary>
        public ThrottleAttribute()
        {
            UseUserAgent = true;
        }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext c)
        {
            var key = string.Concat(Name, "-", c.HttpContext.Request.UserHostAddress);
            if (UseUserAgent)
                key += "-" + c.HttpContext.Request.UserAgent;
            if (UseSessionId)
                key += c.HttpContext.Session.SessionID;
            var allowExecute = false;

            if (HttpRuntime.Cache[key] == null)
            {
                HttpRuntime.Cache.Add(key,
                    true, // is this the smallest data we can have?
                    null, // no dependencies
                    DateTime.Now.AddSeconds(Seconds), // absolute expiration
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Low,
                    null); // no callback

                allowExecute = true;
            }

            if (allowExecute) return;
            if (string.IsNullOrEmpty(Message))
                Message = "You may only perform this action every {0} seconds.";

            c.Result = new ContentResult { Content = string.Format(Message, Seconds) };
            // see 429 - https://tools.ietf.org/html/rfc6585ml
            c.HttpContext.Response.StatusCode = 429;
        }
    }

}
