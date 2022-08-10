using System.Linq;
using EPiServer.Web.Mvc.Html;
using Fluid;
using Fluid.Ast;
using Fluid.ViewEngine;
using static Parlot.Fluent.Parsers;
using EPiServer.Framework.Web.Resources;
using System.Text.Encodings.Web;
using EPiServer.Editor;
using Microsoft.AspNetCore.Html;
using EPiServer.Framework.Localization;
using EPiServer.Web.Mvc;
using System.Collections.Generic;

namespace Optimizely.CMS.Labs.LiquidTemplating.ViewEngine
{
    public class CmsFluidViewParser : FluidViewParser
    {
        public CmsFluidViewParser(FluidParserOptions options) : base(options)
        {
            RegisterEmptyTag("CanonicalLink", (w, e, c) =>
            {
                var htmlHelper = Helpers.GetHelperAndSetWriter(c, w);
                var link = htmlHelper.CanonicalLink();
                link.WriteTo(w, HtmlEncoder.Default);

                return Statement.Normal();
            });

            RegisterEmptyTag("EPiServerQuickNavigator", (w, e, c) =>
            {
                var htmlHelper = Helpers.GetHelperAndSetWriter(c, w);
                var nav = htmlHelper.RenderEPiServerQuickNavigatorAsync().GetAwaiter().GetResult();
                nav.WriteTo(w, HtmlEncoder.Default);

                return Statement.Normal();
            });

            RegisterIdentifierTag("RequiredClientResources", (i, w, e, c) =>
            {
                w.Write(ClientResources.RenderRequiredResources(i.ToString()));
                return Statement.Normal();
            });

            RegisterExpressionTag("EditAttributes", (exp, w, e, context) =>
            {
                var htmlHelper = Helpers.GetHelperAndSetWriter(context, w);
                var isInEditMode = (context.GetValue("CmsContext").ToObjectValue() as CmsContext).IsInEditMode;

                if (isInEditMode)
                {
                    // Get the property name from the last segment of the passed in expression
                    var propertyName = ((IdentifierSegment)((MemberExpression)exp).Segments.Last()).Identifier;
                    var c = htmlHelper.EditAttributes(propertyName);
                    w.Write(c);
                }

                return Statement.Normal();
            });

            //TODO - need to add parameters
            RegisterExpressionTag("FullRefreshPropertiesMetaData", (exp, w, e, context) =>
            {
                if ((context.GetValue("CmsContext").ToObjectValue() as CmsContext).IsInEditMode)
                {
                    var input = exp.EvaluateAsync(context).Result.ToStringValue();
                    if (input == null)
                    {
                        var helper = Helpers.GetHelperAndSetWriter(context, w);
                        if (helper.ViewData[ViewDataKeys.FullRefreshProperties] is IList<string> editHints && editHints.Count > 0)
                        {
                            w.Write(new HtmlString(string.Format("<input type=\"hidden\" {0}=\"{1}\" />", PageEditing.DataEPiFullRefreshPropertyNames, string.Join(",", editHints))));
                        }
                    }
                    else
                    {
                        w.Write(new HtmlString(string.Format("<input type=\"hidden\" {0}=\"{1}\" />", PageEditing.DataEPiFullRefreshPropertyNames, input)));
                    }
                }

                return Statement.Normal();
            });

            RegisterExpressionTag("PropertyFor", (exp, w, e, context) =>
            {
                var input = exp.EvaluateAsync(context).Result.ToObjectValue();
                var htmlHelper = Helpers.GetHelperAndSetWriter(context, w);
                var propertyName = ((IdentifierSegment)((MemberExpression)exp).Segments.Last()).Identifier;
                var isInEditMode = (context.GetValue("CmsContext").ToObjectValue() as CmsContext).IsInEditMode;

                var propertyRenderer = new Rendering.PropertyRenderer();
                propertyRenderer.Render(w, htmlHelper, input, propertyName, isInEditMode, null);

                return Statement.Normal();
            });

            var translateParser = Terms.String();

            RegisterParserTag("Translate", translateParser, static (t, w, e, c) =>
            {
                var key = t.ToString();
                w.Write(LocalizationService.Current.GetString(key));
                return Statement.Normal();
            });

            var translateFallbackParser = Terms.String().And(Terms.String());

            RegisterParserTag("TranslateWithFallback", translateFallbackParser, static (t, w, e, c) =>
            {
                var key = t.Item1.ToString();
                var fallback = t.Item2.ToString();
                w.Write(LocalizationService.Current.GetString(key, fallback));
                return Statement.Normal();
            });

            RegisterExpressionTag("Debug", (exp, w, e, context) =>
            {
                var model = exp.EvaluateAsync(context).Result.ToObjectValue();
                foreach (var property in model.GetType().GetProperties())
                {
                    w.WriteLine(property.Name);
                }

                return Statement.Normal();
            });
        }
    }
}
