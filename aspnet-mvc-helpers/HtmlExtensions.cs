﻿using System;
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
        private const string MicrosoftCDNRoot = "https://ajax.aspnetcdn.com/ajax/";
        private const string GoogleCDNRoot = "https://ajax.googleapis.com/ajax/libs/";

        /// <summary>
        /// Tag for prevent caching
        /// </summary>
        public static ShortGuid CacheTag = new ShortGuid(Guid.NewGuid());
        /// <summary>
        /// Create a radio button MVC compatible and BootStrap compatible 
        /// </summary>
        /// <typeparam name="TModel">The type of Model</typeparam>
        /// <typeparam name="TValue">The type of Value</typeparam>
        /// <param name="html">Html Comntext</param>
        /// <param name="expression">an axpression</param>
        /// <param name="htmlAttributes">htmlAttributes to add in std control</param>
        /// <returns>The Html of control</returns>
        public static MvcHtmlString BootstrapRadioButtons<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes = null)
        {
            //http://weblogs.asp.net/psheriff/creating-radio-buttons-using-bootstrap-and-mvc
            var metaData = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var names = Enum.GetNames(metaData.ModelType);
            var sb = new StringBuilder();
            foreach (var name in names)
            {
                var id = string.Format(
                    "{0}_{1}_{2}",
                    html.ViewData.TemplateInfo.HtmlFieldPrefix,
                    metaData.PropertyName,
                    name
                    );

                var radio = html.RadioButtonFor(expression, name, htmlAttributes).ToHtmlString();
                var active = radio.Contains("data-val=\"true\"") ? " active" : string.Empty;
                sb.AppendFormat(
                    "<label id='{0}' class='btn btn-primary{3}'>{2}{1}</label>",
                    id,
                    name,
                    radio,
                    active
                    );
            }

            var result = string.Format("<div class='btn-group' data-toggle='buttons'>{0}</div>", sb);

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
                if (bool.TryParse(metadata.Model.ToString(), out modelChecked))
                {
                    isChecked = modelChecked;
                }
            }

            var result = string.Format(domElement, metadata.PropertyName, (!isChecked).ToString().ToLower()/*metadata.Model*/,
                isChecked ? " checked='checked'" : string.Empty,
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
        public static MvcHtmlString EmailFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
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
        public static MvcHtmlString TypeScript(this HtmlHelper helper, string url, bool debug = false)
        {
            // ReSharper disable once NotResolvedInText
            if (debug && string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(@"TypeScript Helper");

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
        public static MvcHtmlString TypeScript(this HtmlHelper helper, string bundleName,
            IEnumerable<string> urls, bool debug = false)
        {
            if (!debug)
            {
                bundleName = "~/bundles/" + bundleName;
                var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(bundleName);
                if (bundleUrl != null)
                    return new MvcHtmlString(string.Format(ScriptTag, bundleUrl));
            }

            var realUrls = new List<string>();
            foreach (var url in urls)
            {
                // ReSharper disable once NotResolvedInText
                if (debug && string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(@"TypeScript Helper");

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
        public static MvcHtmlString JavaScript(this HtmlHelper helper, string url, bool debug = false)
        {
            return JavaScript(helper, url, true, debug);
        }

        /// <summary>
        /// Load a JS, create a Bundle 
        /// and set the script tag to load it
        /// </summary>
        /// <param name="helper">The HTML context</param>
        /// <param name="url">Ulr of JS file</param>
        /// <param name="noBundle"></param>
        /// <param name="debug">Set if we we are in debug mode(true)</param>
        /// <returns>Return the script source</returns>
        public static MvcHtmlString JavaScript(this HtmlHelper helper, string url, bool noBundle = false, bool debug = false)
        {
            // if we are in debug mode, just return the script tag
            if (debug)
                return new MvcHtmlString(string.Format(ScriptTag,
                    System.Web.VirtualPathUtility.ToAbsolute(url + ".js") + "?v=" + DateTime.UtcNow));

            if (!noBundle)
            {
                // if BundleName is null, don't use the bundle and send just a script tag (with .min)
                return new MvcHtmlString(string.Format(ScriptTag,
                System.Web.VirtualPathUtility.ToAbsolute(url + ".min.js") + "?v=" + CacheTag));

            }
            // try to find the bundle in app bundles table
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(url);
            if (bundleUrl != null) return new MvcHtmlString(string.Format(ScriptTag, bundleUrl));

            // if not found, create it (for everybody)
            var jsBundle = new ScriptBundle(url).Include(url + ".js");
            BundleTable.Bundles.Add(jsBundle);
            bundleUrl = BundleTable.Bundles.ResolveBundleUrl(url) + "?v=" + CacheTag;
            // return the script tag with the right bundle url
            return new MvcHtmlString(string.Format(ScriptTag, bundleUrl));
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
        public static MvcHtmlString JavaScript(this HtmlHelper helper, string bundleName,
            IEnumerable<string> urls, bool debug = false)
        {
            // if we are in debug mode, just return the script tags
            if (debug)
            {
                // UTC now is use for prevent against cache joke
                var scr = urls.Aggregate("",
                    (current, url) =>
                        current +
                        string.Format(ScriptTag, System.Web.VirtualPathUtility.ToAbsolute(url + ".js") + "?v=" + DateTime.UtcNow));

                return new MvcHtmlString(scr);
            }
            if (string.IsNullOrEmpty(bundleName))
            {
                // if BundleName is null, don't use the bundle and send just a script tag (with .min)
                var scr = urls.Aggregate("",
                  (current, url) =>
                      current +
                      string.Format(ScriptTag, System.Web.VirtualPathUtility.ToAbsolute(url + ".min.js") + "?v=" + CacheTag));

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
            return new MvcHtmlString(string.Format(ScriptTag, bundleUrl));
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
        /// <param name="version">Default version of JQuery (2.1.4 by default)</param>
        /// <returns>Scripts url for JQuery</returns>
        public static MvcHtmlString JQuery(this HtmlHelper helper, bool debug = false, string bundleName = "~/bundles/jquery", string version = "2.2.0")
        {
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(bundleName);

            if (!debug)
            {
                // setup the script to load Jquery from CDN
                var jQueryVersion = GoogleCDNRoot  + string.Format("jquery/{0}/jquery-min.js", version);
                // setup the script to load Jquery if CDN is fail
                // Inspired by http://www.asp.net/mvc/overview/performance/bundling-and-minification
                // &&  http://www.hanselman.com/blog/CDNsFailButYourScriptsDontHaveToFallbackFromCDNToLocalJQuery.aspx
                var switchNoCdn = string.Format("window.jQuery || document.write('<script src=\"{0}\">\x3C/script>')", bundleUrl);

                return new MvcHtmlString(string.Format(ScriptTag, jQueryVersion) + string.Format(ScriptTag, switchNoCdn));
            }

            return new MvcHtmlString(string.Format(ScriptTag, bundleUrl));
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
        public static MvcHtmlString JQueryVal(this HtmlHelper helper, bool debug = false,
            string bundleName = "~/bundles/jqueryval", string version = "1.14.0", string mvcVersion = "5.2.3")
        {
            // todo : increase version when MS CDN is ready
            // todo : include localized message
            if (!debug)
            {
                var scriptValidate = string.Format("{0}jquery.validate/{1}/jquery.validate.min.js", MicrosoftCDNRoot, version);
                var scriptValidateAdditionalMethods = string.Format("{0}jquery.validate/{1}/additional-methods.min.js", MicrosoftCDNRoot, version);
                var scriptUnobtrusive = string.Format("{0}mvc/{1}/jquery.validate.unobtrusive.min.js", MicrosoftCDNRoot, mvcVersion);
                return new MvcHtmlString(
                        string.Format(ScriptTag, scriptValidate) +
                        string.Format(ScriptTag, scriptValidateAdditionalMethods) +
                        string.Format(ScriptTag, scriptUnobtrusive));

            }
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(bundleName);
            return new MvcHtmlString(string.Format(ScriptTag, bundleUrl));
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
            {
                // in release mode, we send the minify version of css
                name = name.Substring(0, name.IndexOf('.')) + ".min.css";
            }

            name += "?v=" + CacheTag;

            return new MvcHtmlString(string.Format("<link rel='stylesheet' href='{0}'>", System.Web.VirtualPathUtility.ToAbsolute(name)));
        }

        /// <summary>
        /// Helper to combine with ResourceBuilder Class
        /// Give the right bundle Url contructed by 
        /// Resources Builder
        /// </summary>
        /// <param name="helper">HTML context</param>
        /// <param name="language">Language of JS file</param>
        /// <returns>Url of JSON/JS Resources</returns>
        public static MvcHtmlString ResourcesJS(this HtmlHelper helper, string language = "en")
        {
            const string resxName = "~/bundles/resources";
            var culture = helper.BrowserCulture();
            string fullLang = string.Empty;
            string isoLang = "." + language;

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

            return new MvcHtmlString(string.Format(ScriptTag, bundleUrl));
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
        public static MvcHtmlString GravatarImage(this HtmlHelper helper, string mailAdd,
            int size = 80, string @default = "404", string altText = "Gravatar Image")
        {
            const string gravatarUrl = "<img src='http://www.gravatar.com/avatar/{0}?s={1}&d={2}'  alt='{3}'>";
            var hash = (string.IsNullOrWhiteSpace(mailAdd)) ? "unknow" : MD5Helpers.GetMd5Hash(mailAdd);

            return new MvcHtmlString(string.Format(gravatarUrl, hash, size, @default, altText));
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
        public static string CreateGravatarUrl(string mailAdd,
           int size = 80, string @default = "404", string altText = "Gravatar Image")
        {
            const string gravatarUrl = "<img src='http://www.gravatar.com/avatar/{0}?s={1}&d={2}'  alt='{3}'>";
            var hash = (string.IsNullOrWhiteSpace(mailAdd)) ? "unknow" : MD5Helpers.GetMd5Hash(mailAdd);

            return string.Format(gravatarUrl, hash, size, @default, altText);
        }

        #region GlyphIconLink

        /// <summary>
        /// example usage:
        /// <li>@Html.ActionLinkWithGlyphIcon(Url.Action("Index"),
        ///                                   "Back to List",
        ///                                   "glyphicon-list")</li>
        /// instead of "glyphicon-list", we could also use GlyphIcons.list
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="action">Mvc action url</param>
        /// <param name="text">Text for link</param>
        /// <param name="glyphs">Name of the glyphs icon</param>
        /// <param name="tooltip">Tooltip text</param>
        /// <param name="htmlAttributes">Other hmtl attributes (like css class)</param>
        /// <returns></returns>
        public static MvcHtmlString ActionLinkWithGlyphIcon(this HtmlHelper helper,
            string action,
            string text,
            string glyphs,
            string tooltip = "",
            IDictionary<string, object> htmlAttributes = null)
        {

            var glyph = new TagBuilder("span");
            glyph.MergeAttribute("class", string.Format("glyphicon {0}", glyphs));

            var anchor = new TagBuilder("a");
            anchor.MergeAttribute("href", action);

            if (!string.IsNullOrEmpty(tooltip))
                anchor.MergeAttributes(
                    new Dictionary<string, object>
                    {
                            { "rel", "tooltip" },
                            { "data-placement", "top" },
                            { "title", tooltip }
                        }
                    );

            if (htmlAttributes != null)
                anchor.MergeAttributes(htmlAttributes, true);

            anchor.InnerHtml = glyph + " " + text;

            return MvcHtmlString.Create(anchor.ToString());
        }

        #region icon constants
#pragma warning disable 1591
        public const string Glass = "glyphglyphicon-glass";

        public const string Music = "glyphicon-music";
        public const string Search = "glyphicon-search";
        public const string Envelope = "glyphicon-envelope";
        public const string Heart = "glyphicon-heart";
        public const string Star = "glyphicon-star";
        public const string StarEmpty = "glyphicon-star-empty";
        public const string User = "glyphicon-user";
        public const string Film = "glyphicon-film";
        public const string ThLarge = "glyphicon-th-large";
        public const string Th = "glyphicon-th";
        public const string ThList = "glyphicon-th-list";
        public const string Ok = "glyphicon-ok";
        public const string Remove = "glyphicon-remove";
        public const string ZoomIn = "glyphicon-zoom-in";
        public const string ZoomOut = "glyphicon-zoom-out";
        public const string Off = "glyphicon-off";
        public const string Signal = "glyphicon-signal";
        public const string Cog = "glyphicon-cog";
        public const string Trash = "glyphicon-trash";
        public const string Home = "glyphicon-home";
        public const string File = "glyphicon-file";
        public const string Time = "glyphicon-time";
        public const string Road = "glyphicon-road";
        public const string DownloadAlt = "glyphicon-download-alt";
        public const string Download = "glyphicon-download";
        public const string Upload = "glyphicon-upload";
        public const string Inbox = "glyphicon-inbox";
        public const string PlayCircle = "glyphicon-play-circle";
        public const string Repeat = "glyphicon-repeat";
        public const string Refresh = "glyphicon-refresh";
        public const string ListAlt = "glyphicon-list-alt";
        public const string Lock = "glyphicon-lock";
        public const string Flag = "glyphicon-flag";
        public const string Headphones = "glyphicon-headphones";
        public const string VolumeOff = "glyphicon-volume-off";
        public const string VolumeDown = "glyphicon-volume-down";
        public const string VolumeUp = "glyphicon-volume-up";
        public const string Qrcode = "glyphicon-qrcode";
        public const string Barcode = "glyphicon-barcode";
        public const string Tag = "glyphicon-tag";
        public const string Tags = "glyphicon-tags";
        public const string Book = "glyphicon-book";
        public const string Bookmark = "glyphicon-bookmark";
        public const string Print = "glyphicon-print";
        public const string Camera = "glyphicon-camera";
        public const string Font = "glyphicon-font";
        public const string Bold = "glyphicon-bold";
        public const string Italic = "glyphicon-italic";
        public const string TextHeight = "glyphicon-text-height";
        public const string TextWidth = "glyphicon-text-width";
        public const string AlignLeft = "glyphicon-align-left";
        public const string AlignCenter = "glyphicon-align-center";
        public const string AlignRight = "glyphicon-align-right";
        public const string AlignJustify = "glyphicon-align-justify";
        public const string List = "glyphicon-list";
        public const string IndentLeft = "glyphicon-indent-left";
        public const string IndentRight = "glyphicon-indent-right";
        public const string FacetimeVideo = "glyphicon-facetime-video";
        public const string Picture = "glyphicon-picture";
        public const string Pencil = "glyphicon-pencil";
        public const string MapMarker = "glyphicon-map-marker";
        public const string Adjust = "glyphicon-adjust";
        public const string Tint = "glyphicon-tint";
        public const string Edit = "glyphicon-edit";
        public const string Share = "glyphicon-share";
        public const string Check = "glyphicon-check";
        public const string Move = "glyphicon-move";
        public const string StepBackward = "glyphicon-step-backward";
        public const string FastBackward = "glyphicon-fast-backward";
        public const string Backward = "glyphicon-backward";
        public const string Play = "glyphicon-play";
        public const string Pause = "glyphicon-pause";
        public const string Stop = "glyphicon-stop";
        public const string Forward = "glyphicon-forward";
        public const string FastForward = "glyphicon-fast-forward";
        public const string StepForward = "glyphicon-step-forward";
        public const string Eject = "glyphicon-eject";
        public const string ChevronLeft = "glyphicon-chevron-left";
        public const string ChevronRight = "glyphicon-chevron-right";
        public const string PlusSign = "glyphicon-plus-sign";
        public const string MinusSign = "glyphicon-minus-sign";
        public const string RemoveSign = "glyphicon-remove-sign";
        public const string OkSign = "glyphicon-ok-sign";
        public const string QuestionSign = "glyphicon-question-sign";
        public const string InfoSign = "glyphicon-info-sign";
        public const string Screenshot = "glyphicon-screenshot";
        public const string RemoveCircle = "glyphicon-remove-circle";
        public const string OkCircle = "glyphicon-ok-circle";
        public const string BanCircle = "glyphicon-ban-circle";
        public const string ArrowLeft = "glyphicon-arrow-left";
        public const string ArrowRight = "glyphicon-arrow-right";
        public const string ArrowUp = "glyphicon-arrow-up";
        public const string ArrowDown = "glyphicon-arrow-down";
        public const string ShareAlt = "glyphicon-share-alt";
        public const string ResizeFull = "glyphicon-resize-full";
        public const string ResizeSmall = "glyphicon-resize-small";
        public const string Plus = "glyphicon-plus";
        public const string Minus = "glyphicon-minus";
        public const string Asterisk = "glyphicon-asterisk";
        public const string ExclamationSign = "glyphicon-exclamation-sign";
        public const string Gift = "glyphicon-gift";
        public const string Leaf = "glyphicon-leaf";
        public const string Fire = "glyphicon-fire";
        public const string EyeOpen = "glyphicon-eye-open";
        public const string EyeClose = "glyphicon-eye-close";
        public const string WarningSign = "glyphicon-warning-sign";
        public const string Plane = "glyphicon-plane";
        public const string Calendar = "glyphicon-calendar";
        public const string Random = "glyphicon-random";
        public const string Comment = "glyphicon-comment";
        public const string Magnet = "glyphicon-magnet";
        public const string ChevronUp = "glyphicon-chevron-up";
        public const string ChevronDown = "glyphicon-chevron-down";
        public const string Retweet = "glyphicon-retweet";
        public const string ShoppingCart = "glyphicon-shopping-cart";
        public const string FolderClose = "glyphicon-folder-close";
        public const string FolderOpen = "glyphicon-folder-open";
        public const string ResizeVertical = "glyphicon-resize-vertical";
        public const string ResizeHorizontal = "glyphicon-resize-horizontal";
        public const string Hdd = "glyphicon-hdd";
        public const string Bullhorn = "glyphicon-bullhorn";
        public const string Bell = "glyphicon-bell";
        public const string Certificate = "glyphicon-certificate";
        public const string ThumbsUp = "glyphicon-thumbs-up";
        public const string ThumbsDown = "glyphicon-thumbs-down";
        public const string HandRight = "glyphicon-hand-right";
        public const string HandLeft = "glyphicon-hand-left";
        public const string HandUp = "glyphicon-hand-up";
        public const string HandDown = "glyphicon-hand-down";
        public const string CircleArrowRight = "glyphicon-circle-arrow-right";
        public const string CircleArrowLeft = "glyphicon-circle-arrow-left";
        public const string CircleArrowUp = "glyphicon-circle-arrow-up";
        public const string CircleArrowDown = "glyphicon-circle-arrow-down";
        public const string Globe = "glyphicon-globe";
        public const string Wrench = "glyphicon-wrench";
        public const string Tasks = "glyphicon-tasks";
        public const string Filter = "glyphicon-filter";
        public const string Briefcase = "glyphicon-briefcase";
        public const string Fullscreen = "glyphicon-fullscreen";
#pragma warning restore 1591
        #endregion

        #endregion

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
        /// <param name="useUserId">Use the user id (from IPrincipal) in ga (if user is connected)</param>
        /// <returns>Html String to inject in page</returns>
        public static MvcHtmlString AnalyticsScript(this HtmlHelper helper, bool debug = false, bool useUserId = false)
        {

            var applicationInsightsKey = WebConfigurationManager.AppSettings["applicationInsights"];

            if (string.IsNullOrEmpty(applicationInsightsKey) && debug) return null;

            var script = new StringBuilder("<script type='text/javascript'>");

            if (!debug)
            {
                var googlekey = WebConfigurationManager.AppSettings["GoogleAnalytics"];
                if (!string.IsNullOrWhiteSpace(googlekey))
                {
                    var googleScript =
                        "(function (i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){(i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a, m)})(window,document,'script','//www.google-analytics.com/analytics.js','ga');ga('create','"
                            + googlekey + "','auto');ga('send','pageview');";

                    if (useUserId && helper.ViewContext.HttpContext.Request.IsAuthenticated)
                    {
                        // Set the userid (an hashcode of name) for a better tracing
                        // https://developers.google.com/analytics/devguides/collection/analyticsjs/field-reference#userId
                        googleScript += string.Format("ga('set','userId','{0}');", helper.ViewContext.HttpContext.User.Identity.Name.GetHashCode());
                    }
                    script.AppendLine(googleScript);
                }
            }
            if (!string.IsNullOrEmpty(applicationInsightsKey))
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

        private static string SimpleHtmlMinifier(string html)
        {
            html = Regex.Replace(html, @"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}", "");
            html = Regex.Replace(html, @"[ \f\r\t\v]?([\n\xFE\xFF/{}[\];,<>*%&|^!~?:=])[\f\r\t\v]?", "$1");
            html = html.Replace(";\n", ";");
            return html;
        }
        #endregion
    }
}
