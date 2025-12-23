-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ChatWebSocketDB')
BEGIN
    CREATE DATABASE ChatWebSocketDB;
END
GO

USE ChatWebSocketDB;
GO

-- Create Roles table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(20) NOT NULL UNIQUE
    );
    
    -- Insert default roles
    INSERT INTO Roles (Id, Name) VALUES 
        (NEWID(), 'Customer'),
        (NEWID(), 'Admin');
END
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        RoleId UNIQUEIDENTIFIER NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        FullName NVARCHAR(100) NOT NULL,
        Password NVARCHAR(50) NOT NULL,
        PhoneNumber NVARCHAR(11) NULL,
        CreateAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (RoleId) REFERENCES Roles(Id)
    );
    
    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_RoleId ON Users(RoleId);
END
GO

-- Create ChatSessions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChatSessions' AND xtype='U')
BEGIN
    CREATE TABLE ChatSessions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        AdminId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Waiting',
        CreateAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CloseAt DATETIME2 NULL,
        LastActiveAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ConnectionState NVARCHAR(30) NULL,
        InactivityTimeout INT NOT NULL DEFAULT 1800,
        ChannelType NVARCHAR(20) NOT NULL DEFAULT 'Web',
        FOREIGN KEY (CustomerId) REFERENCES Users(Id),
        FOREIGN KEY (AdminId) REFERENCES Users(Id)
    );
    
    CREATE INDEX IX_ChatSessions_CustomerId ON ChatSessions(CustomerId);
    CREATE INDEX IX_ChatSessions_AdminId ON ChatSessions(AdminId);
    CREATE INDEX IX_ChatSessions_Status ON ChatSessions(Status);
    CREATE INDEX IX_ChatSessions_CreateAt ON ChatSessions(CreateAt);
END
GO

-- Create ChatMessages table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChatMessages' AND xtype='U')
BEGIN
    CREATE TABLE ChatMessages (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ChatSessionId UNIQUEIDENTIFIER NOT NULL,
        SenderId UNIQUEIDENTIFIER NOT NULL,
        Content NTEXT NOT NULL,
        SendAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        DeliveryStatus NVARCHAR(20) NOT NULL DEFAULT 'Sent',
        SourcePlatform NVARCHAR(20) NOT NULL DEFAULT 'Web',
        FOREIGN KEY (ChatSessionId) REFERENCES ChatSessions(Id) ON DELETE CASCADE,
        FOREIGN KEY (SenderId) REFERENCES Users(Id)
    );
    
    CREATE INDEX IX_ChatMessages_ChatSessionId ON ChatMessages(ChatSessionId);
    CREATE INDEX IX_ChatMessages_SenderId ON ChatMessages(SenderId);
    CREATE INDEX IX_ChatMessages_SendAt ON ChatMessages(SendAt);
END
GO

-- Insert sample admin user (password is hashed version of "admin123")
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'Admin');
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@example.com')
BEGIN
    INSERT INTO Users (Id, RoleId, Email, FullName, Password, CreateAt)
    VALUES (NEWID(), @AdminRoleId, 'admin@example.com', 'System Administrator', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', GETUTCDATE());
END
GO

PRINT 'Database initialization completed successfully!';
