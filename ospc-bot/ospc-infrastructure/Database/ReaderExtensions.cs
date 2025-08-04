using System.Data.Common;
using OSPC.Domain.Model;

namespace OSPC.Infrastructure.Database
{
	public static class BeatmapReaderExtensions
	{
        public static BeatmapPlaycount ReadBeatmapPlaycount(this DbDataReader reader) 
            =>  new BeatmapPlaycount {
                    UserId = reader.GetInt32(0),
                    BeatmapId = reader.GetInt32(1),
                    Count = reader.GetInt32(2),
                };

        public static BeatmapPlaycount ReadBeatmapPlaycountInclude(this DbDataReader reader)
            =>  new BeatmapPlaycount {
                    UserId = reader.GetInt32(0),
                    BeatmapId = reader.GetInt32(1),
                    Count = reader.GetInt32(2),
                    Beatmap = new Beatmap {
                        Id = reader.GetInt32(3),
                        Version = reader.IsDBNull(4) ? string.Empty:reader.GetString(4),
                        DifficultyRating = reader.GetFloat(5),
                        BeatmapSetId = reader.GetInt32(6)
                    },
                    BeatmapSet = new BeatmapSet {
                        Id = reader.GetInt32(7),
                        Artist = reader.GetString(8),
                        Title = reader.GetString(9),
                        Covers = new Covers {
                            SlimCover2x = reader.GetString(10)
                        },
                        UserId = reader.GetInt32(11)
                    }
                };

        public static Beatmap ReadBeatmap(this DbDataReader reader)
            => new Beatmap {
                Id = reader.GetInt32(0),
                Version = reader.IsDBNull(1) ? string.Empty:reader.GetString(1),
                DifficultyRating = reader.GetFloat(2),
                BeatmapSetId = reader.GetInt32(3)
            };

        public static BeatmapSet ReadBeatmapSet(this DbDataReader reader)
            => new BeatmapSet {
                Id = reader.GetInt32(0),
                Artist = reader.GetString(1),
                Title = reader.GetString(2)
            };		
	}

	public static class UserReaderExtensions
	{		
        public static User ReadUser(this DbDataReader reader) 
            =>  new User {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    CountryCode = reader.GetString(2),
                    AvatarUrl = reader.GetString(3),
                    ProfileColour = reader.IsDBNull(4) ? string.Empty:reader.GetString(4)
                };

        public static DiscordPlayer ReadDiscordPlayerMapping(this DbDataReader reader)
            => new DiscordPlayer {
                DiscordUserId = (ulong)reader.GetInt64(0),
                PlayerUserId = reader.GetInt32(1),
                PlayerUsername = reader.GetString(2)
            };
	}
}
