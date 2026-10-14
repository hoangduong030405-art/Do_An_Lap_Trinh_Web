USE CSDLBANHANG;
GO

UPDATE Products SET Name = N'Nước ngọt Coca-Cola Sleek (Thùng 24 lon 320ml)', Unit = N'Thùng', ImageUrl = '/images/products/sp001-coca-cola.svg', Description = N'Nước ngọt có gas Coca-Cola Sleek chính hãng thùng 24 lon' WHERE ProductCode = 'SP001';
UPDATE Products SET Name = N'Bia Heineken Silver (Thùng 24 lon 330ml)', Unit = N'Thùng', ImageUrl = '/images/products/sp002-heineken.svg', Description = N'Bia cao cấp Heineken Silver êm mượt sảng khoái thùng 24 lon' WHERE ProductCode = 'SP002';
UPDATE Products SET Name = N'Mì Hảo Hảo Tôm Chua Cay (Thùng 30 gói)', Unit = N'Thùng', ImageUrl = '/images/products/sp003-hao-hao.svg', Description = N'Mì ăn liền Acecook Hảo Hảo tôm chua cay thùng 30 gói' WHERE ProductCode = 'SP003';
UPDATE Products SET Name = N'Dầu ăn Simply Đậu Nành (Chai 1 lít)', Unit = N'Chai', ImageUrl = '/images/products/sp004-simply.svg', Description = N'Dầu thực vật dinh dưỡng 100% đậu nành Simply chai 1L' WHERE ProductCode = 'SP004';
UPDATE Products SET Name = N'Sữa tươi Vinamilk có đường (Lốc 4 hộp 180ml)', Unit = N'Lốc', ImageUrl = '/images/products/sp005-vinamilk.svg', Description = N'Sữa tươi tiệt trùng Vinamilk 100% lốc 4 hộp 180ml' WHERE ProductCode = 'SP005';
UPDATE Products SET Name = N'Bia Tiger Crystal (Thùng 24 lon 330ml)', Unit = N'Thùng', ImageUrl = '/images/products/sp006-tiger-crystal.svg', Description = N'Bia Tiger Crystal lên men lạnh sảng khoái thùng 24 lon' WHERE ProductCode = 'SP006';
UPDATE Products SET Name = N'Nước tinh khiết Aquafina (Thùng 24 chai 500ml)', Unit = N'Thùng', ImageUrl = '/images/products/sp007-aquafina.svg', Description = N'Nước uống tinh khiết Aquafina đóng thùng 24 chai 500ml' WHERE ProductCode = 'SP007';
UPDATE Products SET Name = N'Mì Omachi Xốt Bò Hầm (Thùng 30 gói)', Unit = N'Thùng', ImageUrl = '/images/products/sp008-omachi.svg', Description = N'Mì khoai tây Omachi xốt bò hầm cao cấp thùng 30 gói' WHERE ProductCode = 'SP008';
GO

SELECT ProductCode, Name, Unit, UnitPrice, StockQuantity, ImageUrl FROM Products;
GO
