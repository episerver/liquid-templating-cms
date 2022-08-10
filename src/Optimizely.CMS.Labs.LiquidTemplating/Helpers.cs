using Fluid;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Optimizely.CMS.Labs.LiquidTemplating
{
    internal class Helpers
    {
        internal static IHtmlHelper GetHelperAndSetWriter(TemplateContext context, TextWriter writer)
        {
            if (context.GetValue("ViewContext").ToObjectValue() is not ViewContext viewContext)
                throw new Exception("ViewContext Shouldn't be null. It's set in the Fluid context");

            var htmlHelper = viewContext.HttpContext.RequestServices.GetService<IHtmlHelper>();
            (htmlHelper as IViewContextAware).Contextualize(viewContext);
            htmlHelper.ViewContext.Writer = writer;

            return htmlHelper;
        }
    }
}
