-- StarCorp Travel: dados de exemplo para explorar a API. Idempotente.
-- Os voos usam datas relativas ao momento da carga, entao continuam no futuro a cada execucao.

USE StarCorp;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Airlines)
    INSERT INTO dbo.Airlines (Name, IataCode)
    VALUES (N'LATAM', 'LA'), (N'GOL', 'G3'), (N'Azul', 'AD');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
    INSERT INTO dbo.Customers (Name, Document, Email, IsActive)
    VALUES
        (N'Ana Souza',       '11111111111', N'ana@exemplo.com',     1),
        (N'Bruno Lima',      '22222222222', N'bruno@exemplo.com',   1),
        (N'Cliente Inativo', '33333333333', N'inativo@exemplo.com', 0);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Flights)
BEGIN
    DECLARE @la INT = (SELECT Id FROM dbo.Airlines WHERE IataCode = 'LA');
    DECLARE @g3 INT = (SELECT Id FROM dbo.Airlines WHERE IataCode = 'G3');
    DECLARE @ad INT = (SELECT Id FROM dbo.Airlines WHERE IataCode = 'AD');

    INSERT INTO dbo.Flights (AirlineId, OriginCity, DestinationCity, DepartureUtc, ArrivalUtc, BasePrice)
    VALUES
        (@la, N'Sao Paulo',      N'Rio de Janeiro', DATEADD(DAY, 10, SYSUTCDATETIME()), DATEADD(HOUR, 1, DATEADD(DAY, 10, SYSUTCDATETIME())), 500.00),
        (@g3, N'Sao Paulo',      N'Salvador',       DATEADD(DAY, 15, SYSUTCDATETIME()), DATEADD(HOUR, 2, DATEADD(DAY, 15, SYSUTCDATETIME())), 800.00),
        (@ad, N'Salvador',       N'Brasilia',       DATEADD(DAY, 20, SYSUTCDATETIME()), DATEADD(HOUR, 2, DATEADD(DAY, 20, SYSUTCDATETIME())), 650.00),
        (@la, N'Rio de Janeiro', N'Recife',         DATEADD(DAY, 5,  SYSUTCDATETIME()), DATEADD(HOUR, 3, DATEADD(DAY, 5,  SYSUTCDATETIME())), 950.00);

    INSERT INTO dbo.FlightSeats (FlightId, FareClassId, SeatsAvailable)
    SELECT Id, 1, 50 FROM dbo.Flights;

    INSERT INTO dbo.FlightSeats (FlightId, FareClassId, SeatsAvailable)
    SELECT Id, 2, 10 FROM dbo.Flights;
END
GO
