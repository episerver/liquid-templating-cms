using Optimizely.CMS.Labs.LiquidTemplating.CLI.Processors;
using Optimizely.CMS.Labs.LiquidTemplating.CLI.Reader;
using Optimizely.CMS.Labs.LiquidTemplating.CLI.Writer;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI;
class Program
{
    static void Main(string[] args)
    {
        string viewsRootGuid = "088d483d-aa5d-408f-8fa8-9d6cda9da03f";
       
        //cli mode
        Mode mode = Mode.Help;
        if (args.Length > 0)
        {
            object? outMode;
            var canParse = Enum.TryParse(typeof(Mode), args[0], true, out outMode);
            mode = canParse ? (Mode)outMode : Mode.Help;
        }

        //hostname of external CMS instance
        string hostName = "https://localhost:5000";
        if (args.Length > 1)
            hostName = args[1];

        //hostname of external CMS instance
        string clientId = "cli";
        if (args.Length > 2)
            clientId = args[2];

        //hostname of external CMS instance
        string clientSecret = "cli";
        if (args.Length > 3)
            clientSecret = args[3];

        //path in case tool is used in a different location to where files should be written
        string customPath = "";
        if (args.Length > 4)
            customPath = args[4];

        var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), customPath);

        Console.WriteLine("-------------------------------------------");
        Console.WriteLine("opti-liquid-templates cli for Optimizely CMS");
        Console.WriteLine("-------------------------------------------");


        if (mode != Mode.Help)
        {
            Console.WriteLine($"Mode {mode}. Operating on '{hostName}' from local path '{workingDirectory}'");
            Console.WriteLine("---");

            if (string.IsNullOrEmpty(hostName) || !hostName.StartsWith("http") || hostName.EndsWith('/'))
            {
                Console.WriteLine("[hostname] argument not supplied or supplied with incorrect format. Must start with 'http' and not end with a trailing slash.");
                return;
            }
        }

        var apiAuthenticator = new ContentApiAuthenticator(hostName, clientId, clientSecret);

        var localReader = new LocalFileTemplateReader(workingDirectory);
        var localWriter = new LocalFileWriter(workingDirectory);

        var remoteReader = new ContentDeliveryTemplateReader(hostName);
        var remoteWriter = new ContentManagementWriter(hostName, apiAuthenticator, remoteReader);

        IModeProcessor processor = new HelpModeProcessor();

        switch (mode)
        {
            case Mode.Pull:
                 processor = new PullModeProcessor(remoteReader, localReader, localWriter);
                break;
            case Mode.Push:
                processor = new PushModeProcessor(remoteReader, localReader, remoteWriter);
                break;
            case Mode.Watch:
                processor = new WatchModeProcessor(remoteReader, localReader, remoteWriter, workingDirectory);
                break;
            default:
                break;
        }

        processor.Process();
    }
}