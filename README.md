## Setup

### Prerequsites
- .NET 8
- MySQL
- Redis

You can set up MySQL and Redis either by installing them or by running them through docker. In either case, you will need to provide the correct connection strings in the appsettings.json file
For now, to migrate/drop the tables and stored procedures, run the SQL scripts in [.mysql-config](https://github.com/d3akhtar/ospc-bot/tree/main/ospc-bot/.mysql-config)

You also need to create an osu [OAuth application](https://osu.ppy.sh/home/account/edit) and replace the [ClientId] and [ClientSecret] values in the appsettings file, as well as a [Discord application](https://discord.com/developers/applications). Make sure the Discord bot application has 'Bot' >> 'Message Content Intent' option enabled. 

### Running

Run the application by running ```dotnet run``` in the root of the repository. You can also pass an argument to disable caching by running ```dotnet run --caching false``` or ```dotnet run -c false```
