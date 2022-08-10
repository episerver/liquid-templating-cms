using Fluid;
using Fluid.Values;
using System.Threading.Tasks;
using System.Linq;

namespace Optimizely.CMS.Labs.LiquidTemplating.Filters
{
    public static class ArrayFilters
    {
        public static FilterCollection WithArrayFilters(this FilterCollection filters)
        {
            filters.AddFilter("take", Take);
            filters.AddFilter("skip", Skip);

            return filters;
        }

        public static ValueTask<FluidValue> Take(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.Array)
            {
                return input;
            }

            var number = (int)arguments.At(0).ToNumberValue();
            var result = input.Enumerate(context).Take(number);

            return new ArrayValue(result);
        }

        public static ValueTask<FluidValue> Skip(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.Array)
            {
                return input;
            }

            var number = (int)arguments.At(0).ToNumberValue();
            var result = input.Enumerate(context).Skip(number);

            return new ArrayValue(result);
        }
    }
}
