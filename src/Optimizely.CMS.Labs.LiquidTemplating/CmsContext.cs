using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Globalization;
using EPiServer.Web;
using System.Globalization;

namespace Optimizely.CMS.Labs.LiquidTemplating
{
    public class CmsContext
    {
        private readonly IContextModeResolver _contextModeResolver;
        private readonly IDatabaseMode _databaseMode;

        public CmsContext(IContextModeResolver contextModeResolver, IDatabaseMode databaseMode)
        {
            _contextModeResolver = contextModeResolver;
            _databaseMode = databaseMode;
        }

        public bool IsInEditMode => _contextModeResolver.CurrentMode == ContextMode.Edit;

        public bool IsInReadOnlyMode => _databaseMode.DatabaseMode == DatabaseMode.ReadOnly;

        public ContentReference StartPage => ContentReference.StartPage;

        public ContentReference RootPage => ContentReference.RootPage;

        public CultureInfo Culture => ContentLanguage.PreferredCulture;
    }
}
