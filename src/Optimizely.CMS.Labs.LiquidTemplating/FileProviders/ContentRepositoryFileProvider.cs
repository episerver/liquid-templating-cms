using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Optimizely.CMS.Labs.LiquidTemplating.Content;
using System;
using System.Diagnostics;

namespace Optimizely.CMS.Labs.LiquidTemplating.FileProviders
{
    /// <summary>
    /// Implementaton of IFileProvider that retrieves file content from CMS Content
    /// </summary>
    public class ContentRepositoryFileProvider : IFileProvider
    {
        public ContentRepositoryFileProvider()
        {
            //TODO - why null? When is this instantiatiated?
            //_rootService = ServiceLocator.Current.GetInstance<ContentRootService>();
            //_contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var rootService = ServiceLocator.Current.GetInstance<ContentRootService>();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

            if (!subpath.Contains(".liquid"))
                return new NotFoundFileInfo(subpath);

            var root = rootService.Get("Liquid");
            var viewsRoot = contentLoader.GetBySegment(root, "Views", ContentLanguage.PreferredCulture);
            var currentResolved = viewsRoot.ContentLink;

            IContent resolvedContent = null; 

            var segments = subpath.Split('/');
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (string.IsNullOrEmpty(segment))
                    continue;

                var segmentPage = contentLoader.GetBySegment(currentResolved, segment, ContentLanguage.PreferredCulture);
                if (segmentPage == null)
                    break;
                
                currentResolved = segmentPage.ContentLink;

                if (segment.Contains(".liquid") && i == segments.Length - 1 && segmentPage is LiquidTemplateData)
                {
                    resolvedContent = segmentPage;
                    break;
                }
            }

            return resolvedContent != null 
                ? new ContentRepositoryFileInfo(resolvedContent as LiquidTemplateData) 
                : new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return new ContentRepositoryChangeToken();
        }
    }
}
