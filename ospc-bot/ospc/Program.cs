using OSPC.Bot;

namespace OSPC
{
    public class Program
    {
        public static async Task Main(string[] args)
            => await new BotClient(CommandLineArgs.Parse(args)).StartAsync();
    }
}
