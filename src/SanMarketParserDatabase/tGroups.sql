CREATE TABLE [dbo].[tGroups]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ParentId] INT NOT NULL DEFAULT -1, 
    [Name] NVARCHAR(150) NOT NULL, 
    [Url] NVARCHAR(255) NOT NULL
)

GO

CREATE UNIQUE INDEX [IX_tGroups_Url] ON [dbo].[tGroups] ([Url])
