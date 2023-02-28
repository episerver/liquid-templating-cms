using System;
using System.Linq;
using EPiServer.Core;
using Optimizely.CMS.Labs.LiquidTemplating.Filters;
using EPiServer.ServiceLocation;
using Fluid;
using Fluid.MvcViewEngine;
using Fluid.Values;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.CMS.Labs.LiquidTemplating.Values;
using EPiServer.SpecializedProperties;
using Microsoft.Extensions.Options;
using Fluid.ViewEngine;
using Microsoft.Extensions.FileProviders;
using Optimizely.CMS.Labs.LiquidTemplating.FileProviders;

namespace Optimizely.CMS.Labs.LiquidTemplating.ViewEngine
{
    public static class CmsMvcBuilderExtensions
    {
        public static IMvcBuilder AddCmsFluid(this IMvcBuilder builder, IWebHostEnvironment environment, Action<CmsFluidOptions> setupAction = null)
        {
            if (builder == null)
            {
                throw new ArgumentException(nameof(builder));
            }

            builder.Services.AddOptions();
            builder.Services.AddTransient<IConfigureOptions<CmsFluidOptions>, CmsFluidOptionsSetup>();

            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }
            
            //Add CmsContext as per request
            builder.Services.AddScoped<CmsContext>();

            // TODO make this a configurable option?
            bool registerCmsFluidTags = false;
            var parser = registerCmsFluidTags ? new CmsFluidViewParser(new FluidParserOptions { AllowFunctions = true })
                                    : new FluidViewParser(new FluidParserOptions { AllowFunctions = true });

            var mappedFileProvider = new FileProviderMapper(environment.ContentRootFileProvider, "Views");
            var contentRepoFileProvider = new ContentRepositoryFileProvider();

            var compositeFileProviders = new CompositeFileProvider(contentRepoFileProvider, mappedFileProvider);
            //var compositeFileProviders = new DebugFileProvider(mappedFileProvider);

            //Required configuration for Fluid MVC
            builder.Services.Configure<FluidMvcViewOptions>(options =>
            {
                options.RenderingViewAsync = FluidRenderingViewAsync.AddItemsToFluidContext;
                options.ViewsFileProvider = compositeFileProviders;
                options.PartialsFileProvider = compositeFileProviders;
                options.Parser = parser;
                options.TemplateOptions.MemberAccessStrategy = UnsafeMemberAccessStrategy.Instance;
                options.TemplateOptions.Filters
                    .WithUrlFilters()
                    .WithContentReferenceFilters()
                    .WithContentFilters()
                    .WithContentTypeFilters()
                    .WithArrayFilters();

                options.TemplateOptions.ValueConverters.Add((v) =>
                {
                    // We need this as otherwise it gets treated as an XhtmlString (below)
                    if (v is ContentArea c)
                        return new ContentAreaValue(c);

                    if (v is LinkItemCollection i)
                        return new ObjectValue(i);

                    if (v is XhtmlString x)
                        return new XhtmlStringValue(x);

                    if (v is PropertyDataCollection)
                    {
                        var propDict = ((PropertyDataCollection)v).ToDictionary(x => x.Name, x => x.Value);
                        return new DictionaryValue(new DictionaryDictionaryFluidIndexable(propDict, options.TemplateOptions));
                    }

                    return null;
                });

                options.ViewsLocationFormats.Add("/Shared/PagePartials/{0}" + Constants.ViewExtension);
                options.ViewsLocationFormats.Add("/Shared/Blocks/{0}" + Constants.ViewExtension);

                //TODO - Required for Jupiter integration. Dependent on https://github.com/sebastienros/fluid/pull/486
                //builder.Services.AddSingleton<TemplateCacheKeyProvider>();
                //var templateCacheKeyprovider = builder.Services.BuildServiceProvider().GetRequiredService<TemplateCacheKeyProvider>();
                //options.TemplateCacheKeyProvider = templateCacheKeyprovider.GetCacheKey;
                // Template cache key configuration
                //builder.Services.Configure<CmsFluidTemplateCacheOptions>(options =>
                //{
                //    options.TemplateCacheKeyUseEpiserverUserId = true;
                //});
            });

            return builder;
        }

    }
}