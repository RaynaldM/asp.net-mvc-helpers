using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace aspnet_mvc_helpers
{
    /// <summary>
    /// A collection of usefull Html Helpers for bootstrap
    /// </summary>
    public static class BootstrapHtmlExtensions
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
        public static MvcHtmlString BootstrapRadioButtons<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
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

                var radio = html.RadioButtonFor(expression, name, new {id }).ToHtmlString();
                var active = radio.Contains("data-val=\"true\"") ? " active" : string.Empty;
                sb.AppendFormat(
                    "<label class='btn btn-primary{2}'>{1}{0}</label>",
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
        /// <param name="withLabel">Include the label or not</param>
        /// <returns>Checkbox</returns>
        public static MvcHtmlString BootStrapCheckBoxFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, bool withLabel = true)
        {
            const string domElement = @"<div class='checkbox'><label><input name='{0}' id='{0}' type='checkbox' value='{1}'{2}><input type='hidden' value='{4}' name='{0}'>{3}</label></div>";

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
                withLabel ? metadata.DisplayName ?? metadata.PropertyName : "",
                isChecked.ToString().ToLower());

            return new MvcHtmlString(result);
        }

        /// <summary>
        /// Decorate the classic validation summary with alert
        ///  inspired by http://stackoverflow.com/questions/13867307/show-validationsummary-mvc3-as-alert-error-bootstrap
        /// </summary>
        /// <param name="htmlHelper">Html extention *</param>
        /// <param name="alertColor">Color of the alert box</param>
        /// <returns></returns>
        public static HtmlString BootstrapValidationSummary(this HtmlHelper htmlHelper, string alertColor = "alert-warning")
        {
            return htmlHelper.ViewData.ModelState.IsValid ?
                new HtmlString(string.Empty) :
                new HtmlString(String.Format("<div class='alert {0}'>{1}</div>",
                alertColor, htmlHelper.ValidationSummary()));
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

    }
}
