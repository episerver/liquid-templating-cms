using Optimizely.CMS.Labs.LiquidTemplating.CLI.Reader;
using Optimizely.CMS.Labs.LiquidTemplating.CLI.Writer;
using System;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Processors
{
    /// <summary>
    /// Read all Liquid Items from from source (remote) and write to target (local)
    /// </summary>
    public class PullModeProcessor : IModeProcessor
    {
        private readonly ILiquidTemplateReader _remoteReader;
        private readonly ILiquidTemplateReader _localReader;
        private readonly ILiquidTemplateWriter _writer;

        public PullModeProcessor(ILiquidTemplateReader remoteReader, ILiquidTemplateReader localReader, ILiquidTemplateWriter writer)
        {
            _remoteReader = remoteReader;
            _localReader = localReader;
            _writer = writer;
        }

        public void Process()
        {
            //Read all files from local / remote
            var localItems = _localReader.GetAll();
            var remoteItems = _remoteReader.GetAll();
            Console.WriteLine($"Read {remoteItems.Count()} items from Remote | {localItems.Count()} items from Local ");
            Console.WriteLine("--");
            //Calculate operations needed
            var calculator = new TemplateSyncCalculator(remoteItems, localItems);

            //Process creation operations
            var itemsToCreate = calculator.ToCreate();
            Console.WriteLine($"{itemsToCreate.Count()} items to CREATE on Local");
            foreach (var item in itemsToCreate)
            {
                Console.WriteLine(item.Key);
            }
            _writer.Create(itemsToCreate);
            Console.WriteLine("Finished.");

            //Process delete operations
            var itemsToDelete = calculator.ToDelete();
            Console.WriteLine($"{itemsToDelete.Count()} items to DELETE on Local");
            foreach (var item in itemsToDelete)
            {
                Console.WriteLine(item.Key);
            }
            _writer.Delete(itemsToDelete);
            Console.WriteLine("Finished.");

            //ReRead all files from Local
            localItems = _localReader.GetAll();
            
            //ReCalculate operations needed
            calculator = new TemplateSyncCalculator(remoteItems, localItems);

            var itemsToUpdate = calculator.ToUpdate();
            Console.WriteLine($"{itemsToUpdate.Count()} items to UPDATE on Local");
            foreach (var item in itemsToUpdate)
            {
                Console.WriteLine(item.Key);
            }
            _writer.Update(itemsToUpdate);
            Console.WriteLine("Finished.");
            Console.WriteLine("--");
        }
    }
}

