using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;

namespace Optimizely.CMS.Labs.LiquidTemplating.Content
{
    [Component]
    public class LiquidTemplateTreeComponent : ComponentDefinitionBase
    {
        public LiquidTemplateTreeComponent() : base("epi-cms/component/Media")
        {
            base.Title = "Liquid";
     
            Categories = new[] { "content" };
            PlugInAreas = new[] { PlugInArea.AssetsDefaultGroup };
            LanguagePath = "/episerver/cms/components/globalsettings";
            SortOrder = 1000;
            //TODO - Set access to component based on configuration - AllowedRoles = "" or AllowedRoles = "WebAdmins, TemplateAdmins"

            base.Settings.Add(new Setting("repositoryKey", Constants.RootKey));
        }
    }
}
