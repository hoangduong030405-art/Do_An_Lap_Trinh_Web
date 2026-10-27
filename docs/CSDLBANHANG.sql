-- ==========================================================
-- CƠ SỞ DỮ LIỆU ĐỒ ÁN CAPSTONE ECO2415
-- ĐỀ TÀI 18: BÁN HÀNG & CÔNG NỢ KHÁCH HÀNG B2B (SALES & AR)
-- CHUẨN HÓA: MỘT NGUỒN CHÂN LÝ DUY NHẤT VỀ TUỔI NỢ & FIFO
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

-- 1. XÓA BẢNG CŨ NẾU ĐÃ TỒN TẠI (THEO THỨ TỰ PHỤ THUỘC KHÓA NGOẠI)
IF OBJECT_ID('dbo.PaymentAllocations', 'U') IS NOT NULL DROP TABLE dbo.PaymentAllocations;
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.InvoiceLines', 'U') IS NOT NULL DROP TABLE dbo.InvoiceLines;
IF OBJECT_ID('dbo.Invoices', 'U') IS NOT NULL DROP TABLE dbo.Invoices;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
GO

-- 2. TẠO CÁC BẢNG THEO CHUẨN THIẾT KẾ CSDL & RÀNG BUỘC TOÀN VẸN
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

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Unit NVARCHAR(20) NOT NULL DEFAULT N'Cái',
    UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
    CostPrice DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (CostPrice >= 0),
    StockQuantity INT NOT NULL DEFAULT 0 CHECK (StockQuantity >= 0),
    ImageUrl NVARCHAR(500) NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

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

CREATE TABLE InvoiceLines (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(Id) ON DELETE CASCADE,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id),
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
    CostPrice DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (CostPrice >= 0),
    LineTotal AS (Quantity * UnitPrice)
);

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

CREATE TABLE PaymentAllocations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PaymentId INT NOT NULL FOREIGN KEY REFERENCES Payments(Id) ON DELETE CASCADE,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(Id),
    AllocatedAmount DECIMAL(18,2) NOT NULL CHECK (AllocatedAmount > 0),
    AllocationDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 3. SEED DỮ LIỆU DANH MỤC KHÁCH HÀNG (4 ĐẠI LÝ TRỌNG ĐIỂM)
INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Address, CreditLimit, PaymentTermsDays)
VALUES 
(N'KH001', N'Tạp Hóa Bà Ba - Chợ Đông Ba', N'0905123456', N'12 Trần Hưng Đạo, TP. Huế', 25000000, 30),
(N'KH002', N'Siêu Thị Mini Tuấn Hương', N'0914987654', N'145 Đinh Tiên Hoàng, TP. Huế', 50000000, 30),
(N'KH003', N'Cửa Hàng Tiện Lợi An Phú', N'0983222333', N'88 Hùng Vương, TP. Huế', 15000000, 15),
(N'KH004', N'Đại Lý Phân Phối Nam Hà', N'0977888999', N'55 Lê Lợi, TP. Huế', 80000000, 45);

