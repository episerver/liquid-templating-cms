using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Reader
{
    public interface ILiquidTemplateReader
    {
        ILiquidTemplateData? Get(string contentReference);
        List<ILiquidTemplateData> GetAll();
    }
}