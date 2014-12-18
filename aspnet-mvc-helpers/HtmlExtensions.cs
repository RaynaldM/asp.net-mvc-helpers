using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Optimization;

namespace aspnet_mvc_helpers
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Create a checkbox in BootStrap Standard
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML context</param>
        /// <param name="expression">The expression.</param>
        /// <returns>Checkbox</returns>
        public static MvcHtmlString BootStrapCheckBoxFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            const string domElement = @"<div class='checkbox'><label for='{0}'><input name='{0}' id='{0}' type='checkbox' value='{1}'{2}><input type='hidden' value='{4}' name='{0}'>{3}</label></div>";

            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var isChecked = false;
            if (metadata.Model != null)
            {
                bool modelChecked;
                if (Boolean.TryParse(metadata.Model.ToString(), out modelChecked))
                {
                    isChecked = modelChecked;
                }
            }

            var result = String.Format(domElement, metadata.PropertyName, (!isChecked).ToString().ToLower()/*metadata.Model*/,
                isChecked ? " checked='checked'" : String.Empty,
                metadata.DisplayName ?? metadata.PropertyName,
                isChecked.ToString().ToLower());

            return new MvcHtmlString(result);
        }

        /// <summary>
        /// Helpers : set the mail type on an input 
        /// </summary>
        /// <typeparam name="TModel">Exposed model</typeparam>
        /// <typeparam name="TProperty">Property type of field</typeparam>
        /// <param name="html">Html context</param>
        /// <param name="expression">Model expression</param>
        /// <param name="htmlAttributes">Additional attributes</param>
        /// <returns></returns>
        public static MvcHtmlString EmailFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, Object htmlAttributes)
        {
            // unsupported by safari
            var emailfor = html.TextBoxFor(expression, htmlAttributes);
            return new MvcHtmlString(emailfor.ToHtmlString().Replace("type=\"text\"", "type=\"email\""));
        }

        public static MvcHtmlString TypeScript(this HtmlHelper helper, String url)
        {
#if DEBUG
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(@"TypeScript Helper");
#endif

            var suffixPos = url.LastIndexOf('.');
#if DEBUG
            if (suffixPos < 0 || url.Substring(suffixPos) != ".ts")
                throw new ArgumentException("TypeScript Helper : bad name or bad suffix");
#endif

            var realName = url.Substring(0, suffixPos);

#if DEBUG
            return new MvcHtmlString(String.Format("<script src='{0}'></script>",
                System.Web.VirtualPathUtility.ToAbsolute(realName + ".js")));
#else
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(realName);
            if (bundleUrl == null)
            {
                var jsBundle = new ScriptBundle(realName);
                jsBundle.Include(realName + ".js");
                BundleTable.Bundles.Add(jsBundle);
                bundleUrl = BundleTable.Bundles.ResolveBundleUrl(realName);
            }
            return new MvcHtmlString(String.Format("<script src='{0}'></script>", bundleUrl));
#endif
        }

        public static MvcHtmlString JQueryVal(this HtmlHelper helper)
        {
#if !DEBUG
            const string script = "<script src='{0}'></script>";

            return new MvcHtmlString(String.Format(script, "http://ajax.aspnetcdn.com/ajax/jquery.validate/1.13.1/jquery.validate.min.js") +
                String.Format(script, "http://ajax.aspnetcdn.com/ajax/mvc/5.1/jquery.validate.unobtrusive.min.js"));
#else
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl("~/bundles/jqueryval");
            return new MvcHtmlString(String.Format("<script src='{0}'></script>", bundleUrl));
#endif
        }

        public static MvcHtmlString Css(this HtmlHelper helper, String name)
        {
#if !DEBUG
           name = name.Substring(0, name.IndexOf('.') ) + ".min.css";
#endif
            return new MvcHtmlString(String.Format("<link rel='stylesheet' href='{0}'>", System.Web.VirtualPathUtility.ToAbsolute(name)));
        }

        public static MvcHtmlString ResourcesJS(this HtmlHelper helper, String language = "en")
        {
            const string resxName = "~/bundles/resources";
            var culture = helper.BrowserCulture();
            String fullLang = String.Empty;
            String isoLang = "." + language;

            if (culture.Name != language)
            {
                fullLang = "_" + culture.Name;
                isoLang = "_" + culture.TwoLetterISOLanguageName;
            }

            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(resxName + fullLang) ??
                            BundleTable.Bundles.ResolveBundleUrl(resxName + isoLang) ??
                            BundleTable.Bundles.ResolveBundleUrl(resxName);

            return new MvcHtmlString(String.Format("<script src='{0}'></script>", bundleUrl));
        }

        public static MvcHtmlString AnalyticsScript(this HtmlHelper helper)
        {
            var googlekey = WebConfigurationManager.AppSettings["GoogleAnalytics"];
            var applicationInsightsKey = WebConfigurationManager.AppSettings["applicationInsights"];

            if (!String.IsNullOrEmpty(applicationInsightsKey) || !String.IsNullOrEmpty(googlekey))
            {
                var script = new StringBuilder("<script type='text/javascript'>");
                if (!String.IsNullOrEmpty(applicationInsightsKey))
                {
                    script.AppendFormat("window.applicationInsightsKey='{0}';", applicationInsightsKey);
                }
#if !DEBUG
                if (!String.IsNullOrEmpty(googlekey))
                {
                    script.AppendFormat("window.googlekey='{0}';", googlekey);
                }
#endif
                script.Append("</script>");
                script.Append(Scripts.Render("~/bundles/analytics"));
                return new MvcHtmlString(script.ToString());
            }
            return null;
        }

        public static CultureInfo BrowserCulture(this HtmlHelper helper)
        {
            var userLanguages = helper.ViewContext.HttpContext.Request.UserLanguages;
            CultureInfo ci;
            if (userLanguages != null && userLanguages.Any())
            {
                try
                {
                    ci = new CultureInfo(userLanguages[0]);
                }
                catch (CultureNotFoundException)
                {
                    ci = CultureInfo.InvariantCulture;
                }
            }
            else
            {
                ci = CultureInfo.InvariantCulture;
            }
            return ci;
        }

        public static MvcHtmlString CompressedPartial(this HtmlHelper htmlHelper, string partialViewName)
        {
            return CompressedPartial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData);
        }

        public static MvcHtmlString CompressedPartial(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData)
        {
            return CompressedPartial(htmlHelper, partialViewName, null /* model */, viewData);
        }

        public static MvcHtmlString CompressedPartial(this HtmlHelper htmlHelper, string partialViewName, object model)
        {
            return CompressedPartial(htmlHelper, partialViewName, model, htmlHelper.ViewData);
        }

        public static MvcHtmlString CompressedPartial(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData)
        {
            var result = htmlHelper.RenderPartialInternal(partialViewName, viewData, model, ViewEngines.Engines);
            return MvcHtmlString.Create(result);
        }

        private static string RenderPartialInternal(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData, object model, ViewEngineCollection viewEngineCollection)
        {
            if (String.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException(@"Empty view", "partialViewName");
            }

            ViewDataDictionary newViewData = model == null
                ? (viewData == null ? new ViewDataDictionary(htmlHelper.ViewData) : new ViewDataDictionary(viewData))
                : (viewData == null ? new ViewDataDictionary(model) : new ViewDataDictionary(viewData) { Model = model });

            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                var newViewContext = new ViewContext(htmlHelper.ViewContext, htmlHelper.ViewContext.View, newViewData, htmlHelper.ViewContext.TempData, writer);

                IView view = viewEngineCollection.FindPartialView(newViewContext, partialViewName).View;

                view.Render(newViewContext, writer);
                return SimpleHtmlMinifier(writer.ToString());
            }
        }

        private static String SimpleHtmlMinifier(String html)
        {
            html = Regex.Replace(html, @"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}", "");
            html = Regex.Replace(html, @"[ \f\r\t\v]?([\n\xFE\xFF/{}[\];,<>*%&|^!~?:=])[\f\r\t\v]?", "$1");
            html = html.Replace(";\n", ";");
            return html;
        }
    }
}
