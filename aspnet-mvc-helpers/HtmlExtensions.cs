using System;
using System.Collections.Generic;
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
    /// <summary>
    /// A collection of usefull Html Helpers
    /// </summary>
    public static class HtmlExtensions
    {
        private const String CDNRoot = "http://ajax.aspnetcdn.com/ajax/";
        private static ShortGuid _ctag = new ShortGuid(Guid.NewGuid());
        /// <summary>
        /// Create a radio button MVC compatible and BootStrap compatible 
        /// </summary>
        /// <typeparam name="TModel">The type of Model</typeparam>
        /// <typeparam name="TValue">The type of Value</typeparam>
        /// <param name="html">Html Comntext</param>
        /// <param name="expression">an axpression</param>
        /// <param name="htmlAttributes">htmlAttributes to add in std control</param>
        /// <returns>The Html of control</returns>
        public static MvcHtmlString BootstrapRadioButtons<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Object htmlAttributes = null)
        {
            //http://weblogs.asp.net/psheriff/creating-radio-buttons-using-bootstrap-and-mvc
            var metaData = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var names = Enum.GetNames(metaData.ModelType);
            var sb = new StringBuilder();
            foreach (var name in names)
            {
                var id = String.Format(
                    "{0}_{1}_{2}",
                    html.ViewData.TemplateInfo.HtmlFieldPrefix,
                    metaData.PropertyName,
                    name
                    );

                var radio = html.RadioButtonFor(expression, name, htmlAttributes).ToHtmlString();
                var active = radio.Contains("data-val=\"true\"") ? " active" : String.Empty;
                sb.AppendFormat(
                    "<label id='{0}' class='btn btn-primary{3}'>{2}{1}</label>",
                    id,
                    name,
                    radio,
                    active
                    );
            }
            const string domElement = @"<div class='btn-group' data-toggle='buttons'>{0}</div>";

            var result = String.Format(domElement, sb);

            return new MvcHtmlString(result);
        }

        /// <summary>
        /// Create a checkbox in BootStrap Standard
        /// http://getbootstrap.com/css/#forms-controls
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
        /// set the mail type on an input (HTML5)
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

        private const string ScriptTag = "<script src='{0}'></script>";

        /// <summary>
        /// Wrapper used to load the right JS 
        /// generated by a typescript file
        /// And add in bundle cache
        /// </summary>
        /// <param name="helper">The HTML context</param>
        /// <param name="url">Url of the TS file</param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <returns>The right JS file (.min if not debug)</returns>
        public static MvcHtmlString TypeScript(this HtmlHelper helper, String url, Boolean debug = false)
        {
            // ReSharper disable once NotResolvedInText
            if (debug && String.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(@"TypeScript Helper");

            var suffixPos = url.LastIndexOf('.');
            if (debug && (suffixPos < 0 || url.Substring(suffixPos) != ".ts"))
                throw new ArgumentException("TypeScript Helper : bad name or bad suffix");

            var realName = url.Substring(0, suffixPos);

            return helper.JavaScript(realName, debug);
        }

        /// <summary>
        /// Wrapper used to load the right JS  
        /// generated by a typescript files
        /// And add in bundle cache
        /// </summary>
        /// <param name="helper">The HTML context</param>
        /// <param name="bundleName">Name of the future bundle</param>
        /// <param name="urls">List of URL of the TS file</param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <returns>The right JS file (.min if not debug)</returns>
        public static MvcHtmlString TypeScript(this HtmlHelper helper, String bundleName,
            IEnumerable<String> urls, Boolean debug = false)
        {
            if (!debug)
            {
                bundleName = "~/bundles/" + bundleName;
                var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(bundleName);
                if (bundleUrl != null)
                    return new MvcHtmlString(String.Format(ScriptTag, bundleUrl));
            }

            var realUrls = new List<String>();
            foreach (var url in urls)
            {
                // ReSharper disable once NotResolvedInText
                if (debug && String.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(@"TypeScript Helper");

                var suffixPos = url.LastIndexOf('.');
                if (debug && (suffixPos < 0 || url.Substring(suffixPos) != ".ts"))
                    throw new ArgumentException("TypeScript Helper : bad name or bad suffix");

                realUrls.Add(url.Substring(0, suffixPos));
            }
            return helper.JavaScript(bundleName, realUrls, debug);
        }

        /// <summary>
        /// Load a JS, create a Bundle 
        /// and set the script tag to load it
        /// </summary>
        /// <param name="helper">The HTML context</param>
        /// <param name="url">Ulr of JS file</param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <returns>Return the script source</returns>
        public static MvcHtmlString JavaScript(this HtmlHelper helper, String url, Boolean debug = false)
        {
            // if we are in debug mode, just return the script tag
            if (debug)
                return new MvcHtmlString(String.Format(ScriptTag,
                    System.Web.VirtualPathUtility.ToAbsolute(url + ".js") + "?v=" + DateTime.UtcNow));

            // try to find the bundle in app bundles table
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(url);
            if (bundleUrl == null)
            {
                // if not found, create it (for everybody)
                var jsBundle = new ScriptBundle(url).Include(url + ".js");
                BundleTable.Bundles.Add(jsBundle);
                bundleUrl = BundleTable.Bundles.ResolveBundleUrl(url)+"?v="+_ctag;
            }
            // return the script tag with the right bundle url
            return new MvcHtmlString(String.Format(ScriptTag, bundleUrl));
        }

        /// <summary>
        /// Load some JS files, create a Bundle 
        /// and set the script tag to load it
        /// </summary>
        /// <param name="helper">The HTML context</param>
        /// <param name="bundleName">The name of the future bundle</param>
        /// <param name="urls">URL list of JS file</param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <returns></returns>
        public static MvcHtmlString JavaScript(this HtmlHelper helper, String bundleName,
            IEnumerable<String> urls, Boolean debug = false)
        {
            // if we are in debug mode, just return the script tags
            if (debug)
            {
                // UTC now is use for prevent against cache joke
                var scr = urls.Aggregate("",
                    (current, url) =>
                        current +
                        String.Format(ScriptTag, System.Web.VirtualPathUtility.ToAbsolute(url + ".js") + "?v=" + DateTime.UtcNow));

                return new MvcHtmlString(scr);
            }

            // try to find the bundle in app bundles table
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(bundleName);
            if (bundleUrl == null)
            {
                // if not found, create it (for everybody)
                var jsBundle = new ScriptBundle(bundleName);
                foreach (var url in urls)
                {
                    // include all js in bundle
                    jsBundle.Include(url + ".js");
                }
                BundleTable.Bundles.Add(jsBundle);
                bundleUrl = BundleTable.Bundles.ResolveBundleUrl(bundleName);
            }
            // return the script tag with the right bundle url
            return new MvcHtmlString(String.Format(ScriptTag, bundleUrl));
        }

        /// <summary>
        /// Add Jquery files in page
        /// with CDN if it's release mode 
        ///     If CDN fail, it switch on local bundle 
        /// local bundle if it's debug mode (usefull for debugging)
        /// </summary>
        /// <param name="helper">HTML Context</param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <param name="bundleName">Default name of JQuery bundle (default : ~/bundles/jquery) </param>
        /// <param name="version">Default version of JQuery (2.1.3 by default)</param>
        /// <returns>Scripts url for JQuery</returns>
        public static MvcHtmlString JQuery(this HtmlHelper helper, Boolean debug = false, String bundleName = "~/bundles/jquery", String version = "2.1.3")
        {
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(bundleName);

            if (!debug)
            {
                // setup the script to load Jquery from CDN
                var jQueryVersion = CDNRoot + "JQuery/" + String.Format("jquery-{0}.min.js", version);
                // setup the script to load Jquery if CDN is fail
                // Inspired by http://www.asp.net/mvc/overview/performance/bundling-and-minification
                // &&  http://www.hanselman.com/blog/CDNsFailButYourScriptsDontHaveToFallbackFromCDNToLocalJQuery.aspx
                var switchNoCdn = String.Format("window.jQuery || document.write('<script src=\"{0}\">\x3C/script>')", bundleUrl);

                return new MvcHtmlString(String.Format(ScriptTag, jQueryVersion) + String.Format(ScriptTag, switchNoCdn));
            }

            return new MvcHtmlString(String.Format(ScriptTag, bundleUrl));
        }

        /// <summary>
        /// Add Jquery validation files in page
        /// with CDN if it's  release mode and local bundle 
        /// if it's debug mode (usefull for debugging)
        /// </summary>
        /// <param name="helper">HTML Context</param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <param name="bundleName">Default name of JQuery Validate bundle (default : ~/bundles/jqueryval)</param>
        /// <param name="version">Default version of JQuery Validate (1.13.1 by default)</param>
        /// <param name="mvcVersion">Default version of JQuery Validate Unobtrusive (5.2.3 by default)</param>
        /// <returns>Scripts url for validations</returns>
        public static MvcHtmlString JQueryVal(this HtmlHelper helper, Boolean debug = false,
            String bundleName = "~/bundles/jqueryval", String version = "1.14.0", String mvcVersion = "5.2.3")
        {
            // todo : increase version when MS CDN is ready
            // todo : include localized message
            if (!debug)
            {
                var scriptValidate = String.Format("{0}jquery.validate/{1}/jquery.validate.min.js", CDNRoot, version);
                var scriptValidateAdditionalMethods = String.Format("{0}jquery.validate/{1}/additional-methods.min.js", CDNRoot, version);
                var scriptUnobtrusive = String.Format("{0}mvc/{1}/jquery.validate.unobtrusive.min.js", CDNRoot, mvcVersion);
                return new MvcHtmlString(
                        String.Format(ScriptTag, scriptValidate) +
                        String.Format(ScriptTag, scriptValidateAdditionalMethods) +
                        String.Format(ScriptTag, scriptUnobtrusive));

            }
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(bundleName);
            return new MvcHtmlString(String.Format(ScriptTag, bundleUrl));
        }

        /// <summary>
        /// In full less configuration (or with webessential)
        /// VS/Build generate file.css and file.min.css
        /// this method choose the right file depend of mode 'release' or 'debug'
        /// </summary>
        /// <param name="helper">HTML Context</param>
        /// <param name="name">The CSS files</param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <returns></returns>
        public static MvcHtmlString Css(this HtmlHelper helper, string name, bool debug = false)
        {
            if (!debug)
                // in release mode, we send the minify version of css
                name = name.Substring(0, name.IndexOf('.')) + ".min.css";

            name += "?v=" + _ctag;

            return new MvcHtmlString(String.Format("<link rel='stylesheet' href='{0}'>", System.Web.VirtualPathUtility.ToAbsolute(name)));
        }

        /// <summary>
        /// Helper to combine with ResourceBuilder Class
        /// Give the right bundle Url contructed by 
        /// Resources Builder
        /// </summary>
        /// <param name="helper">HTML context</param>
        /// <param name="language">Language of JS file</param>
        /// <returns>Url of JSON/JS Resources</returns>
        public static MvcHtmlString ResourcesJS(this HtmlHelper helper, String language = "en")
        {
            const string resxName = "~/bundles/resources";
            var culture = helper.BrowserCulture();
            String fullLang = String.Empty;
            String isoLang = "." + language;

            // by convention, English (en) is the main language
            // and it should not be modified
            if (culture.Name != language)
            {
                // else we add the standard name of language
                fullLang = "_" + culture.Name;
                isoLang = "_" + culture.TwoLetterISOLanguageName;
            }

            // and we try to find the good ressources file (depend of create method)
            // first the full language package (eg : fr-FR)
            // second the main language package (eg : fr)
            // and if it's not found, we use the main pack : en
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(resxName + fullLang) ??
                            BundleTable.Bundles.ResolveBundleUrl(resxName + isoLang) ??
                            BundleTable.Bundles.ResolveBundleUrl(resxName);

            return new MvcHtmlString(String.Format(ScriptTag, bundleUrl));
        }

        /// <summary>
        /// Get the culture from browser (client side)
        /// </summary>
        /// <param name="helper">Html Context</param>
        /// <returns>A culture</returns>
        public static CultureInfo BrowserCulture(this HtmlHelper helper)
        {
            // get from browser
            var userLanguages = helper.ViewContext.HttpContext.Request.UserLanguages;
            CultureInfo ci;
            if (userLanguages != null && userLanguages.Any())
            {
                // browser have one or more language
                try
                {
                    // take the first
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

        ///  <summary>
        ///  Compose a valid Gravatar Url from email
        ///  
        ///   see : https://fr.gravatar.com/site/implement/images/
        ///   for more details
        ///  </summary>
        ///  <param name="helper">Html Helper context</param>
        ///  <param name="mailAdd">Address to use</param>
        ///  <param name="size">By default, images are presented at 80px by 80px</param>
        ///  <param name="default">
        ///     404: do not load any image if none is associated with the email hash, instead return an HTTP 404 (File Not Found) response
        ///     mm: (mystery-man) a simple, cartoon-style silhouetted outline of a person (does not vary by email hash)
        ///     identicon: a geometric pattern based on an email hash
        ///     monsterid: a generated 'monster' with different colors, faces, etc
        ///     wavatar: generated faces with differing features and backgrounds
        ///      retro: awesome generated, 8-bit arcade-style pixelated faces
        ///     blank: a transparent PNG image (border added to HTML below for demonstration purposes)
        /// </param>
        /// <param name="altText">Alternate text for image (default : Gravatar Image)</param>
        /// <returns>url of Gravatar image</returns>
        public static MvcHtmlString GravatarImage(this HtmlHelper helper, String mailAdd,
            int size = 80, String @default = "404", String altText = "Gravatar Image")
        {
            const string gravatarUrl = "<img src='http://www.gravatar.com/avatar/{0}?s={1}&d={2}'  alt='{3}'>";
            var hash = (String.IsNullOrWhiteSpace(mailAdd)) ? "unknow" : MD5Helpers.GetMd5Hash(mailAdd);

            return new MvcHtmlString(String.Format(gravatarUrl, hash, size, @default, altText));
        }

        ///  <summary>
        ///  Compose a valid Gravatar Url from email
        ///  
        ///   see : https://gravatar.com/site/implement/images/
        ///   for more details
        ///  </summary>
        ///  <param name="mailAdd">Address to use</param>
        ///  <param name="size">By default, images are presented at 80px by 80px</param>
        ///  <param name="default">
        ///     404: do not load any image if none is associated with the email hash, instead return an HTTP 404 (File Not Found) response
        ///     mm: (mystery-man) a simple, cartoon-style silhouetted outline of a person (does not vary by email hash)
        ///     identicon: a geometric pattern based on an email hash
        ///     monsterid: a generated 'monster' with different colors, faces, etc
        ///     wavatar: generated faces with differing features and backgrounds
        ///      retro: awesome generated, 8-bit arcade-style pixelated faces
        ///     blank: a transparent PNG image (border added to HTML below for demonstration purposes)
        /// </param>
        /// <param name="altText">Alternate text for image (default : Gravatar Image)</param>
        /// <returns>url of Gravatar image</returns>
        public static String CreateGravatarUrl(String mailAdd,
           int size = 80, String @default = "404", String altText = "Gravatar Image")
        {
            const string gravatarUrl = "<img src='http://www.gravatar.com/avatar/{0}?s={1}&d={2}'  alt='{3}'>";
            var hash = (String.IsNullOrWhiteSpace(mailAdd)) ? "unknow" : MD5Helpers.GetMd5Hash(mailAdd);

            return String.Format(gravatarUrl, hash, size, @default, altText);
        }

        /// <summary>
        /// Add a bundle with
        ///     Google Analytics
        ///     MS Application Insight
        /// You should have a Analytics sub-directory in Scripts
        /// this sub-dir should contain google-analytics.js and
        /// ApplicationInsight.js
        /// </summary>
        /// <param name="helper">Html Context</param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <returns>Html String to inject in page</returns>
        public static MvcHtmlString AnalyticsScript(this HtmlHelper helper, Boolean debug = false)
        {

            var applicationInsightsKey = WebConfigurationManager.AppSettings["applicationInsights"];

            if (String.IsNullOrEmpty(applicationInsightsKey) && debug) return null;

            var script = new StringBuilder("<script type='text/javascript'>");

            if (!debug)
            {
                var googlekey = WebConfigurationManager.AppSettings["GoogleAnalytics"];
                if (!String.IsNullOrWhiteSpace(googlekey))
                {
                    var googleScript =
                        "(function (i, s, o, g, r, a, m) {i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {(i[r].q = i[r].q || []).push(arguments)}, i[r].l = 1 * new Date(); a = s.createElement(o), m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)})(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');ga('create', '"
                            + googlekey + "', 'auto');ga('send', 'pageview');";

                    script.AppendLine(googleScript);
                }
            }
            if (!String.IsNullOrEmpty(applicationInsightsKey))
            {
                script.AppendFormat("window.applicationInsightsKey='{0}';", applicationInsightsKey);
            }

            script.Append("</script>");
            script.Append(Scripts.Render("~/bundles/analytics"));
            return new MvcHtmlString(script.ToString());
        }

        #region Compressed Partial
        /// <summary>
        /// Compress a partial view
        /// </summary>
        /// <param name="htmlHelper">HTML Context</param>
        /// <param name="partialViewName">The name (and path) of the partial view</param>
        /// <returns>The HTML code of partial compressed</returns>
        public static MvcHtmlString CompressedPartial(this HtmlHelper htmlHelper, string partialViewName)
        {
            return CompressedPartial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData);
        }

        /// <summary>
        /// Compress a partial view
        /// </summary>
        /// <param name="htmlHelper">HTML Context</param>
        /// <param name="partialViewName">The name (and path) of the partial view</param>
        /// <param name="viewData">Additionnal data from server</param>
        /// <returns>The HTML code of partial compressed</returns>
        public static MvcHtmlString CompressedPartial(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData)
        {
            return CompressedPartial(htmlHelper, partialViewName, null /* model */, viewData);
        }

        /// <summary>
        /// Compress a partial view
        /// </summary>
        /// <param name="htmlHelper">HTML Context</param>
        /// <param name="partialViewName">The name (and path) of the partial view</param>
        /// <param name="model">Model come from server</param>
        /// <returns>The HTML code of partial compressed</returns>
        public static MvcHtmlString CompressedPartial(this HtmlHelper htmlHelper, string partialViewName, object model)
        {
            return CompressedPartial(htmlHelper, partialViewName, model, htmlHelper.ViewData);
        }

        /// <summary>
        /// Compress a partial view
        /// </summary>
        /// <param name="htmlHelper">HTML Context</param>
        /// <param name="partialViewName">The name (and path) of the partial view</param>
        /// <param name="model">Model come from server</param>
        /// <param name="viewData">Additionnal data from server</param>
        /// <returns>The HTML code of partial compressed</returns>
        public static MvcHtmlString CompressedPartial(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData)
        {
            var result = htmlHelper.RenderPartialInternal(partialViewName, viewData, model, ViewEngines.Engines);
            return MvcHtmlString.Create(result);
        }

        private static string RenderPartialInternal(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData, object model, ViewEngineCollection viewEngineCollection)
        {
#if DEBUG
            if (String.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException(@"Empty view", "partialViewName");
            }
#endif

            ViewDataDictionary newViewData = model == null
                ? (viewData == null ? new ViewDataDictionary(htmlHelper.ViewData) : new ViewDataDictionary(viewData))
                : (viewData == null ? new ViewDataDictionary(model) : new ViewDataDictionary(viewData) { Model = model });

            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                var newViewContext = new ViewContext(htmlHelper.ViewContext, htmlHelper.ViewContext.View, newViewData, htmlHelper.ViewContext.TempData, writer);

                var view = viewEngineCollection.FindPartialView(newViewContext, partialViewName).View;

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
        #endregion
    }
}
