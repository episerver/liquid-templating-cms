using EPiServer.Core;
using Fluid;
using Fluid.Values;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

namespace Optimizely.CMS.Labs.LiquidTemplating.Values
{
    public class ContentAreaValue : FluidValue
    {
        private ContentArea _value;
        public ContentAreaValue(ContentArea value)
        {
            _value = value;
        }
        public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo) => writer.Write(ToStringValue());

        public override FluidValues Type => FluidValues.Object;

        protected override FluidValue GetValue(string name, TemplateContext context)
        {
            switch (name)
            {
                case "size":
                    return NumberValue.Create(_value.Items.Count);
                case "items":
                    if (_value.Items.Any())
                    {
                        return Create(_value.Items.ToArray(), new TemplateOptions());
                    }
                    break;

                case "filtered_items":
                    if (_value.FilteredItems.Any())
                    {
                        return Create(_value.FilteredItems.ToArray(), new TemplateOptions());
                    }
                    break;

            }

            return NilValue.Instance;
        }

        public override bool Equals(FluidValue other) => false;
        public override bool ToBooleanValue() => false;
        public override decimal ToNumberValue() => 0;
        public override object ToObjectValue() => _value;
        public override string ToStringValue()
        {
            return _value.ToString();
        }
    }
}