-- 4. SEED DỮ LIỆU DANH MỤC HÀNG HÓA (12 SẢN PHẨM PHÂN PHỐI SỈ B2B)
INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
VALUES
(N'SP001', N'Nước ngọt Coca-Cola Sleek (Thùng 24 lon 320ml)', N'Thùng', 210000, 185000, 150, '/images/products/sp001-coca-cola.svg', N'Hàng chính hãng Coca-Cola Việt Nam'),
(N'SP002', N'Bia Heineken Silver (Thùng 24 lon 330ml)', N'Thùng', 435000, 390000, 100, '/images/products/sp002-heineken.svg', N'Bia cao cấp Heineken Silver lon cao'),
(N'SP003', N'Mì Hảo Hảo Tôm Chua Cay (Thùng 30 gói)', N'Thùng', 125000, 108000, 200, '/images/products/sp003-hao-hao.svg', N'Mì ăn liền Acecook quốc dân'),
(N'SP004', N'Dầu ăn Simply Đậu Nành (Chai 1 lít)', N'Chai', 65000, 52000, 80, '/images/products/sp004-simply.svg', N'Dầu thực vật dinh dưỡng Simply 1L'),
(N'SP005', N'Sữa tươi Vinamilk có đường (Lốc 4 hộp 180ml)', N'Lốc', 35000, 28000, 300, '/images/products/sp005-vinamilk.svg', N'Sữa tươi tiệt trùng Vinamilk 100%'),
(N'SP006', N'Bia Tiger Crystal (Thùng 24 lon 330ml)', N'Thùng', 410000, 370000, 120, '/images/products/sp006-tiger-crystal.svg', N'Bia Tiger Crystal lên men lạnh sảng khoái'),
(N'SP007', N'Nước tinh khiết Aquafina (Thùng 24 chai 500ml)', N'Thùng', 95000, 78000, 180, '/images/products/sp007-aquafina.svg', N'Nước uống tinh khiết Aquafina đóng thùng 24 chai'),
(N'SP008', N'Mì Omachi Xốt Bò Hầm (Thùng 30 gói)', N'Thùng', 230000, 195000, 95, '/images/products/sp008-omachi.svg', N'Mì khoai tây Omachi xốt bò hầm cao cấp'),
(N'SP009', N'Nước tăng lực Red Bull Thái Lan (Thùng 24 lon 250ml)', N'Thùng', 265000, 230000, 150, '/images/products/sp009-red-bull.svg', N'Nước tăng lực lon vàng Red Bull chính hãng'),
(N'SP010', N'Cà phê hòa tan G7 3in1 Trung Nguyên (Thùng 24 hộp x 18 gói)', N'Thùng', 360000, 310000, 120, '/images/products/sp010-g7-coffee.svg', N'Cà phê hòa tan Trung Nguyên đậm đà'),
(N'SP011', N'Bánh ChocoPie Orion Truyền Thống (Thùng 8 hộp x 12 cái)', N'Thùng', 230000, 195000, 160, '/images/products/sp011-chocopie.svg', N'Bánh mềm phủ socola nhân kem dẻo marshmallow'),
(N'SP012', N'Nước mắm Nam Ngư Đệ Nhị (Thùng 15 chai 900ml)', N'Thùng', 175000, 145000, 200, '/images/products/sp012-nam-ngu.svg', N'Nước mắm Nam Ngư hương vị cá cơm thơm ngon'),
(N'SP013', N'Nước ngọt Pepsi lon cao (Thùng 24 lon 320ml)', N'Thùng', 205000, 180000, 160, '/images/products/sp013-pepsi.svg', N'Thùng 24 lon 320ml, vị nguyên bản sảng khoái'),
(N'SP014', N'Bia Saigon Special (Thùng 24 lon 330ml)', N'Thùng', 355000, 315000, 110, '/images/products/sp014-saigon-special.svg', N'Bia lon rồng vàng 100% lúa mạch nguyên chất'),
(N'SP015', N'Trà Xanh C2 Hương Chanh (Thùng 24 chai 455ml)', N'Thùng', 170000, 142000, 140, '/images/products/sp015-c2-lemon.svg', N'Trà xanh đóng chai 455ml thanh nhiệt giải độc'),
(N'SP016', N'Hạt nêm Knorr Thịt Thăn Xương Ống (Gói 900g)', N'Gói', 78000, 64000, 125, '/images/products/sp016-knorr.svg', N'Hạt nêm đậm đà vị ngọt từ thịt thăn và tủy xương'),
(N'SP017', N'Nước tương Chinsu Tỏi Ớt (Chai 330ml)', N'Chai', 22000, 17500, 210, '/images/products/sp017-chinsu.svg', N'Nước tương đậu nành lên men tự nhiên cay nồng tỏi ớt'),
(N'SP018', N'Bánh quy Cosy Marie Kinh Đô (Hộp 336g)', N'Hộp', 45000, 36000, 175, '/images/products/sp018-cosy.svg', N'Bánh quy sữa giòn xốp dinh dưỡng phù hợp mọi lứa tuổi'),
(N'SP019', N'Nước khoáng thiên nhiên La Vie (Thùng 24 chai 500ml)', N'Thùng', 105000, 85000, 190, '/images/products/sp019-lavie.svg', N'Nước khoáng tự nhiên đóng chai 500ml tinh khiết từ thiên nhiên'),
(N'SP020', N'Nước tăng lực Monster Energy Green (Thùng 24 lon 355ml)', N'Thùng', 580000, 510000, 85, '/images/products/sp020-monster.svg', N'Nước tăng lực nhập khẩu cao cấp thùng 24 lon chính hãng'),
(N'SP021', N'Bia Larue Export đậm đà (Thùng 24 lon 330ml)', N'Thùng', 260000, 230000, 140, '/images/products/sp021-larue.svg', N'Bia con cọp Larue thơm nồng đậm đà truyền thống miền Trung'),
(N'SP022', N'Mì Kokomi Đại 90g Tôm Chua Cay (Thùng 30 gói)', N'Thùng', 135000, 115000, 175, '/images/products/sp022-kokomi.svg', N'Mì gói to 90g sợi dai giòn thơm ngon đậm vị tôm chua cay'),
(N'SP023', N'Dầu hào Maggi Đậm Đặc nguyên chất (Chai 820g)', N'Chai', 42000, 33000, 160, '/images/products/sp023-maggi.svg', N'Dầu hào tự nhiên tạo màu nâu cánh gián và hương vị đậm đà món xào'),
(N'SP024', N'Tương ớt Chinsu đượm vị cay bùng nổ (Chai 250g)', N'Chai', 16500, 12800, 280, '/images/products/sp024-tuong-ot-chinsu.svg', N'Tương ớt đỏ tươi lên men cay nồng chấm mọi món ăn'),
(N'SP025', N'Sữa đặc có đường Ông Thọ nhãn đỏ (Lon 380g)', N'Lon', 26000, 21500, 250, '/images/products/sp025-ong-tho.svg', N'Sữa đặc Vinamilk béo ngọt sánh mịn chuyên pha cà phê và làm bánh'),
(N'SP026', N'Trà Ô Long TEA+ Plus Suntory (Thùng 24 chai 450ml)', N'Thùng', 215000, 185000, 130, '/images/products/sp026-oolong-tea.svg', N'Trà ô long chứa OTPP hỗ trợ hạn chế hấp thu chất béo thanh mát'),
(N'SP027', N'Bánh gạo giòn One One Bò Nướng Ngô (Gói 150g)', N'Gói', 28000, 22000, 220, '/images/products/sp027-one-one.svg', N'Bánh gạo nướng không chiên qua dầu giòn tan thơm lừng vị bò nướng ngô'),
(N'SP028', N'Nước xả vải Comfort Đậm Đặc Hương Ban Mai (Túi 3.8 lít)', N'Túi', 235000, 198000, 95, '/images/products/sp028-comfort.svg', N'Nước xả vải gia đình túi lớn 3.8L lưu hương thơm ngát suốt ngày dài');

