using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Internal;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Templating;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Optimizely.CMS.Labs.LiquidTemplating.Rendering
{
    /// <summary>
    /// Customised version of the Episerver <see cref="ContentAreaRenderer"/> that corrects the use of TagBuilder  (.WriteTo() instead of .Render() )
    /// </summary>
    public class LiquidContentAreaRenderer : ContentAreaRenderer
    {
        private readonly IContentRenderer _contentRenderer;
        private readonly IContentRepository _contentRepository;
        private readonly IContentAreaLoader _contentAreaLoader;
        private readonly IContextModeResolver _contextModeResolver;
        private readonly ContentAreaRenderingOptions _contentAreaRenderingOptions;
        private readonly ModelExplorerFactory _modelExplorerFactory;
        private readonly IModelTemplateTagResolver _modelTagResolver;
        private readonly ITemplateResolver _templateResolver;
        private readonly IContentAreaItemAttributeAssembler _attributeAssembler;

        private bool? _isMethodsOverriden;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiquidContentAreaRenderer" /> class.
        /// </summary>
        public LiquidContentAreaRenderer()
            : this(
                ServiceLocator.Current.GetInstance<IContentRenderer>(),
                ServiceLocator.Current.GetInstance<ITemplateResolver>(),
                ServiceLocator.Current.GetInstance<IContentAreaItemAttributeAssembler>(),
                ServiceLocator.Current.GetInstance<IContentRepository>(),
                ServiceLocator.Current.GetInstance<IContentAreaLoader>(),
                ServiceLocator.Current.GetInstance<IContextModeResolver>(),
                ServiceLocator.Current.GetInstance<ContentAreaRenderingOptions>(),
                ServiceLocator.Current.GetInstance<ModelExplorerFactory>(),
                ServiceLocator.Current.GetInstance<IModelTemplateTagResolver>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiquidContentAreaRenderer"/> class.
        /// </summary>
        public LiquidContentAreaRenderer(IContentRenderer contentRenderer, ITemplateResolver templateResolver, IContentAreaItemAttributeAssembler attributeAssembler,
            IContentRepository contentRepository, IContentAreaLoader contentAreaLoader, IContextModeResolver contextModeResolver,
            ContentAreaRenderingOptions contentAreaRenderingOptions, ModelExplorerFactory modelExplorerFactory, IModelTemplateTagResolver modelTemplateTagResolver)
        {
            _contentRenderer = contentRenderer;
            _templateResolver = templateResolver;
            _attributeAssembler = attributeAssembler;
            _contentRepository = contentRepository;
            _contentAreaLoader = contentAreaLoader;
            _contextModeResolver = contextModeResolver;
            _contentAreaRenderingOptions = contentAreaRenderingOptions;
            _modelExplorerFactory = modelExplorerFactory;
            _modelTagResolver = modelTemplateTagResolver;
        }

        /// <summary>
        /// Renders the <see cref="ContentArea"/> to the writer on the current <see cref="HtmlHelper"/>.
        /// </summary>
        /// <param name="htmlHelper">The Html helper</param>
        /// <param name="contentArea">The content area</param>
        public override void Render(IHtmlHelper htmlHelper, ContentArea contentArea)
        {
            if (contentArea == null || contentArea.IsEmpty)
            {
                return;
            }

            var viewContext = htmlHelper.ViewContext;
            TagBuilder containerTag = null;

            // Render a start tag for the container if not in edit mode and should render wrapping element.
            if (!IsInEditMode() && ShouldRenderWrappingElement(htmlHelper))
            {
                containerTag = new TagBuilder(GetContentAreaHtmlTag(htmlHelper, contentArea));

                // Apply the CSS class defined in the template to the container element.
                AddNonEmptyCssClass(containerTag, viewContext.ViewData[RenderSettings.CssClass] as string);

                containerTag.RenderStartTag().WriteTo(viewContext.Writer, HtmlEncoder.Default);
                //OLD: viewContext.Writer.Write(containerTag.RenderStartTag());
            }

            RenderContentAreaItems(htmlHelper, FilteredItems(contentArea));

            // If we rendered a begin tag then we must render the end tag.
            if (containerTag != null)
            {
                containerTag.RenderEndTag().WriteTo(viewContext.Writer, HtmlEncoder.Default);
                //OLD: viewContext.Writer.Write(containerTag.RenderEndTag());
            }
        }

        /// <summary>
        /// Render a <see cref="ContentAreaItem"/>.
        /// </summary>
        /// <param name="htmlHelper">The html helper</param>
        /// <param name="contentAreaItem">The content area item to render</param>
        /// <param name="templateTag">The template tag used to resolve the display template</param>
        /// <param name="htmlTag">The html tag for the element wrapping the display template</param>
        /// <param name="cssClass">The css class for the element wrapping the display template</param>
        protected override void RenderContentAreaItem(IHtmlHelper htmlHelper, ContentAreaItem contentAreaItem, string templateTag, string htmlTag, string cssClass)
        {
            var renderSettings = new Dictionary<string, object>
            {
                [RenderSettings.ChildrenCustomTagName] = htmlTag,
                [RenderSettings.ChildrenCssClass] = cssClass,
                [RenderSettings.Tag] = templateTag
            };

            if (contentAreaItem.RenderSettings != null)
            {
                // Merge the rendersettings from the content area with the render settings for the fragment
                renderSettings = contentAreaItem.RenderSettings.Concat(renderSettings.Where(r => !contentAreaItem.RenderSettings.ContainsKey(r.Key)))
                            .ToDictionary(r => r.Key, r => r.Value);
            }
            // Store the render settings in the view bag.
            htmlHelper.ViewBag.RenderSettings = renderSettings;

            var content = _contentAreaLoader.LoadContent(contentAreaItem);

            if (content == null)
            {
                return;
            }

            var tags = Enumerable.Empty<string>();
            if (true || IsMethodsOverriden)
            {
                //if partner has overriden our methods we run with partner code instead of default
                var contentAreaTag = GetContentAreaTemplateTag(htmlHelper);
                tags = string.IsNullOrWhiteSpace(templateTag)
                    ? new[] { contentAreaTag }
                    : _contentAreaRenderingOptions.TemplateTagSelectionStrategy == MissingTemplateTagSelectionStrategy.NoTag || string.IsNullOrEmpty(contentAreaTag)
                        ? new[] { templateTag }
                        : new[] { templateTag, contentAreaTag };
            }
            else
            {
                tags = _modelTagResolver.Resolve(_modelExplorerFactory.CreateFromModel(contentAreaItem), htmlHelper.ViewContext);
            }

            // Resolve the template for the content fragment based on the given template tag.
            var templateModel = ResolveContentTemplate(htmlHelper, content, tags);
            // If there is no template and not in edit mode then don't render.
            if (templateModel == null && !IsInEditMode())
            {
                return;
            }

            using (new ContentRenderingScope(htmlHelper.ViewContext.HttpContext, content, templateModel, tags))
            {
                var tagBuilder = new TagBuilder(htmlTag);

                AddNonEmptyCssClass(tagBuilder, cssClass);

                // Applies the required edit attributes to the fragment.
                tagBuilder.MergeAttributes(_attributeAssembler.GetAttributes(contentAreaItem, IsInEditMode(), templateModel != null));

                // Allow partners to modify the start tag before rendering.
                BeforeRenderContentAreaItemStartTag(tagBuilder, contentAreaItem);

                var startTag = tagBuilder.RenderStartTag();
                var sb = new StringBuilder();
                using (var writer = new StringWriter(sb))
                {
                    startTag.WriteTo(writer, HtmlEncoder.Default);
                    var s = sb.ToString();
                }

                tagBuilder.RenderStartTag().WriteTo(htmlHelper.ViewContext.Writer, HtmlEncoder.Default);
                //OLD: htmlHelper.ViewContext.Writer.Write(tagBuilder.RenderStartTag());

                // Render the content
                htmlHelper.RenderContentData(content, true, templateModel, _contentRenderer);

                tagBuilder.RenderEndTag().WriteTo(htmlHelper.ViewContext.Writer, HtmlEncoder.Default);
                //OLD: htmlHelper.ViewContext.Writer.Write(tagBuilder.RenderEndTag());
            }
        }

        private static IEnumerable<ContentAreaItem> FilteredItems(ContentArea contentArea)
        {
            return contentArea.Fragments.GetFilteredFragments(PrincipalInfo.CurrentPrincipal).OfType<ContentFragment>().Select(f =>
            new ContentAreaItem(f));
        }

        private bool IsMethodsOverriden
        {
            get
            {
                if (!_isMethodsOverriden.HasValue)
                {
                    var isTemplateTagOverriden = GetType().GetMethod(nameof(GetContentAreaTemplateTag), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(IHtmlHelper) }, null).DeclaringType != typeof(ContentAreaRenderer);
                    var isItemTemplateTagOverriden = GetType().GetMethod(nameof(GetContentAreaItemTemplateTag), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(IHtmlHelper), typeof(ContentAreaItem) }, null).DeclaringType != typeof(ContentAreaRenderer);
                    _isMethodsOverriden = isTemplateTagOverriden || isItemTemplateTagOverriden;
                }
                return _isMethodsOverriden.Value;
            }
        }
    }
}