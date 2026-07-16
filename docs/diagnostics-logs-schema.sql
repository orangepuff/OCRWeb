-- ============================================================================
-- DiagnosticLogs — canonical schema (DBA-owned, shared across applications).
-- Reference copy for the Diagnostics.* logging library.
-- ============================================================================

IF DB_ID('DiagnosticLogs') IS NULL
BEGIN
    CREATE DATABASE DiagnosticLogs;
END
GO

USE DiagnosticLogs;
GO

------------------------------------------------------------------------------
-- Environments
------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Environments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Environments
    (
        iId int IDENTITY(1,1) NOT NULL,
        sName varchar(256) NOT NULL,
        sVersion varchar(30) NULL,
        sUrl varchar(2000) NULL,

        CONSTRAINT PK_Environments
            PRIMARY KEY CLUSTERED (iId)
    );
END
GO

------------------------------------------------------------------------------
-- Environments — seed data
------------------------------------------------------------------------------
GO
IF NOT EXISTS(SELECT 1 FROM Environments)
BEGIN
	INSERT INTO Environments(sName, sVersion, sUrl)
	VALUES('DEV', NULL, NULL)
END
GO

------------------------------------------------------------------------------
-- Configurations
------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Configurations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Configurations
    (
        iId int IDENTITY(1,1) NOT NULL,
        sLoggerName varchar(100) NOT NULL,
        iEnvironmentId int NULL,
        xValue xml NOT NULL,
        dtUpdatedTime datetime NULL,

        CONSTRAINT PK_Configurations
            PRIMARY KEY CLUSTERED (iId)
    );
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.foreign_keys
    WHERE name = 'FK_Configurations_Environment'
)
BEGIN
    ALTER TABLE dbo.Configurations
    ADD CONSTRAINT FK_Configurations_Environment
        FOREIGN KEY (iEnvironmentId)
        REFERENCES dbo.Environments(iId);
END
GO

