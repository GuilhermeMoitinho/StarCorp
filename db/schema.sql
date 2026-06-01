
IF DB_ID('StarCorp') IS NULL
    CREATE DATABASE StarCorp;
GO

USE StarCorp;
GO

IF OBJECT_ID('dbo.Customers', 'U') IS NULL
CREATE TABLE dbo.Customers
(
    Id        INT IDENTITY(1,1) CONSTRAINT PK_Customers PRIMARY KEY,
    Name      NVARCHAR(160) NOT NULL,
    Document  VARCHAR(14)   NOT NULL,
    Email     NVARCHAR(160) NOT NULL,
    IsActive  BIT           NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT 1,
    CreatedAt DATETIME2(0)  NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Customers_Document UNIQUE (Document)
);
GO

IF OBJECT_ID('dbo.Airlines', 'U') IS NULL
CREATE TABLE dbo.Airlines
(
    Id       INT IDENTITY(1,1) CONSTRAINT PK_Airlines PRIMARY KEY,
    Name     NVARCHAR(120) NOT NULL,
    IataCode CHAR(2)       NOT NULL,
    CONSTRAINT UQ_Airlines_Iata UNIQUE (IataCode)
);
GO

IF OBJECT_ID('dbo.FareClasses', 'U') IS NULL
CREATE TABLE dbo.FareClasses
(
    Id              TINYINT      CONSTRAINT PK_FareClasses PRIMARY KEY,
    Name            VARCHAR(20)  NOT NULL,
    PriceMultiplier DECIMAL(4,2) NOT NULL,
    CONSTRAINT UQ_FareClasses_Name UNIQUE (Name)
);
GO

IF OBJECT_ID('dbo.Flights', 'U') IS NULL
CREATE TABLE dbo.Flights
(
    Id              INT IDENTITY(1,1) CONSTRAINT PK_Flights PRIMARY KEY,
    AirlineId       INT           NOT NULL CONSTRAINT FK_Flights_Airlines REFERENCES dbo.Airlines(Id),
    OriginCity      NVARCHAR(80)  NOT NULL,
    DestinationCity NVARCHAR(80)  NOT NULL,
    DepartureUtc    DATETIME2(0)  NOT NULL,
    ArrivalUtc      DATETIME2(0)  NOT NULL,
    BasePrice       DECIMAL(18,2) NOT NULL,
    CONSTRAINT CK_Flights_BasePrice CHECK (BasePrice >= 0),
    CONSTRAINT CK_Flights_Times CHECK (ArrivalUtc > DepartureUtc)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Flights_Search' AND object_id = OBJECT_ID('dbo.Flights'))
    CREATE INDEX IX_Flights_Search ON dbo.Flights (OriginCity, DestinationCity, DepartureUtc);
GO

IF OBJECT_ID('dbo.FlightSeats', 'U') IS NULL
CREATE TABLE dbo.FlightSeats
(
    FlightId       INT     NOT NULL CONSTRAINT FK_FlightSeats_Flights REFERENCES dbo.Flights(Id),
    FareClassId    TINYINT NOT NULL CONSTRAINT FK_FlightSeats_FareClasses REFERENCES dbo.FareClasses(Id),
    SeatsAvailable INT     NOT NULL,
    CONSTRAINT PK_FlightSeats PRIMARY KEY (FlightId, FareClassId),
    CONSTRAINT CK_FlightSeats_NonNegative CHECK (SeatsAvailable >= 0)
);
GO

IF OBJECT_ID('dbo.Bookings', 'U') IS NULL
CREATE TABLE dbo.Bookings
(
    Id             INT IDENTITY(1,1) CONSTRAINT PK_Bookings PRIMARY KEY,
    CustomerId     INT           NOT NULL CONSTRAINT FK_Bookings_Customers REFERENCES dbo.Customers(Id),
    FlightId       INT           NOT NULL CONSTRAINT FK_Bookings_Flights REFERENCES dbo.Flights(Id),
    FareClassId    TINYINT       NOT NULL CONSTRAINT FK_Bookings_FareClasses REFERENCES dbo.FareClasses(Id),
    Status         TINYINT       NOT NULL,
    PassengerCount INT           NOT NULL,
    FarePrice      DECIMAL(18,2) NOT NULL,
    Subtotal       DECIMAL(18,2) NOT NULL,
    Taxes          DECIMAL(18,2) NOT NULL,
    ServiceFee     DECIMAL(18,2) NOT NULL,
    AmountDue      DECIMAL(18,2) NOT NULL,
    CreatedAt      DATETIME2(0)  NOT NULL CONSTRAINT DF_Bookings_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Bookings_PassengerCount CHECK (PassengerCount > 0)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_Customer' AND object_id = OBJECT_ID('dbo.Bookings'))
    CREATE INDEX IX_Bookings_Customer ON dbo.Bookings (CustomerId);
GO

IF OBJECT_ID('dbo.BookingPassengers', 'U') IS NULL
CREATE TABLE dbo.BookingPassengers
(
    Id        INT IDENTITY(1,1) CONSTRAINT PK_BookingPassengers PRIMARY KEY,
    BookingId INT           NOT NULL CONSTRAINT FK_BookingPassengers_Bookings REFERENCES dbo.Bookings(Id),
    Name      NVARCHAR(160) NOT NULL,
    Document  VARCHAR(20)   NOT NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BookingPassengers_Booking' AND object_id = OBJECT_ID('dbo.BookingPassengers'))
    CREATE INDEX IX_BookingPassengers_Booking ON dbo.BookingPassengers (BookingId);
GO

IF OBJECT_ID('dbo.Payments', 'U') IS NULL
CREATE TABLE dbo.Payments
(
    Id         INT IDENTITY(1,1) CONSTRAINT PK_Payments PRIMARY KEY,
    BookingId  INT           NOT NULL CONSTRAINT FK_Payments_Bookings REFERENCES dbo.Bookings(Id),
    Method     TINYINT       NOT NULL,
    Adjustment DECIMAL(18,2) NOT NULL,
    AmountPaid DECIMAL(18,2) NOT NULL,
    PaidAt     DATETIME2(0)  NOT NULL CONSTRAINT DF_Payments_PaidAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Payments_Booking UNIQUE (BookingId)
);
GO

IF OBJECT_ID('dbo.Cancellations', 'U') IS NULL
CREATE TABLE dbo.Cancellations
(
    Id               INT IDENTITY(1,1) CONSTRAINT PK_Cancellations PRIMARY KEY,
    BookingId        INT           NOT NULL CONSTRAINT FK_Cancellations_Bookings REFERENCES dbo.Bookings(Id),
    RefundPercentage DECIMAL(5,2)  NOT NULL,
    RefundAmount     DECIMAL(18,2) NOT NULL,
    CancelledAt      DATETIME2(0)  NOT NULL CONSTRAINT DF_Cancellations_CancelledAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Cancellations_Booking UNIQUE (BookingId)
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.FareClasses)
    INSERT INTO dbo.FareClasses (Id, Name, PriceMultiplier)
    VALUES (1, 'Economica', 1.00), (2, 'Executiva', 2.50);
GO
