using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.IO;
using System.Linq;

namespace Optimizely.CMS.Labs.LiquidTemplating.Content
{
    /// <summary>
    /// Module for ensuring a root container for LiquidTemplateData object exists and set up of required Content Events for LiquidTemplateData
    /// </summary>
    [ModuleDependency(typeof(InitializationModule))]
    [InitializableModule]
    public class LiquidTemplateContentEventsInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            //TODO return if EnableUserInterface CMSFluidOptions = false;
            
            var contentRootService = context.Locate.Advanced.GetInstance<ContentRootService>();
            contentRootService.Register<ContentFolder>(Constants.RootName, new Guid(Constants.RootGuid), ContentReference.RootPage);
            var root = contentRootService.Get(Constants.RootName);

            //Create direct child folder Views
            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            var children = repo.GetChildren<ContentFolder>(root);
            if (!children.Any())
            {
                var newAssetFolder = repo.GetDefault<ContentFolder>(root);
                newAssetFolder.Name = "Views";
                repo.Publish(newAssetFolder, EPiServer.Security.AccessLevel.NoAccess);
            }

            //Register blob / text sync events
            var events = ServiceLocator.Current.GetInstance<IContentEvents>();
            events.CreatingContent += Events_CreatingContent;
            events.SavingContent += Events_SavingContent;
        }

        private void Events_SavingContent(object sender, ContentEventArgs e)
        {
            //Ensure Template and Blob data from LiquidTemplateData are synchronised
            if (e.Content is LiquidTemplateData)
            {
                //Liquid Template Content has been changed
                var liquidData = e.Content as LiquidTemplateData;

                if (liquidData.BinaryData != null)
                {
                    if (liquidData.BinaryData.ToString() != liquidData.ComparisonBinaryData.ToString())
                    {
                        //Binarydata exists but is different to comparison - must have been reuploaded
                        liquidData.ComparisonBinaryData = liquidData.BinaryData;
                        liquidData.Template = liquidData.BinaryDataAsString;
                    }
                    else
                    {
                        //sync blob from template field
                        var blobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();

                        var blob = blobFactory.CreateBlob(liquidData.BinaryDataContainer, ".liquid");
                        using (var s = blob.OpenWrite())
                        {
                            var w = new StreamWriter(s);
                            w.BaseStream.Write(liquidData.TemplateAsBytes, 0, liquidData.TemplateAsBytes.Length);
                            w.Flush();
                        }

                        liquidData.BinaryData = blob;
                        liquidData.ComparisonBinaryData = liquidData.BinaryData;
                    }
                }
            }
        }

        private void Events_CreatingContent(object sender, ContentEventArgs e)
        {
            //Create tenmplate field from Blob data if uploaded
            if (e.Content is LiquidTemplateData)
            {
                //Read blob data and write to Template field
                var liquidData = e.Content as LiquidTemplateData;

                if (liquidData.BinaryData != null)
                {
                    liquidData.Template = liquidData.BinaryDataAsString;
                    liquidData.ComparisonBinaryData = liquidData.BinaryData;
                }
            }

            return;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var events = ServiceLocator.Current.GetInstance<IContentEvents>();
            events.CreatingContent -= Events_CreatingContent;
            events.SavingContent -= Events_SavingContent;
        }
    }
}