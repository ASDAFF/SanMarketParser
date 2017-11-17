CREATE TABLE [dbo].[tInventItems]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ParentId] INT NOT NULL DEFAULT -1, 
    [Url] NVARCHAR(255) NOT NULL, 
    [Name] NVARCHAR(255) NOT NULL, 
    [ShortDescr] TEXT NULL, 
    [Descr] TEXT NULL, 
    [OldPrice] NUMERIC(18, 2) NULL, 
    [Price] NUMERIC(18, 2) NOT NULL
)

GO

CREATE UNIQUE INDEX [IX_tInventItems_Url] ON [dbo].[tInventItems] ([Url])

GO

CREATE INDEX [IX_tInventItems_ParentId] ON [dbo].[tInventItems] ([ParentId])
