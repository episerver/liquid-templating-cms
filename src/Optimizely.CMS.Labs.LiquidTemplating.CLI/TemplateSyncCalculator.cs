using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI
{
    public class TemplateSyncCalculator
    {
        private readonly IEnumerable<ILiquidTemplateData> _sourceList;
        private readonly IEnumerable<ILiquidTemplateData> _targetList;

        public TemplateSyncCalculator(IEnumerable<ILiquidTemplateData> sourceList, IEnumerable<ILiquidTemplateData> targetList)
        {
            _sourceList = sourceList;
            _targetList = targetList;
        }

        public IEnumerable<ILiquidTemplateData> ToCreate()
        {
            var itemsToCreate = _sourceList.Except(_targetList, new LiquidTemplateDataComparer());
            //order by folder, then length of key. Longer keys last to ensure hierarchy is created first
            return itemsToCreate.OrderBy(i => i.LiquidContentType).ThenBy(i => i.Key.Length);
        }

        public IEnumerable<ILiquidTemplateData> ToUpdate()
        {
            return _sourceList.Except(_targetList, new LiquidTemplateDataContentComparer());
        }

        public IEnumerable<ILiquidTemplateData> ToDelete()
        {
            var itemsToDelete = _targetList.Except(_sourceList, new LiquidTemplateDataComparer());
            //order by file then folder. Longer keys first to delete from base of hierarchy
            return itemsToDelete.OrderByDescending(i => i.LiquidContentType).ThenByDescending(i => i.Key.Length);
        }
    }
}
