using EPiServer.Core;
using EPiServer.DataAbstraction;
using Optimizely.CMS.Labs.LiquidTemplating.Values;
using EPiServer.SpecializedProperties;
using EPiServer.Web.Mvc.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.IO;
using System.Text.Encodings.Web;
using EPiServer;

namespace Optimizely.CMS.Labs.LiquidTemplating.Rendering
{
    public class PropertyRenderer
    {
        public void Render(TextWriter w, IHtmlHelper htmlHelper, object property, string propertyName, bool isInEditMode, string customTag)
        {
            if (property is ContentAreaValue)
            {
                foreach (var item in property as IEnumerable)
                {
                    RenderInternal(w, htmlHelper, item, propertyName, isInEditMode, true, customTag);
                }
            }
            else
            {
                RenderInternal(w, htmlHelper, property, propertyName, isInEditMode, false, customTag);
            }
        }

        private void RenderInternal(TextWriter w, IHtmlHelper htmlHelper, object input, string propertyName, bool isInEditMode, bool IsInContentArea, string customTag)
        {
            if (string.IsNullOrEmpty(customTag))
                customTag = "div";
            
            if (isInEditMode)
            {
                // Get the property name from the last segment of the passed in expression
                htmlHelper.BeginEditSection(customTag, propertyName).WriteTo(w, HtmlEncoder.Default);
            }

            if (input is ContentData)
            {
                htmlHelper.RenderContentData(input as ContentData, IsInContentArea);
            }
            else if (input is ContentArea)
            {
                htmlHelper.RenderContentArea(input as ContentArea);
            }
            else if (input is XhtmlString)
            {
                htmlHelper.RenderXhtmlString(input as XhtmlString);
            }
            else if (input is ContentReference)
            {
                var html = htmlHelper.ContentLink(input as ContentReference);
                html.WriteTo(w, HtmlEncoder.Default);
            }
            else if (input is PageReference)
            {
                var html = htmlHelper.PageLink(input as PageReference);
                html.WriteTo(w, HtmlEncoder.Default);
            }
            else if (input is Url)
            {
                var html = htmlHelper.ContentLink(input as Url);
                html.WriteTo(w, HtmlEncoder.Default);
            }
            else if (input is LinkItemCollection)
            {
                w.WriteLine("<ul>");
                foreach (var linkItem in input as LinkItemCollection)
                {
                    w.WriteLine("<li>");
                    var html = htmlHelper.ContentLink(linkItem);
                    html.WriteTo(w, HtmlEncoder.Default);
                    w.WriteLine("</li>");
                }
                w.WriteLine("</ul>");

            }
            else if (input is LinkItem)
            {
                var html = htmlHelper.ContentLink(input as LinkItem);
                html.WriteTo(w, HtmlEncoder.Default);
            }
            else if (input is PageType)
            {
                w.Write((input as PageType).LocalizedFullName);
            }
            else if (input is CategoryList)
            {
                var html = htmlHelper.CategoryList(input as CategoryList);
                html.WriteTo(w, HtmlEncoder.Default);
            }
            else if (input is IEnumerable)
            {
                w.WriteLine("<ul>");
                foreach (var item in input as IEnumerable)
                {
                    w.Write("<li>");
                    w.Write(item.ToString());
                    w.WriteLine("</li>");
                }
                w.WriteLine("</ul>");
            }

            else if (input != null)
            {
                w.Write(input.ToString());
            }

            if (isInEditMode)
            {
                htmlHelper.EndEditSection(customTag).WriteTo(w, HtmlEncoder.Default);
            }
        }
    }
}
