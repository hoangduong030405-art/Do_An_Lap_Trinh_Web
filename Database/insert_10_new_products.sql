USE CSDLBANHANG;
GO

SET NOCOUNT ON;

-- Chèn 10 sản phẩm phân phối sỉ mới (SP019 -> SP028)
IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP019')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP019', N'Nước khoáng thiên nhiên La Vie (Thùng 24 chai 500ml)', N'Thùng', 105000, 85000, 190, '/images/products/sp019-lavie.svg', N'Nước khoáng tự nhiên đóng chai 500ml tinh khiết từ thiên nhiên');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP020')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP020', N'Nước tăng lực Monster Energy Green (Thùng 24 lon 355ml)', N'Thùng', 580000, 510000, 85, '/images/products/sp020-monster.svg', N'Nước tăng lực nhập khẩu cao cấp thùng 24 lon chính hãng');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP021')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP021', N'Bia Larue Export đậm đà (Thùng 24 lon 330ml)', N'Thùng', 260000, 230000, 140, '/images/products/sp021-larue.svg', N'Bia con cọp Larue thơm nồng đậm đà truyền thống miền Trung');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP022')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP022', N'Mì Kokomi Đại 90g Tôm Chua Cay (Thùng 30 gói)', N'Thùng', 135000, 115000, 175, '/images/products/sp022-kokomi.svg', N'Mì gói to 90g sợi dai giòn thơm ngon đậm vị tôm chua cay');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP023')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP023', N'Dầu hào Maggi Đậm Đặc nguyên chất (Chai 820g)', N'Chai', 42000, 33000, 160, '/images/products/sp023-maggi.svg', N'Dầu hào tự nhiên tạo màu nâu cánh gián và hương vị đậm đà món xào');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP024')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP024', N'Tương ớt Chinsu đậm vị cay bùng nổ (Chai 250g)', N'Chai', 16500, 12800, 280, '/images/products/sp024-tuong-ot-chinsu.svg', N'Tương ớt đỏ tươi lên men cay nồng chấm mọi món ăn');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP025')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP025', N'Sữa đặc có đường Ông Thọ nhãn đỏ (Lon 380g)', N'Lon', 26000, 21500, 250, '/images/products/sp025-ong-tho.svg', N'Sữa đặc Vinamilk béo ngọt sánh mịn chuyên pha cà phê và làm bánh');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP026')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP026', N'Trà Ô Long TEA+ Plus Suntory (Thùng 24 chai 450ml)', N'Thùng', 215000, 185000, 130, '/images/products/sp026-oolong-tea.svg', N'Trà ô long chứa OTPP hỗ trợ hạn chế hấp thu chất béo thanh mát');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP027')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP027', N'Bánh gạo giòn One One Bò Nướng Ngô (Gói 150g)', N'Gói', 28000, 22000, 220, '/images/products/sp027-one-one.svg', N'Bánh gạo nướng không chiên qua dầu giòn tan thơm lừng vị bò nướng ngô');
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SP028')
BEGIN
    INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, CostPrice, StockQuantity, ImageUrl, Description)
    VALUES ('SP028', N'Nước xả vải Comfort Đậm Đặc Hương Ban Mai (Túi 3.8 lít)', N'Túi', 235000, 198000, 95, '/images/products/sp028-comfort.svg', N'Nước xả vải gia đình túi lớn 3.8L lưu hương thơm ngát suốt ngày dài');
END
GO

-- Xác nhận tổng số lượng sản phẩm sau khi chèn
SELECT COUNT(*) AS TotalProducts FROM Products;
GO
