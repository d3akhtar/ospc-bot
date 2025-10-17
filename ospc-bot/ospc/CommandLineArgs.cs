namespace OSPC
{
	public class CommandLineArgs
	{
		public required bool DisableCaching { get; set; }
        public required string AppsettingsFilePath { get; set; }

		public static CommandLineArgs Parse(string[] args)
		{
			var botArgs = Default;
			
			for(int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "--caching":
					case "-c":
						{
							botArgs.DisableCaching = bool.Parse(args[++i]); 
							break;
						}
                    case "--appsettings":
                    case "-a":
                        {
                            botArgs.AppsettingsFilePath = args[++i];
                            break;
                        }
					default: throw new ArgumentException($"Invalid flag: {args[i]}");
				}
			}

			return botArgs;
		}

		public static CommandLineArgs Default => new()
		{
			DisableCaching = false,
            AppsettingsFilePath = Path.Join(Directory.GetCurrentDirectory(), "appsettings.json")
		};
	}
}
