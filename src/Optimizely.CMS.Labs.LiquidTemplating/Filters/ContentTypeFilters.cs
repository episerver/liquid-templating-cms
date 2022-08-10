using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Fluid;
using Fluid.Values;
using System.Threading.Tasks;

namespace Optimizely.CMS.Labs.LiquidTemplating.Filters
{
    public static class ContentTypeFilters
    {
        public static FilterCollection WithContentTypeFilters(this FilterCollection filters)
        {
            filters.AddFilter("is_type", IsType);
            return filters;
        }

        public static ValueTask<FluidValue> IsType(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.Object)
                return BooleanValue.Create(false);

            if (arguments.At(0).IsNil())
                return BooleanValue.Create(false);

            if (input.ToObjectValue() is IContent c)
            {
                var contentTypeId = c.ContentTypeID;
                var isInteger = arguments.At(0).IsInteger();
                if (isInteger)
                {
                    var compareTypeId = arguments.At(0).ToNumberValue();
                    return BooleanValue.Create(contentTypeId == compareTypeId);
                }
                else
                {
                    var compareTypeName = arguments.At(0).ToStringValue();
                    var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
                    var contentType = contentTypeRepository.Load(compareTypeName);
                    if (contentType == null)
                        return BooleanValue.Create(false);

                    return BooleanValue.Create(contentTypeId == contentType.ID);
                }
            }

            return BooleanValue.Create(false);
        }
    }
}
