namespace Optimizely.CMS.Labs.LiquidTemplating
{
    public class CmsFluidTemplateCacheOptions
    {
        /// <summary>
        /// The name of a cookie whose value should be used when creating the template cache key
        /// </summary>
        public string TemplateCacheKeyCookieName { get; set; }

        /// <summary>
        /// Whether to include the ID of the logged in Episerver user in the template cache key
        /// </summary>
        public bool TemplateCacheKeyUseEpiserverUserId { get; set; }
    }
}
