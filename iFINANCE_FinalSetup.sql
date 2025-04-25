-- iFINANCE Final Setup Script
-- Run this ONCE on a fresh server or machine
-- Creates database, all tables, and inserts default admin

-- 1. Create DB if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Group13_iFINANCEDB')
BEGIN
    CREATE DATABASE Group13_iFINANCEDB;
END
GO

USE Group13_iFINANCEDB;
GO

-- 2. Tables (only create if not already exists)

IF OBJECT_ID('iFINANCEUser', 'U') IS NULL
BEGIN
    CREATE TABLE iFINANCEUser(
        ID VARCHAR(50) PRIMARY KEY,
        UsersName VARCHAR(100)
    );
END

IF OBJECT_ID('UserPassword', 'U') IS NULL
BEGIN
    CREATE TABLE UserPassword(
        ID VARCHAR(50) PRIMARY KEY,
        userName VARCHAR(50),
        encryptedPassword VARCHAR(200),
        passwordExpiryTime INT,
        userAccountExpiryDate DATE,
        FOREIGN KEY (ID) REFERENCES iFINANCEUser (ID)
    );
END

IF OBJECT_ID('Administrator', 'U') IS NULL
BEGIN
    CREATE TABLE Administrator(
        ID VARCHAR(50) PRIMARY KEY,
        dateHired DATE,
        dateFinished DATE,
        FOREIGN KEY (ID) REFERENCES iFINANCEUser (ID)
    );
END

IF OBJECT_ID('NonAdminUser', 'U') IS NULL
BEGIN
    CREATE TABLE NonAdminUser(
        ID VARCHAR(50) PRIMARY KEY,
        StreetAddress VARCHAR(150),
        Email VARCHAR(150),
        FOREIGN KEY (ID) REFERENCES iFINANCEUser (ID)
    );
END

IF OBJECT_ID('AccountCategory', 'U') IS NULL
BEGIN
    CREATE TABLE AccountCategory(
        ID VARCHAR(50) PRIMARY KEY,
        accountName VARCHAR(50),
        accountType VARCHAR(50)
    );
END

IF OBJECT_ID('GroupTable', 'U') IS NULL
BEGIN
    CREATE TABLE GroupTable(
        ID VARCHAR(50) PRIMARY KEY,
        groupName VARCHAR(100),
        parent VARCHAR(50),
        element VARCHAR(50),
		userId VARCHAR (50),
        FOREIGN KEY (parent) REFERENCES GroupTable (ID),
        FOREIGN KEY (element) REFERENCES AccountCategory (ID),
        FOREIGN KEY (userId) REFERENCES NonAdminUser (ID)
    );
END

IF OBJECT_ID('MasterAccount', 'U') IS NULL
BEGIN
    CREATE TABLE MasterAccount(
        ID VARCHAR(50) PRIMARY KEY,
        name VARCHAR(100),
        openingAmount FLOAT,
        closingAmount FLOAT,
        accountGroup VARCHAR(50),
        FOREIGN KEY (accountGroup) REFERENCES GroupTable (ID)
    );
END

IF OBJECT_ID('FinanceTransaction', 'U') IS NULL
BEGIN
    CREATE TABLE FinanceTransaction(
        ID VARCHAR(50) PRIMARY KEY,
        TransactionDate DATE,
        TransactionDescription VARCHAR(250),
        authorID VARCHAR(50),
        FOREIGN KEY (authorID) REFERENCES NonAdminUser (ID)
    );
END

IF OBJECT_ID('TransactionLine', 'U') IS NULL
BEGIN
    CREATE TABLE TransactionLine(
        ID VARCHAR(50) PRIMARY KEY,
        creditedAmount FLOAT,
        debitAmount FLOAT,
        comment VARCHAR(250),
        transactionID VARCHAR(50),
        firstMasterAccount VARCHAR(50),
        secondMasterAccount VARCHAR(50),
        FOREIGN KEY (transactionID) REFERENCES FinanceTransaction(ID),
        FOREIGN KEY (firstMasterAccount) REFERENCES MasterAccount(ID),
        FOREIGN KEY (secondMasterAccount) REFERENCES MasterAccount(ID)
    );
END

-- 3. Insert permanent admin (if not already exists)

IF NOT EXISTS (SELECT * FROM iFINANCEUser WHERE ID = 'admin001')
BEGIN
    INSERT INTO iFINANCEUser (ID, UsersName)
    VALUES ('admin001', 'System Admin');

    INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
    VALUES (
        'admin001',
        'admin',
        'admin',  -- plaintext for now
        90,
        NULL
    );

    INSERT INTO Administrator (ID, dateHired, dateFinished)
    VALUES ('admin001', GETDATE(), NULL);
END

-- 4. Insert default category types (if not already exists)

IF NOT EXISTS (SELECT * FROM AccountCategory WHERE accountName = 'Assets')
BEGIN
	INSERT INTO AccountCategory (ID, accountName, accountType)
	VALUES 
		(NEWID(), 'Assets', 'Debit'),
		(NEWID(), 'Liabilities', 'Credit'),
		(NEWID(), 'Income', 'Credit'),
		(NEWID(), 'Expenses', 'Debit'),
		(NEWID(), 'Equity', 'Credit');
END