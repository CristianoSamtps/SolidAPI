-- Dados de exemplo obrigatórios: app1, channel1, dr1, sub1

INSERT INTO Applications (ResourceName, ResourceDesc, DtCreation)
VALUES ('app1', 'Aplicacao de exemplo', '2026-11-25T10:00:00');

INSERT INTO Channels (ApplicationId, ResourceName, ResourceDesc, DtCreation)
VALUES ((SELECT Id FROM Applications WHERE ResourceName = 'app1'),
        'channel1', 'Canal de exemplo', '2026-11-25T10:00:00');

INSERT INTO DataRecords (ChannelId, ResourceName, ContentType, Content, DtCreation, DtExpiration)
VALUES ((SELECT c.Id FROM Channels c JOIN Applications a ON a.Id = c.ApplicationId
         WHERE a.ResourceName = 'app1' AND c.ResourceName = 'channel1'),
        'dr1', 'application/json', '{"humidity": 45}', '2026-11-25T10:00:00', NULL);

INSERT INTO Subscriptions (ChannelId, ResourceName, Evt, Endpoint, DtCreation, DtExpiration)
VALUES ((SELECT c.Id FROM Channels c JOIN Applications a ON a.Id = c.ApplicationId
         WHERE a.ResourceName = 'app1' AND c.ResourceName = 'channel1'),
        'sub1', 1, 'mqtt://localhost:1883', '2026-11-25T10:00:00', NULL);