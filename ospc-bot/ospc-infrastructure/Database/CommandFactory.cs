using MySql.Data.MySqlClient;
using OSPC.Domain.Model;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database
{
	public static class CommandFactory
	{
		public static MySqlCommand CreateGetBeatmapByIdCommand(MySqlConnection conn, int beatmapId)
			=> CreateQueryIdCommand("CALL GetBeatmapById(@Id)", conn, beatmapId);

		public static MySqlCommand CreateGetBeatmapSetByIdCommand(MySqlConnection conn, int beatmapSetId)
			=> CreateQueryIdCommand("CALL GetBeatmapSetById(@Id)", conn, beatmapSetId);

		private static MySqlCommand CreateQueryIdCommand(string query, MySqlConnection conn, int id)
		{
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@Id", id);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}

		public static MySqlCommand CreateGetPlaycountForBeatmapCommand(MySqlConnection conn, int userId, int beatmapId)
			=> CreateBeatmapPlaycountCommand("CALL GetPlaycountForBeatmap(@UserId, @BeatmapId)", conn, userId, beatmapId);

		public static MySqlCommand CreateGetBeatmapPlaycountForUserCommand(MySqlConnection conn, int userId, int beatmapId)
			=> CreateBeatmapPlaycountCommand("CALL GetBeatmapPlaycountForUser(@UserId, @BeatmapId)", conn, userId, beatmapId);

		private static MySqlCommand CreateBeatmapPlaycountCommand(string query, MySqlConnection conn, int userId, int beatmapId)
		{
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@UserId", userId);
			command.Parameters.AddWithValue("@BeatmapId", beatmapId);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}

		public static MySqlCommand CreateUpdateReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId, int beatmapId)
		{
			string query = "CALL UpdateReferencedBeatmapIdForChannel(@ChannelId, @BeatmapId)";
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@ChannelId", channelId);
			command.Parameters.AddWithValue("@BeatmapId", beatmapId);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}
		
		public static MySqlCommand CreateGetReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId)
		{
			string query = "CALL GetReferencedBeatmapIdForChannel(@ChannelId)";
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@ChannelId", channelId);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}

		public static MySqlCommand CreateAddDiscordPlayerMappingCommand(MySqlConnection conn, ulong discordUserId, int playerUserId)
		{
			string query = "CALL AddDiscordPlayerMapping(@DiscordUserId, @PlayerUserId)";
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
			command.Parameters.AddWithValue("@PlayerUserId", playerUserId);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}

		public static MySqlCommand CreateGetPlayerInfoFromDiscordIdCommand(MySqlConnection conn, ulong discordUserId)
		{
			string query = "CALL GetPlayerInfoFromDiscordId(@DiscordUserId)";
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}

		public static MySqlCommand CreateAddUserCommand(MySqlConnection conn, User user)
		{
			string query = "CALL AddUser(@Id, @Username, @CountryCode, @AvatarUrl, @ProfileColour)";
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@Id", user.Id);
			command.Parameters.AddWithValue("@Username", user.Username);
			command.Parameters.AddWithValue("@CountryCode", user.CountryCode);
			command.Parameters.AddWithValue("@AvatarUrl", user.AvatarUrl);
			command.Parameters.AddWithValue("@ProfileColour", user.ProfileColour);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}

		public static MySqlCommand CreateGetUserByIdCommand(MySqlConnection conn, int userId)
		{
			string query = "CALL GetUserById(@Id)";
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@Id", userId);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}


		public static MySqlCommand CreateGetUserByUsernameCommand(MySqlConnection conn, string username)
		{
			string query = "CALL GetUserByUsername(@Username)";
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@Username", username);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}


		public static MySqlCommand CreateGetUserWithDiscordIdCommand(MySqlConnection conn, ulong discordUserId)
		{
			string query = "CALL GetUserWithDiscordId(@DiscordUserId)";
			var command = new MySqlCommand(query, conn);
			command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
			return command;
		}

		public static MySqlCommand CreateBeatmapPlaycountFilterCommand(this SearchParams searchParams, MySqlConnection conn, int userId, int pageSize = -1, int pageNumber = -1)
        {
            (string query, bool generalQuery) = searchParams.CreateFilterQuery(userId, pageSize, pageNumber);
            MySqlCommand command = new MySqlCommand(query, conn);
            if (generalQuery) command.Parameters.AddWithValue("@query", searchParams.Query);
            else {
                command.Parameters.AddWithValue("@artist", searchParams.Artist);
                command.Parameters.AddWithValue("@title", searchParams.Title);
            }
			Console.WriteLine($"Creating command: {command.CommandText}");
			foreach (MySqlParameter param in command.Parameters) Console.WriteLine($"{param.ParameterName} => {param.Value}");
            return command;
        }

        private static (string, bool) CreateFilterQuery(this SearchParams searchParams, int userId, int pageSize, int pageNumber) 
        {
            bool countQuery = pageSize == -1 || pageNumber == -1;
            bool generalQuery = string.IsNullOrEmpty(searchParams.Artist) && string.IsNullOrEmpty(searchParams.Title);
            string artistConcat = generalQuery ? (searchParams.Exact && searchParams.Query != "" ? "":"%") : (searchParams.Exact && searchParams.Artist != "" ? "":"%");
            string titleConcat = generalQuery ? (searchParams.Exact && searchParams.Query != "" ? "":"%") : (searchParams.Exact && searchParams.Title != "" ? "":"%");
            string countComparison = ComparisonConverter.CreateComparisonClause(searchParams.Playcount, "BeatmapPlaycounts", "Count");
            string query = $@"
                    SELECT {(countQuery ? "COUNT(BeatmapPlaycounts.Count)":"BeatmapPlaycounts.*,Beatmaps.Id,Beatmaps.Version,Beatmaps.DifficultyRating,Beatmaps.BeatmapSetId,BeatmapSet.*")}
                    FROM BeatmapPlaycounts
                    JOIN Beatmaps ON BeatmapPlaycounts.BeatmapId = Beatmaps.Id 
                    JOIN BeatmapSet ON Beatmaps.BeatmapSetId = BeatmapSet.Id 
                    WHERE BeatmapPlaycounts.UserId = {userId}
                    {countComparison}
                    AND (
                        BeatmapSet.Title LIKE CONCAT('{titleConcat}', {(generalQuery ? "@query":"@title")}, '{titleConcat}') 
                        {(generalQuery ? "OR":"AND")} 
                        BeatmapSet.Artist LIKE CONCAT('{artistConcat}', {(generalQuery ? "@query":"@artist")}, '{artistConcat}')
                    )
                    {(searchParams.BeatmapFilter == null ? "":searchParams.BeatmapFilter.GetClause())}
                    {(
                        countQuery ? "":
                        $@"
                            ORDER BY BeatmapPlaycounts.Count DESC
                            LIMIT {pageSize}
                            OFFSET {(pageNumber-1) * pageSize}
                        "
                    )}
                ";
            return (query, generalQuery);
        }
	}
}
