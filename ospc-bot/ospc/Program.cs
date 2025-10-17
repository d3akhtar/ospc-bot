using OSPC.Bot;

namespace OSPC
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var cmdArgs = CommandLineArgs.Parse(args);
            await new BotClient(DependencyInjection.GetServiceCollection(cmdArgs)).StartAsync();
        }
    }
}
