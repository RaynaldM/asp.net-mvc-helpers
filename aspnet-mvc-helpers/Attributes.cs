using System;
using System.Web;
using System.Web.Mvc;

namespace aspnet_mvc_helpers
{
    /// <summary>
    /// Used attribute to remove cache page for each call to server
    /// Place attribute on controller method
    /// </summary>
    public class NoCache : ActionFilterAttribute
    {
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

    /// <summary>
    /// Used attribute to filter Ajax call from client
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
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

    /// <summary>
    /// Attribut used to say to the browser (and server)
    /// this action is highly cacheable
    /// </summary>
    public class OptimizedForCache : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(30));
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(true);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
            //filterContext.HttpContext.Response.Cache.SetETag(todo);
            base.OnResultExecuting(filterContext);
        }
    }

}
