USE CSDLBANHANG;
GO

-- 1. CẬP NHẬT DANH MỤC KHÁCH HÀNG (CUSTOMERS)
UPDATE Customers SET 
    FullName = N'Tạp Hóa Bà Ba - Chợ Đông Ba',
    Address = N'12 Trần Hưng Đạo, TP. Huế'
WHERE CustomerCode = 'KH001';

UPDATE Customers SET 
    FullName = N'Siêu Thị Mini Tuấn Hương',
    Address = N'145 Đinh Tiên Hoàng, TP. Huế'
WHERE CustomerCode = 'KH002';

UPDATE Customers SET 
    FullName = N'Cửa Hàng Tiện Lợi An Phú',
    Address = N'88 Hùng Vương, TP. Huế'
WHERE CustomerCode = 'KH003';

UPDATE Customers SET 
    FullName = N'Đại Lý Phân Phối Nam Hà',
    Address = N'55 Lê Lợi, TP. Huế'
WHERE CustomerCode = 'KH004';

-- 2. CẬP NHẬT DANH MỤC SẢN PHẨM (PRODUCTS)
UPDATE Products SET 
    Name = N'Nước ngọt Coca-Cola Sleek (Thùng 24 lon 320ml)',
    Unit = N'Thùng',
    Description = N'Hàng chính hãng Coca-Cola Việt Nam'
WHERE ProductCode = 'SP001';

UPDATE Products SET 
    Name = N'Bia Heineken Silver (Thùng 24 lon 330ml)',
    Unit = N'Thùng',
    Description = N'Bia cao cấp Heineken Silver lon cao'
WHERE ProductCode = 'SP002';

UPDATE Products SET 
    Name = N'Mì Hảo Hảo Tôm Chua Cay (Thùng 30 gói)',
    Unit = N'Thùng',
    Description = N'Mì ăn liền Acecook quốc dân'
WHERE ProductCode = 'SP003';

UPDATE Products SET 
    Name = N'Dầu ăn Simply Đậu Nành (Chai 1 lít)',
    Unit = N'Chai',
    Description = N'Dầu thực vật dinh dưỡng Simply 1L'
WHERE ProductCode = 'SP004';

UPDATE Products SET 
    Name = N'Sữa tươi Vinamilk có đường (Lốc 4 hộp 180ml)',
    Unit = N'Lốc',
    Description = N'Sữa tươi tiệt trùng Vinamilk 100%'
WHERE ProductCode = 'SP005';

UPDATE Products SET 
    Name = N'Bia Tiger Crystal (Thùng 24 lon 330ml)',
    Unit = N'Thùng',
    Description = N'Bia Tiger Crystal lên men lạnh sảng khoái'
WHERE ProductCode = 'SP006';

UPDATE Products SET 
    Name = N'Nước tinh khiết Aquafina (Thùng 24 chai 500ml)',
    Unit = N'Thùng',
    Description = N'Nước uống tinh khiết Aquafina đóng thùng 24 chai'
WHERE ProductCode = 'SP007';

UPDATE Products SET 
    Name = N'Mì Omachi Xốt Bò Hầm (Thùng 30 gói)',
    Unit = N'Thùng',
    Description = N'Mì khoai tây Omachi xốt bò hầm cao cấp'
WHERE ProductCode = 'SP008';

UPDATE Products SET 
    Name = N'Nước tăng lực Red Bull Thái Lan (Thùng 24 lon 250ml)',
    Unit = N'Thùng',
    Description = N'Nước tăng lực lon vàng Red Bull chính hãng'
WHERE ProductCode = 'SP009';

UPDATE Products SET 
    Name = N'Cà phê hòa tan G7 3in1 Trung Nguyên (Thùng 24 hộp x 18 gói)',
    Unit = N'Thùng',
    Description = N'Cà phê hòa tan Trung Nguyên đậm đà'
WHERE ProductCode = 'SP010';

UPDATE Products SET 
    Name = N'Bánh ChocoPie Orion Truyền Thống (Thùng 8 hộp x 12 cái)',
    Unit = N'Thùng',
    Description = N'Bánh mềm phủ socola nhân kem dẻo marshmallow'
WHERE ProductCode = 'SP011';

UPDATE Products SET 
    Name = N'Nước mắm Nam Ngư Đệ Nhị (Thùng 15 chai 900ml)',
    Unit = N'Thùng',
    Description = N'Nước mắm Nam Ngư hương vị cá cơm thơm ngon'
WHERE ProductCode = 'SP012';

-- 3. CẬP NHẬT GHI CHÚ PHIẾU THU (PAYMENTS)
UPDATE Payments SET 
    Note = N'Tạp hóa Bà Ba thanh toán tiền mặt đợt 1 cấn trừ FIFO cho HĐ cũ nhất'
WHERE PaymentNumber = 'PT-2026-001';

UPDATE Payments SET 
    Note = N'Tạp hóa Bà Ba chuyển khoản VietQR đợt 2 cấn trừ FIFO cho HĐ tiếp theo'
WHERE PaymentNumber = 'PT-2026-002';
GO
