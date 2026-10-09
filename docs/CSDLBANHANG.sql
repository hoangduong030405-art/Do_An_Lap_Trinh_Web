-- ==========================================================
-- CƠ SỞ DỮ LIỆU ĐỒ ÁN CAPSTONE ECO2415
-- ĐỀ TÀI 18: BÁN HÀNG & CÔNG NỢ KHÁCH HÀNG (SALES & AR)
-- ==========================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CSDLBANHANG')
BEGIN
    CREATE DATABASE CSDLBANHANG;
END
GO

USE CSDLBANHANG;
GO

-- 1. BẢNG KHÁCH HÀNG (Customers)
IF OBJECT_ID('dbo.PaymentAllocations', 'U') IS NOT NULL DROP TABLE dbo.PaymentAllocations;
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.InvoiceLines', 'U') IS NOT NULL DROP TABLE dbo.InvoiceLines;
IF OBJECT_ID('dbo.Invoices', 'U') IS NOT NULL DROP TABLE dbo.Invoices;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
GO

CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerCode NVARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(150) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    CreditLimit DECIMAL(18,2) NOT NULL DEFAULT 10000000 CHECK (CreditLimit >= 0),
    PaymentTermsDays INT NOT NULL DEFAULT 30 CHECK (PaymentTermsDays > 0),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 2. BẢNG HÀNG HÓA (Products)
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Unit NVARCHAR(20) NOT NULL DEFAULT N'Cái',
    UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
    StockQuantity INT NOT NULL DEFAULT 0 CHECK (StockQuantity >= 0),
    ImageUrl NVARCHAR(500) NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 3. BẢNG HÓA ĐƠN (Invoices)
CREATE TABLE Invoices (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber NVARCHAR(30) NOT NULL UNIQUE,
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(Id),
    InvoiceDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    DueDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL CHECK (TotalAmount >= 0),
    PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (PaidAmount >= 0),
    Balance DECIMAL(18,2) NOT NULL CHECK (Balance >= 0),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 4. BẢNG CHI TIẾT HÓA ĐƠN (InvoiceLines)
CREATE TABLE InvoiceLines (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(Id) ON DELETE CASCADE,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id),
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
    LineTotal AS (Quantity * UnitPrice)
);

-- 5. BẢNG PHIẾU THU TIỀN (Payments)
CREATE TABLE Payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PaymentNumber NVARCHAR(30) NOT NULL UNIQUE,
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(Id),
    PaymentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Amount DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
    UnallocatedAmount DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (UnallocatedAmount >= 0),
    Note NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 6. BẢNG PHÂN BỔ THU TIỀN (PaymentAllocations - FIFO)
CREATE TABLE PaymentAllocations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PaymentId INT NOT NULL FOREIGN KEY REFERENCES Payments(Id) ON DELETE CASCADE,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(Id),
    AllocatedAmount DECIMAL(18,2) NOT NULL CHECK (AllocatedAmount > 0),
    AllocationDate DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- DỮ LIỆU KHỞI TẠO (SEED DATA)
INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Address, CreditLimit, PaymentTermsDays)
VALUES 
(N'KH001', N'Tạp Hóa Bà Ba - Chợ Đông Ba', N'0905123456', N'12 Trần Hưng Đạo, TP. Huế', 20000000, 30),
(N'KH002', N'Siêu Thị Mini Tuấn Hương', N'0914987654', N'145 Đinh Tiên Hoàng, TP. Huế', 50000000, 30),
(N'KH003', N'Cửa Hàng Tiện Lợi An Phú', N'0983222333', N'88 Hùng Vương, TP. Huế', 15000000, 15),
(N'KH004', N'Đại Lý Phân Phối Nam Hà', N'0977888999', N'55 Lê Lợi, TP. Huế', 80000000, 45);

INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, StockQuantity, Description)
VALUES
(N'SP001', N'Nước ngọt Coca-Cola Sleek (Thùng 24 lon 320ml)', N'Thùng', 210000, 150, N'Hàng chính hãng Coca-Cola Việt Nam'),
(N'SP002', N'Bia Heineken Silver (Thùng 24 lon 330ml)', N'Thùng', 435000, 100, N'Bia cao cấp Heineken'),
(N'SP003', N'Mì Hảo Hảo Tôm Chua Cay (Thùng 30 gói)', N'Thùng', 125000, 200, N'Mì ăn liền Acecook'),
(N'SP004', N'Dầu ăn Simply Đậu Nành (Chai 1 lít)', N'Chai', 65000, 80, N'Dầu thực vật dinh dưỡng'),
(N'SP005', N'Sữa tươi Vinamilk có đường (Lốc 4 hộp 180ml)', N'Lốc', 35000, 300, N'Sữa tiệt trùng Vinamilk 100%');
GO
