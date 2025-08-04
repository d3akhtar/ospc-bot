CREATE DATABASE IF NOT EXISTS ospc;
USE ospc;

CREATE TABLE IF NOT EXISTS Users (
    Id                  INT NOT NULL,
    Username            VARCHAR(255) NOT NULL,
    CountryCode         VARCHAR(255) NOT NULL,
    AvatarUrl           VARCHAR(255),
    ProfileColour       VARCHAR(255),

    PRIMARY KEY (Id)
);

CREATE TABLE IF NOT EXISTS DiscordPlayer (
    DiscordUserId       BIGINT UNSIGNED NOT NULL,
    PlayerUserId        INT NOT NULL,

    PRIMARY KEY (DiscordUserId),
    FOREIGN KEY (PlayerUserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS BeatmapSet (
    Id                  INT NOT NULL,
    Artist              VARCHAR(255),
    Title               VARCHAR(255),
    SlimCover2x         VARCHAR(255),
    UserId              INT NOT NULL,

    PRIMARY KEY (Id)
);

CREATE TABLE IF NOT EXISTS Beatmaps (
    Id                  INT NOT NULL,
    Version             VARCHAR(255),
    DifficultyRating    FLOAT,
    BeatmapSetId        INT NOT NULL,
    CircleSize          FLOAT,
    BPM                 FLOAT,
    Length              FLOAT,
    HpDrain             FLOAT,
    OD                  FLOAT,
    AR                  FLOAT,

    PRIMARY KEY (Id),
    FOREIGN KEY (BeatmapSetId) REFERENCES BeatmapSet(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS BeatmapPlaycounts (
    UserId              INT NOT NULL,
    BeatmapId           INT NOT NULL,
    Count               INT NOT NULL,

    PRIMARY KEY (UserId,BeatmapId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (BeatmapId) REFERENCES Beatmaps(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS LastReferencedBeatmapIdForChannel (
    ChannelId           BIGINT UNSIGNED NOT NULL,
    BeatmapId           INT NOT NULL,
    PRIMARY KEY (ChannelId)
)


