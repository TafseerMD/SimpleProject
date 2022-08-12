
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 07/20/2022 13:50:02
-- Generated from EDMX file: C:\Users\tafse\OneDrive\Desktop\asp.net mvc project\SimpleProject\SimpleProject\Models\mvcDataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [mvc_db];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_documents_mvc_tb]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[documents] DROP CONSTRAINT [FK_documents_mvc_tb];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[documents]', 'U') IS NOT NULL
    DROP TABLE [dbo].[documents];
GO
IF OBJECT_ID(N'[dbo].[mvc_tb]', 'U') IS NOT NULL
    DROP TABLE [dbo].[mvc_tb];
GO
IF OBJECT_ID(N'[dbo].[sysdiagrams]', 'U') IS NOT NULL
    DROP TABLE [dbo].[sysdiagrams];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'mvc_tb'
CREATE TABLE [dbo].[mvc_tb] (
    [id] int IDENTITY(1,1) NOT NULL,
    [FirstName] varchar(50)  NULL,
    [LastName] varchar(50)  NULL,
    [Address] varchar(50)  NULL,
    [Email] varchar(50)  NULL,
    [MobNum] varchar(50)  NULL,
    [Title] varchar(max)  NULL
);
GO

-- Creating table 'documents'
CREATE TABLE [dbo].[documents] (
    [docs_id] int IDENTITY(1,1) NOT NULL,
    [docs_type] varchar(50)  NULL,
    [docs_path] varchar(max)  NULL,
    [id] int  NOT NULL
);
GO

-- Creating table 'sysdiagrams'
CREATE TABLE [dbo].[sysdiagrams] (
    [name] nvarchar(128)  NOT NULL,
    [principal_id] int  NOT NULL,
    [diagram_id] int IDENTITY(1,1) NOT NULL,
    [version] int  NULL,
    [definition] varbinary(max)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'mvc_tb'
ALTER TABLE [dbo].[mvc_tb]
ADD CONSTRAINT [PK_mvc_tb]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [docs_id], [docsId] in table 'documents'
ALTER TABLE [dbo].[documents]
ADD CONSTRAINT [PK_documents]
    PRIMARY KEY CLUSTERED ([docs_id], [docsId] ASC);
GO

-- Creating primary key on [diagram_id] in table 'sysdiagrams'
ALTER TABLE [dbo].[sysdiagrams]
ADD CONSTRAINT [PK_sysdiagrams]
    PRIMARY KEY CLUSTERED ([diagram_id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [id] in table 'documents'
ALTER TABLE [dbo].[documents]
ADD CONSTRAINT [FK_documents_mvc_tb]
    FOREIGN KEY ([id])
    REFERENCES [dbo].[mvc_tb]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_documents_mvc_tb'
CREATE INDEX [IX_FK_documents_mvc_tb]
ON [dbo].[documents]
    ([id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------