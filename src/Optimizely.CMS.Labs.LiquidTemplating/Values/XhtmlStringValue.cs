using EPiServer.Core;
using Fluid.Values;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;

namespace Optimizely.CMS.Labs.LiquidTemplating.Values
{
    public class XhtmlStringValue : FluidValue
    {
        private XhtmlString _value;
        public XhtmlStringValue(XhtmlString value)
        {
            _value = value;
        }
        public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo) => writer.Write(ToStringValue());

        public override FluidValues Type => FluidValues.Object;
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
