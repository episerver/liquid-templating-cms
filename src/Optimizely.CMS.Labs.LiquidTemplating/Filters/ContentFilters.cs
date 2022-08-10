using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Fluid;
using Fluid.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optimizely.CMS.Labs.LiquidTemplating.Filters
{
    public static class ContentFilters
    {
        public static FilterCollection WithContentFilters(this FilterCollection filters)
        {
            filters.AddFilter("access", FilterAccess);
            filters.AddFilter("for_visitor", FilterContentForVisitor);
            filters.AddFilter("published", FilterPublished);
            filters.AddFilter("has_template", FilterTemplate);
            filters.AddFilter("of_type", FilterType);
            //filters.AddFilter("with_property", FilterPropertyValue);
            filters.AddFilter("visible_in_menu", FilterVisibleInMenu);

            return filters;
        }

        public static ValueTask<FluidValue> FilterAccess(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var access = AccessLevel.Read;

            if (!arguments.At(0).IsNil())
            {
                var requiredAccess = arguments.At(0).ToStringValue();
                switch (requiredAccess.ToLower())
                {
                    case "read":
                        access = AccessLevel.Read;
                        break;
                    case "create":
                        access = AccessLevel.Create;
                        break;
                    case "delete":
                        access = AccessLevel.Delete;
                        break;
                    case "administer":
                        access = AccessLevel.Administer;
                        break;
                    case "edit":
                        access = AccessLevel.Edit;
                        break;
                    case "publish":
                        access = AccessLevel.Publish;
                        break;
                    case "noaccess":
                        access = AccessLevel.NoAccess;
                        break;
                    case "fullaccess":
                        access = AccessLevel.FullAccess;
                        break;
                    default:
                        access = AccessLevel.Read;
                        break;
                }
            }

            var strategy = new FilterStrategy(new FilterAccess(access));
            strategy.Execute(input);
            return input;
        }

        public static ValueTask<FluidValue> FilterContentForVisitor(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var strategy = new FilterStrategy(new FilterContentForVisitor());
            return strategy.Execute(input);
        }

        public static ValueTask<FluidValue> FilterPublished(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var strategy = new FilterStrategy(new FilterPublished(PagePublishedStatus.Published));
            return strategy.Execute(input);
        }

        public static ValueTask<FluidValue> FilterTemplate(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var strategy = new FilterStrategy(new FilterTemplate());
            return strategy.Execute(input);
        }

        public static ValueTask<FluidValue> FilterSort(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var sortOrderArg = arguments.At(0).Or(new StringValue(FilterSortOrder.Alphabetical.ToString())).ToStringValue();
            var enumSortOrder = (FilterSortOrder)Enum.Parse(typeof(FilterSortOrder), sortOrderArg, true);

            var strategy = new FilterStrategy(new FilterSort(enumSortOrder));
            return strategy.Execute(input);
        }

        public static ValueTask<FluidValue> FilterType(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.Array)
                return input;

            //first argument is the content type name
            var contentTypeName = arguments.At(0).ToStringValue();
            //Load the ID, as IContent only exposes this - Lookup is by DisplayName
            var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            var contentType = contentTypeRepository.List().FirstOrDefault(c => c.DisplayName == contentTypeName);
            if (contentType == null)
                return input;

            var inputObject = input.ToObjectValue() as object[];
            var filteredContents = inputObject.Select(c => c as IContent).Where(p => p.ContentTypeID == contentType.ID);
            return new ArrayValue(filteredContents.Select(c => new ObjectValue(c)));
        }

        public static ValueTask<FluidValue> FilterPropertyValue(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.Array)
                return input;

            // First argument is the property name to match
            var propertyname = arguments.At(0).ToStringValue();

            // Second argument is the value to match, or 'true' if none is defined
            var targetValue = arguments.At(1).Or(BooleanValue.True).ToObjectValue();

            var inputObject = input.ToObjectValue() as object[];
            var filteredContents1 = inputObject.Select(c => c as PageData);
            var filteredContents = inputObject.Select(c => c as PageData).Where(p => p.Property[propertyname] == targetValue);
            return new ArrayValue(filteredContents.Select(c => new ObjectValue(c)));
        }

        public static ValueTask<FluidValue> FilterVisibleInMenu(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.Array)
                return input;

            var inputObject = input.ToObjectValue() as object[];
            var filteredContents = inputObject.Select(c => c as PageData).Where(p => p.VisibleInMenu);
            return new ArrayValue(filteredContents.Select(c => new ObjectValue(c)));
        }

        internal class FilterStrategy
        {
            public FilterStrategy(IContentFilter filter)
            {
                Filter = filter;
            }
            public IContentFilter Filter { get; set; }

            public ArrayValue Execute(FluidValue input)
            {
                if (!IsFluidArrayValue(input))
                    return ArrayValue.Empty;

                var contents = CastInputAsListContent(input);

                Filter.Filter(contents);
                return new ArrayValue(contents.Select(c => new ObjectValue(c)));
            }

            protected IList<IContent> CastInputAsListContent(FluidValue input)
            {
                var inputObject = input.ToObjectValue() as object[];
                var contents = inputObject.Select(c => c as IContent);
                return contents.ToList();
            }
            protected bool IsFluidArrayValue(FluidValue input) => input.Type == FluidValues.Array;
        }
    }
}
