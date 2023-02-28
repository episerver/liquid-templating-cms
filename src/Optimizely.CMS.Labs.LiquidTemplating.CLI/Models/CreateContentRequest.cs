using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

public class CreateContentRequest : CreateFolderRequest
{
    public CreateContentRequest(CreateFolderRequest baseRequest)
    {
        name = baseRequest.name;
        contentType = baseRequest.contentType;
        parentLink = baseRequest.parentLink;
        routeSegment = baseRequest.routeSegment;
    }

    public Language language { get; set; }
    public DateTime startPublish { get; set; }
    public DateTime stopPublish { get; set; }
    public string status { get; set; }
   
    public string template { get; set; }
}

