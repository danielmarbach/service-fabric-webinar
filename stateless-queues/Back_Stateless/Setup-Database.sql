USE [master]
GO

IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'[statelessqueues]')
CREATE DATABASE [statelessqueues]
GO

DECLARE @NetSer VARCHAR(100)
SET @NetSer  = (SELECT SUSER_SNAME(0x010100000000000514000000))

        DECLARE @cmd VARCHAR(200)
        SET @cmd = N'CREATE LOGIN [' + @NetSer + '] FROM windows with DEFAULT_DATABASE=statelessqueues'
        EXEC (@cmd)

use [statelessqueues];
exec sp_addrolemember 'db_owner', @NetSer;

USE [statelessqueues]
GO

DROP TABLE IF EXISTS [dbo].[Orders]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Orders](
	[OrderId] [uniqueidentifier] unique NOT NULL,
	[SubmittedOn] [datetime] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
) ON [PRIMARY]
GO