------------------------------------------------------------------------------
-- Configurations — seed data
------------------------------------------------------------------------------
GO
IF NOT EXISTS(
	SELECT 1 
	FROM Configurations
)
BEGIN 
INSERT [dbo].[Configurations] ([sLoggerName], [iEnvironmentId], [xValue], [dtUpdatedTime]) 
VALUES (N'NLogLogger', NULL, N'<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.nlog-project.org/schemas/NLog.xsd NLog.xsd" autoReload="true" throwExceptions="false" internalLogLevel="Error" internalLogFile="c:\temp\nlog-internal.log"><targets async="true"><target name="Log" xsi:type="Database" connectionStringName="DiagnosticFramework" commandType="StoredProcedure" commandText="[dbo].[spAddLog]"><parameter name="@sTransactionId" layout="${event-properties:item=TransactionId}" /><parameter name="@iEnvironmentId" layout="${event-properties:item=EnvironmentId}" /><parameter name="@sCorrelationId" layout="${event-properties:item=CorrelationId}" /><parameter name="@iClientId" layout="${event-properties:item=ClientId}" /><parameter name="@iCategoryId" layout="${event-properties:item=CategoryId}" /><parameter name="@iServerId" layout="${event-properties:item=ServerId}" /><parameter name="@sSeverity" layout="${level}" /><parameter name="@dtTimeLogged" layout="${event-properties:item=TimeLogged}" /><parameter name="@sMessage" layout="${message}" /><parameter name="@sException" layout="${exception:format=ToString,StackTrace}" /><parameter name="@sUser" layout="${event-properties:item=User}" /><parameter name="@sModule" layout="${event-properties:item=Module}" /><parameter name="@sCustomAttributes" layout="${event-properties:item=CustomAttributes}" /></target><target name="Transaction" xsi:type="Database" connectionStringName="DiagnosticFramework" commandType="StoredProcedure" commandText="[dbo].[spAddTransaction]"><parameter name="@sId" layout="${event-properties:item=Id}" /><parameter name="@sParentId" layout="${event-properties:item=ParentId}" /><parameter name="@iEnvironmentId" layout="${event-properties:item=EnvironmentId}" /><parameter name="@sCorrelationId" layout="${event-properties:item=CorrelationId}" /><parameter name="@iClientId" layout="${event-properties:item=ClientId}" /><parameter name="@iCategoryId" layout="${event-properties:item=CategoryId}" /><parameter name="@iServerId" layout="${event-properties:item=ServerId}" /><parameter name="@sMessage" layout="${message}" /><parameter name="@sUrl" layout="${event-properties:item=Url}" /><parameter name="@dtStartTime" layout="${event-properties:item=StartTime}" /><parameter name="@iDuration" layout="${event-properties:item=Duration}" /><parameter name="@xRequestXml" layout="${event-properties:item=RequestXml}" /><parameter name="@sRequestJson" layout="${event-properties:item=RequestJson}" /><parameter name="@sRequestText" layout="${event-properties:item=RequestText}" /><parameter name="@xResponseXml" layout="${event-properties:item=ResponseXml}" /><parameter name="@sResponseJson" layout="${event-properties:item=ResponseJson}" /><parameter name="@sResponseText" layout="${event-properties:item=ResponseText}" /><parameter name="@sModule" layout="${event-properties:item=Module}" /><parameter name="@sUser" layout="${event-properties:item=User}" /><parameter name="@sCustomAttributes" layout="${event-properties:item=CustomAttributes}" /><parameter name="@sSql" layout="${event-properties:item=SQL}" /><parameter name="@sBaseUrl" layout="${event-properties:item=BaseUrl}" /></target></targets><rules><logger name="Log" minlevel="Info" writeTo="Log" /><logger name="Transaction" minlevel="Info" writeTo="Transaction" /><logger name="RootTransaction" minlevel="Trace" writeTo="Transaction" /><logger name="DatabaseTransaction" minlevel="Info" writeTo="Transaction" /><logger name="ExternalServiceTransaction" minlevel="Trace" writeTo="Transaction" /></rules></nlog>', CAST(N'2023-02-28T03:15:53.020' AS DateTime))
END
GO
------------------------------------------------------------------------------
-- Categories
------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        iId int IDENTITY(1,1) NOT NULL,
        sName varchar(256) NOT NULL,

        CONSTRAINT PK_Categories
            PRIMARY KEY CLUSTERED (iId)
    );
END
GO

------------------------------------------------------------------------------
-- Categories — seed data (one row per module under src/Backend and src/Frontend)
------------------------------------------------------------------------------
INSERT INTO dbo.Categories (sName)
SELECT v.sName
FROM (VALUES
    ('API'),
    ('Bff'),
    ('Document'),
    ('Document.Api'),
    ('Document.Contract'),
    ('Identity'),
    ('Identity.Contract'),
    ('OCR'),
    ('OCR.Contract'),
    ('Pdf'),
    ('Pdf.Contract'),
    ('ProjectManagement'),
    ('ProjectManagement.Contract'),
    ('Shared'),
    ('Frontend'),
    ('Frontend.Shared')
) AS v(sName)
WHERE NOT EXISTS
(
    SELECT *
    FROM dbo.Categories c
    WHERE c.sName = v.sName
);
GO

