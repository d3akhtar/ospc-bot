USE ospc;

-- Beatmap related
DELIMITER //

CREATE PROCEDURE IF NOT EXISTS AddBeatmapPlaycount(
    IN UserId INT,
    IN BeatmapId INT,
    IN UpdatedCount INT
)
BEGIN
    INSERT INTO BeatmapPlaycounts VALUES (UserId, BeatmapId, UpdatedCount)
    ON DUPLICATE KEY UPDATE Count=UpdatedCount;
END //

CREATE PROCEDURE IF NOT EXISTS AddBeatmap(
    IN Id INT,
    IN Version VARCHAR(255),
    IN DifficultyRating FLOAT,
    IN BeatmapSetId INT,
    IN CircleSize FLOAT,
    IN BPM FLOAT,
    IN Length FLOAT,
    IN HpDrain FLOAT,
    IN OD FLOAT,
    IN AR FLOAT
)
BEGIN
    INSERT IGNORE INTO Beatmaps VALUES
    (
        Id,
        Version,
        DifficultyRating,
        BeatmapSetId,
        CircleSize,
        BPM,
        Length,
        HpDrain,
        OD,
        AR
    );
END //

CREATE PROCEDURE IF NOT EXISTS AddBeatmapSet(
    IN Id INT,
    IN Artist VARCHAR(255),
    IN Title VARCHAR(255),
    IN SlimCover2x VARCHAR(255),
    IN UserId INT
)
BEGIN
    INSERT IGNORE INTO BeatmapSet VALUES
    (
        Id,
        Artist,
        Title,
        SlimCover2x,
        UserId  
    );
END //

CREATE PROCEDURE IF NOT EXISTS GetBeatmapById(
    IN Id INT
)
BEGIN
    SELECT * FROM Beatmaps b WHERE b.Id=Id;
END //

CREATE PROCEDURE IF NOT EXISTS GetBeatmapSetById(
    IN Id INT
)
BEGIN
    SELECT * FROM BeatmapSet bs WHERE bs.Id = Id;
END //

CREATE PROCEDURE IF NOT EXISTS GetPlaycountForBeatmap(
    IN UserId INT,
    IN BeatmapId INT
)
BEGIN
    SELECT Count FROM BeatmapPlaycounts bpc
    WHERE bpc.UserId=UserId AND bpc.BeatmapId=BeatmapId;
END //

CREATE PROCEDURE IF NOT EXISTS GetBeatmapPlaycountForUser(
    IN UserId INT,
    IN BeatmapId INT
)
BEGIN
    SELECT BeatmapPlaycounts.*,Beatmaps.Id,Beatmaps.Version,Beatmaps.DifficultyRating,Beatmaps.BeatmapSetId,BeatmapSet.* 
    FROM BeatmapPlaycounts
    JOIN Beatmaps ON BeatmapPlaycounts.BeatmapId = Beatmaps.Id 
    JOIN BeatmapSet ON Beatmaps.BeatmapSetId = BeatmapSet.Id 
    WHERE BeatmapPlaycounts.UserId = UserId AND BeatmapPlaycounts.BeatmapId = BeatmapId;    
END //

CREATE PROCEDURE IF NOT EXISTS UpdateReferencedBeatmapIdForChannel(
    IN ChannelId BIGINT UNSIGNED,
    IN LastReferencedBeatmapId INT
)
BEGIN
    INSERT INTO LastReferencedBeatmapIdForChannel VALUES (ChannelId, BeatmapId)
    ON DUPLICATE KEY UPDATE BeatmapId=LastReferencedBeatmapId;
END //

CREATE PROCEDURE IF NOT EXISTS GetReferencedBeatmapIdForChannel(
    IN ChannelId BIGINT UNSIGNED
)
BEGIN
    SELECT BeatmapId
    FROM LastReferencedBeatmapIdForChannel l
    WHERE l.ChannelId=ChannelId;
END //


-- User related

CREATE PROCEDURE IF NOT EXISTS AddDiscordPlayerMapping(
    IN DiscordUserId BIGINT UNSIGNED,
    IN UpdatedPlayerUserId INT    
)
BEGIN
    INSERT INTO DiscordPlayer VALUES (DiscordUserId, UpdatedPlayerUserId)
    ON DUPLICATE KEY UPDATE PlayerUserId = UpdatedPlayerUserId;
END //    

CREATE PROCEDURE IF NOT EXISTS GetPlayerInfoFromDiscordId(
    IN DiscordUserId BIGINT UNSIGNED
)
BEGIN
    SELECT DiscordPlayer.*,Users.Username 
    FROM DiscordPlayer 
    JOIN Users ON Users.Id = DiscordPlayer.PlayerUserId 
    WHERE DiscordPlayer.DiscordUserId = DiscordUserId;
END //

CREATE PROCEDURE IF NOT EXISTS AddUser(
    IN Id INT,
    IN Username VARCHAR(255),
    IN CountryCode VARCHAR(255),
    IN AvatarUrl VARCHAR(255),
    IN ProfileColour VARCHAR(255)
)
BEGIN
    INSERT IGNORE INTO Users
    VALUES (Id, Username, CountryCode, AvatarUrl, ProfileColour);
END //

CREATE PROCEDURE IF NOT EXISTS GetUserById(
    IN Id INT    
)
BEGIN
    SELECT * FROM Users u WHERE u.Id = Id;
END //

CREATE PROCEDURE IF NOT EXISTS GetUserByUsername(
    IN Username VARCHAR(255)
)
BEGIN
    SELECT * FROM Users u WHERE u.Username = Username;
END //

CREATE PROCEDURE IF NOT EXISTS GetUserWithDiscordId(
    IN DiscordUserId BIGINT UNSIGNED
)
BEGIN
    SELECT Users.* FROM DiscordPlayer 
    JOIN Users ON DiscordPlayer.PlayerUserId = Users.Id 
    WHERE DiscordPlayer.DiscordUserId = DiscordUserId;
END //

DELIMITER ;
