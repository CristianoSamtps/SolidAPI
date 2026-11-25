-- Esquema da BD do middleware SOLID
-- Decisões: nomes únicos dentro do pai; cascata ao apagar o pai;
-- datas em TEXT no formato yyyy-MM-ddTHH:mm:ss; DtExpiration NULL = não expira

CREATE TABLE IF NOT EXISTS Applications (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    ResourceName  TEXT NOT NULL UNIQUE,
    ResourceDesc  TEXT,
    DtCreation    TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Channels (
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    ApplicationId  INTEGER NOT NULL,
    ResourceName   TEXT NOT NULL,
    ResourceDesc   TEXT,
    DtCreation     TEXT NOT NULL,
    FOREIGN KEY (ApplicationId) REFERENCES Applications(Id) ON DELETE CASCADE,
    UNIQUE (ApplicationId, ResourceName)
);

CREATE TABLE IF NOT EXISTS DataRecords (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    ChannelId     INTEGER NOT NULL,
    ResourceName  TEXT NOT NULL CHECK (ResourceName <> 'subs'),
    ContentType   TEXT NOT NULL CHECK (ContentType IN ('application/json', 'application/xml', 'text/plain')),
    Content       TEXT NOT NULL,
    DtCreation    TEXT NOT NULL,
    DtExpiration  TEXT,
    FOREIGN KEY (ChannelId) REFERENCES Channels(Id) ON DELETE CASCADE,
    UNIQUE (ChannelId, ResourceName)
);

CREATE TABLE IF NOT EXISTS Subscriptions (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    ChannelId     INTEGER NOT NULL,
    ResourceName  TEXT NOT NULL,
    Evt           INTEGER NOT NULL CHECK (Evt IN (1, 2)),
    Endpoint      TEXT NOT NULL,
    DtCreation    TEXT NOT NULL,
    DtExpiration  TEXT,
    FOREIGN KEY (ChannelId) REFERENCES Channels(Id) ON DELETE CASCADE,
    UNIQUE (ChannelId, ResourceName)
);

CREATE INDEX IF NOT EXISTS IX_DataRecords_DtExpiration   ON DataRecords(DtExpiration);
CREATE INDEX IF NOT EXISTS IX_Subscriptions_DtExpiration ON Subscriptions(DtExpiration);