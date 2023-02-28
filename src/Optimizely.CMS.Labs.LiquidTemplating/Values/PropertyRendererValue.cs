using EPiServer.Web;
using Fluid;
using Fluid.Values;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyRenderer = Optimizely.CMS.Labs.LiquidTemplating.Rendering.PropertyRenderer;

namespace Optimizely.CMS.Labs.LiquidTemplating.Values
{
    public class PropertyRendererValue : ObjectValueBase
    {
        object _model;

        public PropertyRendererValue() : base(new object())
        {
        }

        public PropertyRendererValue(object model) : base(new object())
        {
            _model = model;
        }

        public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
        {
            //TODO: check / enforce that name == "For" (if required)
            return new FunctionValue(new Func<FunctionArguments, TemplateContext, FluidValue>(GetProperty));
        }

        public FluidValue GetProperty(FunctionArguments a, TemplateContext c)
        {
            //The model argument must be explicitly named or is the first argument
            var model = !a["Model"].IsNil() ? a["Model"].ToObjectValue() : a.At(0).ToObjectValue();
            
            //TODO fallback to c# property name by reflection?
            // The propertyname must be explicitly provided or is the second argument
            var propertyName = a["PropertyName"].IsNil() ? a.At(1).ToStringValue() : a["PropertyName"].ToStringValue();
            if (propertyName != null && propertyName.StartsWith("Model."))
            {
                propertyName = propertyName["Model.".Length..];
            }

            //Resolve fluid rendering parameters 
            var tag = !a["Tag"].IsNil() ? a["Tag"].ToStringValue() : null;
            var cssClass = !a["CssClass"].IsNil() ? a["CssClass"].ToStringValue() : null;
            var viewData = !a["ViewData"].IsNil() ? a["ViewData"].ToStringValue() : null;

            var additional = new { CssClass = cssClass, Tag = tag };
            var templateName = !a["TemplateName"].IsNil() ? a["TemplateName"].ToStringValue() : null;
            var editorSettings = !a["EditorSettings"].IsNil() ? JsonConvert.DeserializeObject(a["EditorSettings"].ToStringValue()) : null;
            var customTag = !a["CustomTag"].IsNil() ? a["CustomTag"].ToStringValue() : null;

            //TODO EditContainerClass - 
            //TODO ChildrenCustomTagName - RenderSettings.ChildrenCustomTag
            //TODO ChildrenCssClass - RenderSettings.ChildrenCssClass

            //Set RenderSettings to ViewData
            using (var w = new StringWriter())
            {
                var htmlHelper = Helpers.GetHelperAndSetWriter(c, w);

                //add items to viewcontext
                if (tag != null)
                {
                    htmlHelper.ViewContext.ViewData[RenderSettings.Tag] = tag;
                }
                else
                {
                    // Unset it, in case it's been set on the context previously
                    htmlHelper.ViewContext.ViewData[RenderSettings.Tag] = null;
                }

                // If there's a css class then add it to the view data
                if (cssClass != null)
                {
                    htmlHelper.ViewContext.ViewData[RenderSettings.CssClass] = cssClass;
                }
                else
                {
                    // Unset it, in case it's been set on the context previously
                    htmlHelper.ViewContext.ViewData[RenderSettings.CssClass] = null;
                }

                if (customTag != null)
                {
                    htmlHelper.ViewContext.ViewData[RenderSettings.CustomTagName] = customTag;
                }
                else
                {
                    // Unset it, in case it's been set on the context previously
                    htmlHelper.ViewContext.ViewData[RenderSettings.CustomTagName] = null;
                }

                // If there's additional viewData add it as a flag. Useful for display logic
                if (viewData != null)
                {
                    htmlHelper.ViewContext.ViewData[viewData] = true;
                }


                var cmsContext = c.GetValue("CmsContext").ToObjectValue() as CmsContext;

                //defer to internalpropertyrenderer
                var renderer = new PropertyRenderer();
                renderer.Render(w, htmlHelper, model, propertyName, cmsContext.IsInEditMode, customTag);

                return new StringValue(w.ToString(), false);
            }
        }
    }
}
