using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Optimizely.CMS.Labs.LiquidTemplating;
using Optimizely.CMS.Labs.LiquidTemplating.Api;
using System.Threading.Tasks;

public class HideRouteFromProductionMiddleware
{
	private RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    private readonly CmsFluidOptions _configuration;

	public HideRouteFromProductionMiddleware(RequestDelegate next, IOptions<CmsFluidOptions> option, IWebHostEnvironment hostingEnvironment)
	{
		_next = next;
        _env = hostingEnvironment;
        _configuration = option.Value;
	}

	public Task Invoke(HttpContext httpContext)
	{
		var isSensitiveEndpoint = httpContext
									.GetEndpoint()?.Metadata
									.GetMetadata<HideRouteFromProductionAttribute>();
		
		if (isSensitiveEndpoint == null)
			return _next(httpContext);

		if (_env.IsProduction() && !_configuration.EnableProductionMetaDataEndpoint)
        {
			httpContext.SetEndpoint(new Endpoint((context) =>
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				return Task.CompletedTask;
			},
						EndpointMetadataCollection.Empty,
						"Metadata endpoints are disabled on this environment"));
		}

		return _next(httpContext);
	}
}