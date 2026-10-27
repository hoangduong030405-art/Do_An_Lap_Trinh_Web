USE CSDLBANHANG;
GO

SET NOCOUNT ON;

-- Chèn 6 sản phẩm phân phối sỉ mới nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP013')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP013', N'Nước ngọt Pepsi lon cao (Thùng 24 lon 320ml)', N'Thùng', 205000, 180000, 160, '/images/products/sp013-pepsi.svg', N'Thùng 24 lon 320ml, vị nguyên bản sảng khoái');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP014')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP014', N'Bia Saigon Special (Thùng 24 lon 330ml)', N'Thùng', 355000, 315000, 110, '/images/products/sp014-saigon-special.svg', N'Bia lon rồng vàng 100% lúa mạch nguyên chất');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP015')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP015', N'Trà Xanh C2 Hương Chanh (Thùng 24 chai 455ml)', N'Thùng', 170000, 142000, 140, '/images/products/sp015-c2-lemon.svg', N'Trà xanh đóng chai 455ml thanh nhiệt giải độc');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP016')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP016', N'Hạt nêm Knorr Thịt Thăn Xương Ống (Gói 900g)', N'Gói', 78000, 64000, 125, '/images/products/sp016-knorr.svg', N'Hạt nêm đậm đà vị ngọt từ thịt thăn và tủy xương');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP017')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP017', N'Nước tương Chinsu Tỏi Ớt (Chai 330ml)', N'Chai', 22000, 17500, 210, '/images/products/sp017-chinsu.svg', N'Nước tương đậu nành lên men tự nhiên cay nồng tỏi ớt');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP018')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP018', N'Bánh quy Cosy Marie Kinh Đô (Hộp 336g)', N'Hộp', 45000, 36000, 175, '/images/products/sp018-cosy.svg', N'Bánh quy sữa giòn xốp dinh dưỡng phù hợp mọi lứa tuổi');
END
GO

-- Xác nhận số lượng sản phẩm sau khi chèn
SELECT Id, ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl 
FROM Products 
ORDER BY Id ASC;
GO
