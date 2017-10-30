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
	[Accepted] [bit] NOT NULL,
) ON [PRIMARY]
GO


DECLARE @schema varchar(30);  
DECLARE @tablePrefix varchar(30);  

/* TableNameVariable */

SET  @schema = 'dbo';
SET  @tablePrefix = '';
declare @tableName nvarchar(max) = '[' + @schema + '].[' + @tablePrefix + N'ProcessOrderSaga]';

/* Initialize */

/* CreateTable */

if not exists
(
    select *
    from sys.objects
    where
        object_id = object_id(@tableName) and
        type in ('U')
)
begin
declare @createTable nvarchar(max);
set @createTable = '
    create table ' + @tableName + '(
        Id uniqueidentifier not null primary key,
        Metadata nvarchar(max) not null,
        Data nvarchar(max) not null,
        PersistenceVersion varchar(23) not null,
        SagaTypeVersion varchar(23) not null,
        Concurrency int not null
    )
';
exec(@createTable);
end

/* AddProperty OrderId */

if not exists
(
  select * from sys.columns
  where
    name = N'Correlation_OrderId' and
    object_id = object_id(@tableName)
)
begin
  declare @createColumn_OrderId nvarchar(max);
  set @createColumn_OrderId = '
  alter table ' + @tableName + N'
    add Correlation_OrderId uniqueidentifier;';
  exec(@createColumn_OrderId);
end

/* VerifyColumnType Guid */

declare @dataType_OrderId nvarchar(max);
set @dataType_OrderId = (
  select data_type
  from information_schema.columns
  where
    table_name = ' + @tableName + N' and
    column_name = 'Correlation_OrderId'
);
if (@dataType_OrderId <> 'uniqueidentifier')
  begin
    declare @error_OrderId nvarchar(max) = N'Incorrect data type for Correlation_OrderId. Expected uniqueidentifier got ' + @dataType_OrderId + '.';
    throw 50000, @error_OrderId, 0
  end

/* WriteCreateIndex OrderId */

if not exists
(
    select *
    from sys.indexes
    where
        name = N'Index_Correlation_OrderId' and
        object_id = object_id(@tableName)
)
begin
  declare @createIndex_OrderId nvarchar(max);
  set @createIndex_OrderId = N'
  create unique index Index_Correlation_OrderId
  on ' + @tableName + N'(Correlation_OrderId)
  where Correlation_OrderId is not null;';
  exec(@createIndex_OrderId);
end

/* PurgeObsoleteIndex */

declare @dropIndexQuery nvarchar(max);
select @dropIndexQuery =
(
    select 'drop index ' + name + ' on ' + @tableName + ';'
    from sysindexes
    where
        Id = object_id(@tableName) and
        Name is not null and
        Name like 'Index_Correlation_%' and
        Name <> N'Index_Correlation_OrderId'
);
exec sp_executesql @dropIndexQuery

/* PurgeObsoleteProperties */

declare @tableNameWithoutSchema nvarchar(max) = @tablePrefix + N'ProcessOrderSaga';

declare @dropPropertiesQuery nvarchar(max);
select @dropPropertiesQuery =
(
    select 'alter table ' + @tableName + ' drop column ' + column_name + ';'
    from information_schema.columns
    where
        table_name = @tableNameWithoutSchema and
        table_schema = @schema and
        column_name like 'Correlation_%' and
        column_name <> N'Correlation_OrderId'
);
exec sp_executesql @dropPropertiesQuery

/* CompleteSagaScript */
