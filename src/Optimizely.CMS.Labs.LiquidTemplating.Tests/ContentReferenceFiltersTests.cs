using EPiServer.Core;
using Fluid;
using Fluid.Values;
using NUnit.Framework;
using Optimizely.CMS.Labs.LiquidTemplating.Filters;

namespace Optimizely.CMS.Labs.LiquidTemplating.Tests
{
    public class ContentReferenceFiltersTests
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
        public void IsNullOrEmpty_Returns_True_For_EmptyReference()
        {
            var input = new ObjectValue(ContentReference.EmptyReference);
            var result = ContentReferenceFilters.IsNullOrEmpty(input, _arguments, _context);

            Assert.IsTrue(result.Result.ToBooleanValue());
        }

        [Test]
        public void IsNullOrEmpty_Returns_True_For_NullReference()
        {
            var input = new ObjectValue(null);
            var result = ContentReferenceFilters.IsNullOrEmpty(input, _arguments, _context);

            Assert.IsTrue(result.Result.ToBooleanValue());
        }

        [Test]
        public void IsNullOrEmpty_Returns_False_For_PopulatedReference()
        {
            var input = new ObjectValue(new ContentReference() { ID = 1 });
            var result = ContentReferenceFilters.IsNullOrEmpty(input, _arguments, _context);

            Assert.IsFalse(result.Result.ToBooleanValue());
        }

        [Test]
        public void IsNullOrEmpty_Returns_True_For_NonObjectValueType()
        {
            var input = new StringValue("Test");
            var result = ContentReferenceFilters.IsNullOrEmpty(input, _arguments, _context);

            Assert.IsTrue(result.Result.ToBooleanValue());
        }

        [Test]
        public void IsNullOrEmpty_Returns_True_For_NonContentReferenceType()
        {
            var input = new ObjectValue(new StringValue("Test"));
            var result = ContentReferenceFilters.IsNullOrEmpty(input, _arguments, _context);

            Assert.IsTrue(result.Result.ToBooleanValue());
        }
    }
}