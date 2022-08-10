namespace Optimizely.CMS.Labs.LiquidTemplating
{
    public class CmsFluidOptions
    {
        /// <summary>
        /// Set {true} to allow access to ModelMetaData api, on the production environments based on app.environment configuration
        /// </summary>
        public bool EnableProductionMetaDataEndpoint { get; set; }
    }
}
