namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

public interface ILiquidTemplateData
{
    public string Key { get; }
    public string ContentId { get; }
    public string Name { get; }
    public LiquidContentItem LiquidContentType { get; }
    public string Template { get; }
}