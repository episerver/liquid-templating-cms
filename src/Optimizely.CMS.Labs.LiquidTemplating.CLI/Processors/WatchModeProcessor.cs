using Optimizely.CMS.Labs.LiquidTemplating.CLI.Reader;
using Optimizely.CMS.Labs.LiquidTemplating.CLI.Writer;
using System;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Processors
{
    /// <summary>
    /// Watches for changes in given directory and pushs all changes to remote.
    /// </summary>
    public class WatchModeProcessor : IModeProcessor
    {
        private readonly ILiquidTemplateReader _remoteReader;
        private readonly ILiquidTemplateReader _localReader;
        private readonly ILiquidTemplateWriter _writer;
        private readonly string _path;
        private readonly PushModeProcessor _processor;

        private bool _changeToBeProcessed;

        public WatchModeProcessor(ILiquidTemplateReader remoteReader, ILiquidTemplateReader localReader, ILiquidTemplateWriter writer, string path)
        {
            _remoteReader = remoteReader;
            _localReader = localReader;
            _writer = writer;
            _path = path;
            _processor = new PushModeProcessor(remoteReader, localReader, writer);
        }

        public void Process()
        {
            if (!Directory.Exists(_path))
            { 
                Directory.CreateDirectory(_path);
            }
            Console.WriteLine($"Waiting for changes...");

            using var watcher = new FileSystemWatcher(_path);

            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.Filter = "*.liquid";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private void ProcessAllChanges(FileSystemEventArgs e) 
        {
            if (_changeToBeProcessed)
                return;

            _changeToBeProcessed = true;

            Console.WriteLine($"Change detected: {e.Name}");
            Console.WriteLine("--");
            _changeToBeProcessed = true;
            Thread.Sleep(1000);
            _processor.Process();
            _changeToBeProcessed = false;          
        }

        private void OnChanged(object sender, FileSystemEventArgs e) => ProcessAllChanges(e);
        
        private void OnCreated(object sender, FileSystemEventArgs e) => ProcessAllChanges(e);

        private void OnDeleted(object sender, FileSystemEventArgs e) => ProcessAllChanges(e);

        private void OnRenamed(object sender, RenamedEventArgs e) => ProcessAllChanges(e);

        private static void OnError(object sender, ErrorEventArgs e) => PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }
    }
}


