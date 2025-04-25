-- Insert test account categories into AccountCategory
INSERT INTO AccountCategory (ID, accountName, accountType)
VALUES 
('cat1', 'Cash', 'Asset'),
('cat2', 'Accounts Payable', 'Liability'),
('cat3', 'Revenue', 'Income');

-- Insert test master accounts into MasterAccount
INSERT INTO MasterAccount (ID, name, openingAmount, closingAmount, accountGroup)
VALUES 
('acc1', 'Cash Account', 1000.00, NULL, 'Cash'),
('acc2', 'Accounts Payable Account', 500.00, NULL, 'Cash'),
('acc3', 'Revenue Account', 2000.00, NULL, 'Loans');

-- Insert test transactions into FinanceTransaction
INSERT INTO FinanceTransaction (ID, TransactionDate, TransactionDescription, authorID)
VALUES 
('trans1', '2025-04-01', 'Initial Cash Deposit', 'user001'),
('trans2', '2025-04-02', 'Payment to Vendor', 'user001'),
('trans3', '2025-04-03', 'Revenue Earned', 'user001');

-- Insert test transaction lines into TransactionLine
INSERT INTO TransactionLine (ID, creditedAmount, debitAmount, comment, transactionID, firstMasterAccount, secondMasterAccount)
VALUES 
('line1', NULL, 1000.00, 'Cash Deposit', 'trans1', 'acc1', NULL),
('line2', 500.00, NULL, 'Payment to Vendor', 'trans2', 'acc1', 'acc2'),
('line3', NULL, 2000.00, 'Revenue Earned', 'trans3', 'acc3', 'acc1');
