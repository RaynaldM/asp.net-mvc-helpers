using System;
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
    }
}
