using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Web.Resources;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;

namespace Optimizely.CMS.Labs.LiquidTemplating.Values
{
    public class CmsHelperValue : MemberValueBase
    {
        object _model = null;

        public CmsHelperValue(object model) : base()
        {
            _model = model;
        }

        public static FluidValue FullRefreshPropertiesMetadata(FunctionArguments a, TemplateContext c)
        {
            if ((c.GetValue("CmsContext").ToObjectValue() as CmsContext).IsInEditMode)
            {
                using (var w = new StringWriter())
                {
                    var input = !a.At(0).IsNil() ? a.At(0).ToStringValue() : null;
                    var helper = Helpers.GetHelperAndSetWriter(c, w);
                    if (input == null)
                    {
                        if (helper.ViewData[ViewDataKeys.FullRefreshProperties] is IList<string> refreshProperties && refreshProperties.Count > 0)
                        {
                            w.Write(new HtmlString(string.Format("<input type=\"hidden\" {0}=\"{1}\" />", PageEditing.DataEPiFullRefreshPropertyNames, string.Join(",", refreshProperties))));
                        }
                    }
                    else
                    {
                        w.Write(new HtmlString(string.Format("<input type=\"hidden\" {0}=\"{1}\" />", PageEditing.DataEPiFullRefreshPropertyNames, input)));
                    }

                    return new StringValue(w.ToString(), false);
                }
            }

            return new ObjectValue(string.Empty);
        }

        public FluidValue CanonicalLink(FunctionArguments a, TemplateContext c)
        {
            using (var w = new StringWriter())
            {
                var htmlHelper = Helpers.GetHelperAndSetWriter(c, w);

                var model = GetModel(a) ?? _model;
                var language = GetStringParameter(a, "Language", 1);
                var action = GetStringParameter(a, "Action", 2);
                
                IHtmlContent link;
                if (model != null && model is PageData pd)
                {
                    link = htmlHelper.CanonicalLink(pd.ContentLink, language, action);
                }
                else
                {
                    link = htmlHelper.CanonicalLink();
                }

                link.WriteTo(w, HtmlEncoder.Default);

                return new StringValue(w.ToString(), false);
            }
        }

        public static FluidValue EPiServerQuickNavigator(FunctionArguments a, TemplateContext c)
        {
            using (var w = new StringWriter())
            {
                var htmlHelper = Helpers.GetHelperAndSetWriter(c, w);
                var nav = htmlHelper.RenderEPiServerQuickNavigatorAsync().GetAwaiter().GetResult();
                nav.WriteTo(w, HtmlEncoder.Default);

                return new StringValue(w.ToString(), false);
            }
        }

        public static FluidValue RequiredClientResources(FunctionArguments a, TemplateContext c)
        {
            using (var w = new StringWriter())
            {
                ClientResources.RenderAllRequiredResources(a.At(0).ToStringValue()).WriteTo(w, HtmlEncoder.Default);
                return new StringValue(w.ToString(), false);
            }
        }

        public FluidValue EditAttributes(FunctionArguments a, TemplateContext c)
        {
            if ((c.GetValue("CmsContext").ToObjectValue() as CmsContext).IsInEditMode)
            {
                using (var w = new StringWriter())
                {
                    var input = a.At(0).ToStringValue();
                    var propertyName = input;
                    if (input.StartsWith("Model"))
                    {
                        //TODO: Can we reflect over the properties to throw an exception if the requested member doesn't exist on the model (nice to have)?
                    }

                    var htmlHelper = Helpers.GetHelperAndSetWriter(c, w);
                    w.Write(htmlHelper.EditAttributes(propertyName));

                    return new StringValue(w.ToString(), false);
                }
            }
            return new StringValue(string.Empty);
        }

        public static FluidValue Debug(FunctionArguments a, TemplateContext c)
        {
            using (var w = new StringWriter())
            {
                var model = a.At(0).ToObjectValue();
                w.WriteLine("DEBUG: Available properties on " + model + "<br>");
                foreach (var property in model.GetType().GetProperties())
                {
                    w.WriteLine(property.Name);
                    w.WriteLine("<br>");
                }

                return new StringValue(w.ToString(), false);
            }
        }

        public FluidValue AlternateLinks(FunctionArguments a, TemplateContext c)
        {
            using (var w = new StringWriter())
            {
                var model = GetModel(a) ?? _model;

                if (model is PageData pd)
                {
                    var action = GetStringParameter(a, "Action", 1);

                    var htmlHelper = Helpers.GetHelperAndSetWriter(c, w);
                    var link = htmlHelper.AlternateLinks(pd.ContentLink, action);
                    link.WriteTo(w, HtmlEncoder.Default);

                    return new StringValue(w.ToString(), false);
                }
            }

            return new StringValue("");
        }

        public FluidValue Translate(FunctionArguments a, TemplateContext c)
        {
            var key = !a.At(0).IsNil() ? a.At(0).ToStringValue() : "";
            var translated = LocalizationService.Current.GetString(key);

            return new StringValue(translated);
        }

        public FluidValue TranslateWithFallback(FunctionArguments a, TemplateContext c)
        {
            var key = !a.At(0).IsNil() ? a.At(0).ToStringValue() : "";
            var fallback = !a.At(1).IsNil() ? a.At(1).ToStringValue() : "";
            var translated = LocalizationService.Current.GetString(key, fallback);

            return new StringValue(translated);
        }
        private static object GetModel(FunctionArguments a)
        {
            if (!a.At(0).IsNil())
            {
                return a.At(0).ToObjectValue();
            }
            return null;
        }

        private static string GetStringParameter(FunctionArguments a, string name, int position)
        {
            return !a[name].IsNil() ? a[name].ToStringValue() : !a.At(position).IsNil() ? a.At(position).ToStringValue() : null;
        }
    }
}
