using Microsoft.Extensions.Options;

namespace Optimizely.CMS.Labs.LiquidTemplating
{
    /// <summary>
    /// Defines the default configuration of <see cref="CmsFluidOptions"/>.for Optimizely CMS
    /// </summary>
    public class CmsFluidOptionsSetup : ConfigureOptions<CmsFluidOptions>
    {
        public CmsFluidOptionsSetup() : base(options =>
         {
             options.EnableTemplateEditingUserInterface = false;
             options.EnableProductionMetaDataEndpoint = false;
         })
        { 
        }
    }
}