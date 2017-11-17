CREATE TABLE [dbo].[tInventOptions]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [InventItemId] INT NOT NULL, 
    [Name] NVARCHAR(150) NOT NULL, 
    [Value] NVARCHAR(255) NOT NULL,
	CONSTRAINT [FK_tInventImages_To_tInventOptions] FOREIGN KEY ([InventItemId]) REFERENCES [dbo].[tInventItems]([Id]) ON DELETE CASCADE ON UPDATE CASCADE
)

GO

CREATE INDEX [IX_tInventOptions_InventItemId] ON [dbo].[tInventOptions] ([InventItemId])

GO

CREATE UNIQUE INDEX [IX_tInventOptions_InventItemId_Name] ON [dbo].[tInventOptions] ([InventItemId], [Name])
