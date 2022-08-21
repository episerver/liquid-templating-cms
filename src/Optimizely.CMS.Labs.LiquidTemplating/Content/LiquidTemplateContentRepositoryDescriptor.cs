using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Optimizely.CMS.Labs.LiquidTemplating.Content
{
    [ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
    public class LiquidTemplateContentRepositoryDescriptor : ContentRepositoryDescriptorBase
    {
        private readonly ContentReference _root;

        public LiquidTemplateContentRepositoryDescriptor(ContentRootService contentRootService)
        {
            //TODO - Why ContentRootService sometimes null?
            //var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            _root = contentRootService.Get("Liquid");
            if (_root == null)
                Debug.WriteLine(_root.ID);

            //_root = page.ContentLink;

            //var pageroots = contentLoader.GetChildren<IContent>(ContentReference.RootPage);
            //var liquidTemplatesRoot = pageroots.FirstOrDefault(c => c.Name == "Liquid");
            //if (liquidTemplatesRoot != null)
            //{
            //    _root = liquidTemplatesRoot.ContentLink;
            //}
            //else
            //{
            //_root = ContentReference.RootPage;
            //}

             _root = new ContentReference(115);
        }

        public override string Key => Constants.RootKey;
        public override string Name => Constants.RootName;

        public override IEnumerable<ContentReference> Roots => new [] { _root };

        public override IEnumerable<Type> ContainedTypes => new [] { typeof(ContentFolder), typeof(LiquidTemplateData) };
        public override IEnumerable<Type> CreatableTypes => new[] { typeof(ContentFolder), typeof(LiquidTemplateData) };
        public override IEnumerable<Type> LinkableTypes => new[] { typeof(LiquidTemplateData) };
        public override IEnumerable<Type> MainNavigationTypes => new[] { typeof(ContentFolder) };
    }
}
