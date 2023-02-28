namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Models
{
    public class CreateFolderRequest
    {
        public string name { get; set; }
        public List<string> contentType { get; set; }
        public ContentLink parentLink { get; set; }
        public string routeSegment { get; set; }
    }
}
