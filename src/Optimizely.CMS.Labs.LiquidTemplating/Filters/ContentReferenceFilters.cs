using EPiServer.Core;
using Fluid;
using Fluid.Values;
using System.Threading.Tasks;

namespace Optimizely.CMS.Labs.LiquidTemplating.Filters
{
    public static class ContentReferenceFilters
    {
        public static FilterCollection WithContentReferenceFilters(this FilterCollection filters)
        {
            filters.AddFilter("is_empty", IsNullOrEmpty);
            return filters;
        }

        public static ValueTask<FluidValue> IsNullOrEmpty(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.Object)
                return BooleanValue.Create(true);

            if (input.ToObjectValue() is ContentReference c)
            {
                return BooleanValue.Create(ContentReference.IsNullOrEmpty(c));
            }

            return BooleanValue.Create(true);
        }
    }
}
