CREATE TABLE [dbo].[Stations]
(
	[StationId] INT NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(50) NOT NULL, 
    [Location] NVARCHAR(512) NOT NULL, 
    [FareZone] NVARCHAR(50) NOT NULL, 
    [MileageToPenn] DECIMAL(18, 2) NULL, 
    [PicFilename] NVARCHAR(50) NOT NULL
)
