using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Writer
{
    public interface ILiquidTemplateWriter
    {
        void Create(IEnumerable<ILiquidTemplateData> items);
        void Create(ILiquidTemplateData item);
        void Delete(IEnumerable<ILiquidTemplateData> items);
        void Delete(ILiquidTemplateData item);
        void Update(IEnumerable<ILiquidTemplateData> items);
        void Update(ILiquidTemplateData item);
    }
}