-- 5. SEED DỮ LIỆU HÓA ĐƠN BÁN HÀNG VỚI NGÀY TƯƠNG ĐỐI (DATEADD TỪ GETDATE())
-- Đầy đủ tất cả các ca kiểm thử tuổi nợ & thanh toán:
-- 1. Đã trả đủ (Balance = 0, Status = 'Paid') -> HD-2026-001
-- 2. Trả một phần (PartiallyPaid) VÀ Quá hạn 15 ngày (Xô 1-30) -> HD-2026-002
-- 3. Quá hạn 45 ngày (Xô 31-60) -> HD-2026-003
-- 4. Quá hạn 70 ngày (Xô 61-90) -> HD-2026-004
-- 5. Quá hạn 100 ngày (Xô >90 - Nợ xấu) -> HD-2026-005
-- 6. Chưa đến hạn (DueDate tương lai +25 ngày) -> HD-2026-006
-- 7. Chưa đến hạn trong kỳ này (DueDate tương lai +28 ngày) -> HD-2026-007
INSERT INTO Invoices (InvoiceNumber, CustomerId, InvoiceDate, DueDate, TotalAmount, PaidAmount, Balance, Status)
VALUES
(N'HD-2026-001', 1, DATEADD(DAY, -70, GETDATE()), DATEADD(DAY, -40, GETDATE()), 3000000,  3000000,        0, 'Paid'),
(N'HD-2026-002', 1, DATEADD(DAY, -45, GETDATE()), DATEADD(DAY, -15, GETDATE()), 5000000,  2000000,  3000000, 'PartiallyPaid'),
(N'HD-2026-003', 2, DATEADD(DAY, -75, GETDATE()), DATEADD(DAY, -45, GETDATE()), 6000000,        0,  6000000, 'Pending'),
(N'HD-2026-004', 2, DATEADD(DAY, -100, GETDATE()), DATEADD(DAY, -70, GETDATE()), 2000000,       0,  2000000, 'Pending'),
(N'HD-2026-005', 3, DATEADD(DAY, -115, GETDATE()), DATEADD(DAY, -100, GETDATE()), 4500000,      0,  4500000, 'Pending'),
(N'HD-2026-006', 4, DATEADD(DAY, -5, GETDATE()),  DATEADD(DAY, 25, GETDATE()), 10000000,       0, 10000000, 'Pending'),
(N'HD-2026-007', 1, DATEADD(DAY, -2, GETDATE()),  DATEADD(DAY, 28, GETDATE()),  4200000,       0,  4200000, 'Pending');

