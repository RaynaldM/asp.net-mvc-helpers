﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Optimization;

namespace aspnet_mvc_helpers
{
    public static class HtmlExtensions
    {
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
                var id = string.Format(
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

        /// <summary>
        /// Wrapper used to load the right JS 
        /// generated by a typescript file
        /// </summary>
        /// <param name="helper">The HTML context</param>
        /// <param name="url">Url of the TS file</param>
        /// <returns>The right JS file (.min if not debug)</returns>
        public static MvcHtmlString TypeScript(this HtmlHelper helper, String url)
        {
#if DEBUG
            // If debug, send an exception
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(@"TypeScript Helper");
#endif

            var suffixPos = url.LastIndexOf('.');
#if DEBUG
            if (suffixPos < 0 || url.Substring(suffixPos) != ".ts")
                throw new ArgumentException("TypeScript Helper : bad name or bad suffix");
#endif

            // compile the real name of compiled TS file : a JS file
            var realName = url.Substring(0, suffixPos);

#if DEBUG
            // In debug, it's just a simple script link, usefull for debugging
            return new MvcHtmlString(String.Format("<script src='{0}'></script>",
                System.Web.VirtualPathUtility.ToAbsolute(realName + ".js")));
#else
            // in release mode, we add this js file in bundle to cache it
            var bundleUrl = BundleTable.Bundles.ResolveBundleUrl(realName);
            if (bundleUrl == null)
            {
                // don't find in bundle list
                var jsBundle = new ScriptBundle(realName);
                jsBundle.Include(realName + ".js");
                // so Add it
                BundleTable.Bundles.Add(jsBundle);
                bundleUrl = BundleTable.Bundles.ResolveBundleUrl(realName);
            }
            // return the URL of bundle
            return new MvcHtmlString(String.Format("<script src='{0}'></script>", bundleUrl));
#endif
        }

        /// <summary>
        /// Add Jquery validation files in page
        /// with CDN if it's  release mode and local bundle 
        /// if it's debug mode (usefull for debugging)
        /// </summary>
        /// <param name="helper">HTML Context</param>
        /// <returns>Scripts url for validations</returns>
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

        /// <summary>
        /// In full less configuration (or with webessential)
        /// VS/Build generate file.css and file.min.css
        /// this method choose the right file depend of mode 'release' or 'debug'
        /// </summary>
        /// <param name="helper">HTML Context</param>
        /// <param name="name">The CSS files</param>
        /// <returns></returns>
        public static MvcHtmlString Css(this HtmlHelper helper, String name)
        {
#if !DEBUG
            // in release mode, we send the minify version of css
           name = name.Substring(0, name.IndexOf('.') ) + ".min.css";
#endif
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

            return new MvcHtmlString(String.Format("<script src='{0}'></script>", bundleUrl));
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
			int size = 80, String @default = "404",String altText= "Gravatar Image")
		{
			const string gravatarUrl = "<img src='http://www.gravatar.com/avatar/{0}?s={1}&d={2}'  alt='{3}'>";
			var hash = (String.IsNullOrWhiteSpace(mailAdd)) ? "unknow" : MD5Helpers.GetMd5Hash(mailAdd);

			return new MvcHtmlString(String.Format(gravatarUrl, hash, size, @default,altText));
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
