using Optimizely.CMS.Labs.LiquidTemplating.Filters;
using Fluid;
using Fluid.Values;
using NUnit.Framework;

namespace Optimizely.CMS.Labs.LiquidTemplating.Tests
{
    public class UrlResolverFiltersTests
    {
        private FilterArguments _arguments;
        private TemplateContext _context;

        [SetUp]
        public void Setup()
        {
            _arguments = new FilterArguments();
            _context = new TemplateContext();
        }

        [Test]
        public void Url_Returns_Null_For_NonObjectValueType()
        {
            var input = new StringValue("Test");
            var result = UrlResolverFilters.Url(input, _arguments, _context);

            Assert.AreEqual(NilValue.Instance, result.Result);
        }

        //[Test]
        //public void Url_Returns_Null_For_Non_IContent_Or_ContentReference_Or_String()
        //{
        //    var input = new ObjectValue(new StringValue("Test"));
        //    var result = UrlResolverFilters.Url(input, _arguments, _context);

        //    Assert.AreEqual(NilValue.Instance, result.Result);
        //}

        //public void Url_Returns_Url_For_IContent()
        //{

        //}

        //public void Url_Returns_Url_For_ContentReference()
        //{

        //}

        //public void Url_Returns_Url_For_InternalLink()
        //{

        //}
    }
}