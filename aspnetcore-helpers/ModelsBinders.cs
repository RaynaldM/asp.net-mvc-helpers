using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace aspnetcore.helpers
{
    /// <summary>
    /// Using interfaces instead of concrete ViewModels allows a better separation between business layer and user interface layer
    /// <code>
    /// Add this code to your startup.cs
    ///      services.AddMvc(o =>
    ///          {
    ///              o.ModelBinderProviders.Insert(0,new InterfacesModelBinderProvider());
    ///          });
    /// </code>
    /// <see cref="http://www.dotnet-programming.com/post/2017/03/17/Custom-Model-Binding-in-Aspnet-Core-2-Model-Binding-Interfaces.aspx"/>
    /// </summary>
    public class InterfacesModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.IsCollectionType ||
                (!context.Metadata.ModelType.GetTypeInfo().IsInterface &&
                 !context.Metadata.ModelType.GetTypeInfo().IsAbstract) ||
                (context.BindingInfo.BindingSource != null && context.BindingInfo.BindingSource
                     .CanAcceptDataFrom(BindingSource.Services))) return null;


            var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
            var len = context.Metadata.Properties.Count;
            for (var i = 0; i < len; i++)
            {
                var property = context.Metadata.Properties[i];
                propertyBinders.Add(property, context.CreateBinder(property));
            }
            return new InterfacesModelBinder(propertyBinders);
        }
    }

    public class InterfacesModelBinder : ComplexTypeModelBinder
    {

        public InterfacesModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinder)
            : base(propertyBinder) { }

        protected override object CreateModel(ModelBindingContext bindingContext)
        {
            return bindingContext.HttpContext
                .RequestServices.GetService(bindingContext.ModelType);
        }
    }
}
