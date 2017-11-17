CREATE TABLE [dbo].[tInventImages]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [InventItemId] INT NOT NULL, 
    [Url] NVARCHAR(255) NOT NULL, 
    CONSTRAINT [FK_tInventImages_To_tInventItems] FOREIGN KEY ([InventItemId]) REFERENCES [dbo].[tInventItems]([Id]) ON DELETE CASCADE ON UPDATE CASCADE
)

GO

CREATE UNIQUE INDEX [IX_tInventImages_InventItemId_Url] ON [dbo].[tInventImages] ([InventItemId], [Url])

GO

CREATE INDEX [IX_tInventImages_InventItemId] ON [dbo].[tInventImages] ([InventItemId])
