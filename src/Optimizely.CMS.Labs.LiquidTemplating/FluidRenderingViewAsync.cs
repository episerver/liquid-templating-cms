using Fluid;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.CMS.Labs.LiquidTemplating.Values;
using System.Threading.Tasks;

namespace Optimizely.CMS.Labs.LiquidTemplating
{
    internal  static class FluidRenderingViewAsync
    {
        /// <summary>
        /// Delegate method configured in Fluid to add additional CMS specific values to Fluid Context.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="viewContext"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static ValueTask AddItemsToFluidContext(string path, ViewContext viewContext, TemplateContext context)
        {
            var cmsContext = viewContext.HttpContext.RequestServices.GetService<CmsContext>();

            context.SetValue("CmsContext", cmsContext);
            context.SetValue("ViewContext", viewContext);
            context.SetValue("ContentLoader", new ContentLoaderValue());
            context.SetValue("Property", new PropertyRendererValue(viewContext.ViewData.Model));
            context.SetValue("CmsHelper", new CmsHelperValue(viewContext.ViewData.Model));

            return new ValueTask();
        }
    }
}
