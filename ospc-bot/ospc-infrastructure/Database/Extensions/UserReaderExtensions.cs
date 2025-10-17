using System.Data.Common;
using OSPC.Domain.Model;

namespace OSPC.Infrastructure.Database.Extensions
{
    public static class UserReaderExtensions
    {
        public static User ReadUser(this DbDataReader reader)
            => new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                CountryCode = reader.GetString(2),
                AvatarUrl = reader.GetString(3),
                ProfileColour = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
            };

        public static DiscordPlayer ReadDiscordPlayerMapping(this DbDataReader reader)
            => new DiscordPlayer
            {
                DiscordUserId = (ulong)reader.GetInt64(0),
                PlayerUserId = reader.GetInt32(1),
                PlayerUsername = reader.GetString(2)
            };
    }
}