-- 6. SEED CHI TIẾT HÓA ĐƠN (KHỚP CHÍNH XÁC TOTALAMOUNT & GIÁ VỐN THỰC TẾ)
INSERT INTO InvoiceLines (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice)
VALUES
-- HD-2026-001: Total = 3,000,000 | Cost = 2,586,000
(1, 1, 10, 210000, 185000), -- 2,100,000
(1, 7,  8,  95000,  78000), --   760,000
(1, 5,  4,  35000,  28000), --   140,000

-- HD-2026-002: Total = 5,000,000 | Cost = 4,420,000
(2, 2, 10, 435000, 390000), -- 4,350,000
(2, 4, 10,  65000,  52000), --   650,000

-- HD-2026-003: Total = 6,000,000 | Cost = 5,220,000
(3, 4, 20,  65000,  52000), -- 1,300,000
(3, 2, 10, 435000, 390000), -- 4,350,000
(3, 5, 10,  35000,  28000), --   350,000

-- HD-2026-004: Total = 2,000,000 | Cost = 1,705,000
(4, 1,  5, 210000, 185000), -- 1,050,000
(4, 7, 10,  95000,  78000), --   950,000

-- HD-2026-005: Total = 4,500,000 | Cost = 3,850,000
(5, 8, 15, 230000, 195000), -- 3,450,000
(5, 1,  5, 210000, 185000), -- 1,050,000

-- HD-2026-006: Total = 10,000,000 | Cost = 8,840,000
(6, 2, 20, 435000, 390000), -- 8,700,000
(6, 4, 20,  65000,  52000), -- 1,300,000

-- HD-2026-007: Total = 4,200,000 | Cost = 3,700,000
(7, 1, 20, 210000, 185000); -- 4,200,000

-- 7. SEED PHIẾU THU TIỀN VÀ PHÂN BỔ CÔNG NỢ THEO NGUYÊN TẮC FIFO
-- Khách hàng KH001 có 2 hóa đơn phát sinh theo thứ tự: HD-2026-001 (cũ hơn), HD-2026-002 (mới hơn).
-- Phiếu thu 1: 3,000,000 đ thanh toán trọn vẹn HD-2026-001.
-- Phiếu thu 2: 2,000,000 đ cấn trừ tiếp theo FIFO vào HD-2026-002 (còn dư nợ 3,000,000 đ).
INSERT INTO Payments (PaymentNumber, CustomerId, PaymentDate, Amount, UnallocatedAmount, Note)
VALUES
(N'PT-2026-001', 1, DATEADD(DAY, -50, GETDATE()), 3000000, 0, N'Tạp hóa Bà Ba thanh toán tiền mặt đợt 1 cấn trừ FIFO cho HĐ cũ nhất'),
(N'PT-2026-002', 1, DATEADD(DAY, -20, GETDATE()), 2000000, 0, N'Tạp hóa Bà Ba chuyển khoản VietQR đợt 2 cấn trừ FIFO cho HĐ tiếp theo');

INSERT INTO PaymentAllocations (PaymentId, InvoiceId, AllocatedAmount, AllocationDate)
VALUES
(1, 1, 3000000, DATEADD(DAY, -50, GETDATE())),
(2, 2, 2000000, DATEADD(DAY, -20, GETDATE()));
GO
