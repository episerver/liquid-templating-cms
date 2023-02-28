namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Processors
{
    /// <summary>
    /// Writes help content to the console
    /// </summary>
    public class HelpModeProcessor : IModeProcessor
    {
        
        public void Process()
        {        
            Console.WriteLine("Usage: opti-liquid-templates [mode] [hostname] [clientId] [clientSecret] [path-to-templates]");
            Console.WriteLine("");
            Console.WriteLine("Synchronize .liquid template files to a remote Optimizely Content Management instance");
            Console.WriteLine("");

            Console.WriteLine("mode:");
            Console.WriteLine("  pull\t\tDownload all remote .liquid template files and folder structure from a remote cms host and save to the specified local folder");
            Console.WriteLine("  push\t\tUpload all local .liquid template files and folders to the specified remote cms host");
            Console.WriteLine("  watch\t\tWatch for changes in the specified local folder and upload changes to the specified remote cms host");
            Console.WriteLine("");
            Console.WriteLine("hostname:");
            Console.WriteLine("  The hostname of the remote cms instance. Must have ContentDelivery and ContentManagement api installed with a client grant Auth scheme. Example - https://localhost:5000");
            Console.WriteLine("");
            Console.WriteLine("clientId:");
            Console.WriteLine("  The clientId configured to allow access to the remote cms ContentManagement api. Defaults to cli");
            Console.WriteLine("");
            Console.WriteLine("clientSecret:");
            Console.WriteLine("  The clientSecret configured to allow access to the remote cms ContentManagement api. Defaults to cli");
            Console.WriteLine("");
            Console.WriteLine("path-to-templates:");
            Console.WriteLine("  The relative path to your local templates from the location of this executable. Example - \\liquid-views");
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}

