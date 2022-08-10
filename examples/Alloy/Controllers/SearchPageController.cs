using Alloy.Liquid.Models.Pages;
using Alloy.Liquid.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Alloy.Liquid.Controllers;

public class SearchPageController : PageControllerBase<SearchPage>
{
    public ViewResult Index(SearchPage currentPage, string q)
    {
        var model = new SearchContentModel(currentPage)
        {
            Hits = Enumerable.Empty<SearchContentModel.SearchHit>(),
            NumberOfHits = 0,
            SearchServiceDisabled = true,
            SearchedQuery = q
        };

        return View("\\SearchPage\\index.liquid", model);
    }
}
