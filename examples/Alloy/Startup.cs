using Alloy.Liquid.Extensions;
using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer.OpenIDConnect;
using Fluid.MvcViewEngine;
using Optimizely.CMS.Labs.LiquidTemplating.ViewEngine;
using EPiServer.ContentApi.Cms;
using EPiServer.ContentManagementApi;

namespace Alloy.Liquid;

public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;

    public Startup(IWebHostEnvironment webHostingEnvironment)
    {
        _webHostingEnvironment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data"));

            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddAlloy()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Startup>();

        services.AddMvc().AddFluid().AddCmsFluid(_webHostingEnvironment);

    
    services.AddOpenIDConnect<ApplicationUser>(
        useDevelopmentCertificate: true, 
        signingCertificate: null, 
        encryptionCertificate: null, 
        createSchema: false, 
        options =>
        {
            // machine-to-machine API calls
            options.Applications.Add(new OpenIDConnectApplication
            {
                ClientId = "cli",
                ClientSecret = "cli",
                Scopes = { ContentManagementApiOptionsDefaults.Scope }
            });
        });

        services.AddContentDeliveryApi();

        services.AddContentManagementApi(OpenIDConnectOptionsDefaults.AuthenticationScheme, options =>
        {
            options.DisableScopeValidation = true;
        });
        services.ConfigureContentApiOptions(o =>
        {                
            o.FlattenPropertyModel = true;                
        });

        // Required by Wangkanai.Detection
        services.AddDetection();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Required by Wangkanai.Detection
        app.UseDetection();
        app.UseSession();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors(b => b
           .WithOrigins(new[] { "http://localhost:5000" })
           .WithExposedContentDeliveryApiHeaders()
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials());

        app.UseMiddleware<HideRouteFromProductionMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapContent();
        });

       
    }
}
