
INSERT INTO iFINANCEUser (ID, UsersName)
VALUES ('user001', 'Default user');

INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
VALUES (
    'user001',
    'user',
    'pass',  -- plaintext for now
    90,
    NULL
);

INSERT INTO NonAdminUser (ID, StreetAddress, Email)
VALUES ('user001', '123 Address St', 'email@email.com');