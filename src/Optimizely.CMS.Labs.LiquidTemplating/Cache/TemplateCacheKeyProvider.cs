using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Optimizely.CMS.Labs.LiquidTemplating.Cache
{
    public class TemplateCacheKeyProvider
    {
        private readonly IHttpContextAccessor _context;
        private readonly CmsFluidTemplateCacheOptions _options;

        public TemplateCacheKeyProvider(IHttpContextAccessor context, IOptions<CmsFluidTemplateCacheOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public string GetCacheKey(string path)
        {
            var key = new StringBuilder(path.Length);

            if (_options.TemplateCacheKeyUseEpiserverUserId)
            {
                if (_context.HttpContext.User != null && _context.HttpContext.User.Identity.IsAuthenticated)
                {
                    var userId = _context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    key.Append($"{userId}:");
                }
            }

            if (!string.IsNullOrEmpty(_options.TemplateCacheKeyCookieName)
                && _context.HttpContext.Request.Cookies.ContainsKey(_options.TemplateCacheKeyCookieName))
            {
                key.Append($"{_context.HttpContext.Request.Cookies[_options.TemplateCacheKeyCookieName]}:");
            }

            key.Append(path);

            return key.ToString();
        }
    }
}
