using EPiServer.Core;
using EPiServer.Web.Routing;
using Fluid;
using Fluid.Values;
using System.Threading.Tasks;

namespace Optimizely.CMS.Labs.LiquidTemplating.Filters
{
    public static class UrlResolverFilters
    {
        public static FilterCollection WithUrlFilters(this FilterCollection filters)
        {
            filters.AddFilter("url", Url);
            return filters;
        }

        public static ValueTask<FluidValue> Url(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.Object)
            {
                return NilValue.Empty;
            }

            var inputObject = input.ToObjectValue();
            var urlResolver = UrlResolver.Current;

            if (inputObject is EPiServer.Url)
                return new StringValue(urlResolver.GetUrl((inputObject as EPiServer.Url).ToString()));

            if (inputObject is IContent)
                return new StringValue(urlResolver.GetUrl(inputObject as IContent));

            if (inputObject is ContentReference)
                return new StringValue(urlResolver.GetUrl(inputObject as ContentReference));

            if (inputObject is string)
                return new StringValue(urlResolver.GetUrl(inputObject.ToString()));

            return NilValue.Empty;
        }
    }
}