------------------------------------------------------------------------------
-- Transactions
------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Transactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions
    (
        sId uniqueidentifier NOT NULL,
        sParentId uniqueidentifier NULL,
        iEnvironmentId int NOT NULL,
        iCategoryId int NOT NULL,
        sCorrelationId uniqueidentifier NOT NULL,
        sMessage nvarchar(max) NULL,
        sUrl nvarchar(max) NULL,
        dtStartTime datetime NOT NULL,
        iDuration int NULL,
        xRequestXml nvarchar(max) NULL,
        sRequestJson nvarchar(max) NULL,
        sRequestText nvarchar(max) NULL,
        xResponseXml nvarchar(max) NULL,
        sResponseJson nvarchar(max) NULL,
        sResponseText nvarchar(max) NULL,
        sUser varchar(256) NULL,
        sCustomAttributes nvarchar(max) NULL,
        sSql nvarchar(max) NULL,
        sBaseUrl nvarchar(max) NULL,

        CONSTRAINT PK_Transactions
            PRIMARY KEY NONCLUSTERED (sId)
    );
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Transactions')
      AND name = 'CCI_Transactions'
)
BEGIN
    CREATE CLUSTERED COLUMNSTORE INDEX CCI_Transactions
        ON dbo.Transactions;
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.default_constraints
    WHERE name = 'DF_Transactions_sId'
)
BEGIN
    ALTER TABLE dbo.Transactions
        ADD CONSTRAINT DF_Transactions_sId
        DEFAULT NEWID() FOR sId;
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.default_constraints
    WHERE name = 'DF_Transactions_sCorrelationId'
)
BEGIN
    ALTER TABLE dbo.Transactions
        ADD CONSTRAINT DF_Transactions_sCorrelationId
        DEFAULT NEWID() FOR sCorrelationId;
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.default_constraints
    WHERE name = 'DF_Transactions_dtStartTime'
)
BEGIN
    ALTER TABLE dbo.Transactions
        ADD CONSTRAINT DF_Transactions_dtStartTime
        DEFAULT GETDATE() FOR dtStartTime;
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.foreign_keys
    WHERE name = 'FK_Transactions_Categories'
)
BEGIN
    ALTER TABLE dbo.Transactions
        ADD CONSTRAINT FK_Transactions_Categories
        FOREIGN KEY (iCategoryId)
        REFERENCES dbo.Categories(iId);
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.foreign_keys
    WHERE name = 'FK_Transactions_Environments'
)
BEGIN
    ALTER TABLE dbo.Transactions
        ADD CONSTRAINT FK_Transactions_Environments
        FOREIGN KEY (iEnvironmentId)
        REFERENCES dbo.Environments(iId);
END
GO

------------------------------------------------------------------------------
-- Logs
------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Logs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Logs
    (
        iId bigint IDENTITY(1,1) NOT NULL,
        sTransactionId uniqueidentifier NULL,
        iEnvironmentId int NOT NULL,
        iCategoryId int NOT NULL,
        sCorrelationId uniqueidentifier NOT NULL,
        dtTimeLogged datetime NOT NULL,
        sMessage nvarchar(max) NULL,
        sException nvarchar(max) NULL,
        sSeverity varchar(20) NOT NULL,
        sUser varchar(256) NULL,
        sCustomAttributes nvarchar(max) NULL,

        CONSTRAINT PK_Logs
            PRIMARY KEY NONCLUSTERED (iId)
    );
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Logs')
      AND name = 'CCI_Logs'
)
BEGIN
    CREATE CLUSTERED COLUMNSTORE INDEX CCI_Logs
        ON dbo.Logs;
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.default_constraints
    WHERE name = 'DF_Logs_sCorrelationId'
)
BEGIN
    ALTER TABLE dbo.Logs
        ADD CONSTRAINT DF_Logs_sCorrelationId
        DEFAULT NEWID() FOR sCorrelationId;
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.default_constraints
    WHERE name = 'DF_Logs_dtTimeLogged'
)
BEGIN
    ALTER TABLE dbo.Logs
        ADD CONSTRAINT DF_Logs_dtTimeLogged
        DEFAULT GETDATE() FOR dtTimeLogged;
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.foreign_keys
    WHERE name = 'FK_Logs_Categories'
)
BEGIN
    ALTER TABLE dbo.Logs
        ADD CONSTRAINT FK_Logs_Categories
        FOREIGN KEY (iCategoryId)
        REFERENCES dbo.Categories(iId);
END
GO

IF NOT EXISTS
(
    SELECT *
    FROM sys.foreign_keys
    WHERE name = 'FK_Logs_Environments'
)
BEGIN
    ALTER TABLE dbo.Logs
        ADD CONSTRAINT FK_Logs_Environments
        FOREIGN KEY (iEnvironmentId)
        REFERENCES dbo.Environments(iId);
END
GO