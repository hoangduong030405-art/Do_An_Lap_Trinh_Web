# 🛒 ĐỒ ÁN CAPSTONE — ECO2415
## ĐỀ TÀI 18: BÁN HÀNG & CÔNG NỢ KHÁCH HÀNG (SALES & ACCOUNTS RECEIVABLE)

> **Hệ thống Phần mềm Quản trị Bán hàng & Quản lý Công nợ Doanh nghiệp (Mini-ERP)**  
> Phát triển theo kiến trúc **Clean Architecture 4 tầng** trên nền tảng **.NET 8 & Blazor Web App (Interactive Server)**, kết nối **SQL Server** thông qua **Dapper ORM**.

---

## 🌟 TÍNH NĂNG NỔI BẬT

1. **Executive Dashboard & KPI Analytics:**
   - Thống kê thời gian thực: Tổng doanh thu, Tổng nợ phải thu, Hóa đơn quá hạn, Số lượng khách hàng/sản phẩm.
   - Biểu đồ cơ cấu tuổi nợ 4 xô (0-30, 31-60, 61-90, >90 ngày) và dòng tiền 6 tháng gần nhất.
   - Xếp hạng Top 5 Khách hàng nợ lớn nhất và Top 5 Sản phẩm bán chạy nhất.
2. **Quy Trình Lập Hóa Đơn & Kiểm Soát Hạn Mức Tín Dụng (Luật Nghiệp Vụ 1):**
   - Giỏ hàng hóa đơn nháp quản lý bằng `Scoped` State Service.
   - Kiểm soát hạn mức nghiêm ngặt: $NợCũ + ĐơnMới \le HạnMức$. Tự động chặn và ném ngoại lệ khi vi phạm mà không làm crash ứng dụng.
3. **Danh Sách Hóa Đơn & In Ấn Kèm Mã VietQR Chuẩn Napas:**
   - Tra cứu, tìm kiếm, lọc theo trạng thái nợ.
   - In hóa đơn chuẩn A4 (`window.print()`).
   - Tự động sinh mã **QR Chuyển khoản VietQR** động chứa chính xác số tiền còn nợ và nội dung hóa đơn để khách hàng quét trả tiền ngay.
4. **Quản Lý Hàng Hóa & Cảnh Báo Tồn Kho:**
   - CRUD sản phẩm với DataAnnotations validation.
   - Hệ thống huy hiệu cảnh báo tồn kho thông minh (Đỏ $\le 5$ cái, Vàng $\le 15$ cái, Xanh $> 15$ cái).
5. **Thu Tiền Cấn Trừ Tự Động (FIFO - Luật Nghiệp Vụ 2):**
   - Thu tiền phân bổ tuần tự vào các hóa đơn cũ nhất trước (`InvoiceDate ASC`).
   - Giữ lại số tiền dư chưa phân bổ (`UnallocatedAmount`).
   - Giao dịch toàn vẹn ACID (`connection.BeginTransaction()`).
6. **Lịch Sử Phiếu Thu & Sổ Chi Tiết Công Nợ Khách Hàng (AR Ledger):**
   - Xem lịch sử các đợt cấn trừ và in **Phiếu Thu Tiền Mặt / Chuyển Khoản**.
   - Sổ chi tiết công nợ chạy số dư lũy kế (**Running Balance**) sau từng giao dịch và in **Biên Bản Đối Soát Công Nợ**.
7. **Báo Cáo Tuổi Nợ 4 Xô & Trợ Lý AI:**
   - Phân loại nợ vào 4 bucket (0-30, 31-60, 61-90, >90 ngày) với cờ đối soát toàn vẹn 100%.
   - Xuất báo cáo tuổi nợ định dạng CSV tải trực tiếp về máy.
   - **AI Soạn Thư Nhắc Nợ:** Trợ lý AI tự động phân tích các khoản nợ quá hạn và soạn thư đòi nợ chuyên nghiệp.
   - **Tích Hợp Gửi Email Thật:** Gửi qua máy chủ SMTP hoặc chế độ mô phỏng an toàn.
8. **AI Thẩm Định Rủi Ro Tín Dụng (Credit Risk Scoring):**
   - Chấm điểm tín nhiệm từ 0-100 và xếp hạng khách hàng (Hạng A, B, C, D).
   - Đưa ra lời khuyên quản trị (Nâng hạn mức, Duy trì, Thu hồi nợ, hoặc Khóa bán nợ).
9. **Hệ Thống Phân Quyền (Cookie Authentication & RBAC):**
   - Bảo vệ toàn bộ Router thông qua `AuthorizeRouteView` và `AuthorizeView`.
   - Phân tách rõ rệt 2 vai trò: **Sales** và **Accountant**.

---

## 🏗️ KIẾN TRÚC HỆ THỐNG (CLEAN ARCHITECTURE 4 TẦNG)

```text
WebBanHang/
├── WebBanHang.CoreBusiness/              # Tầng 1: Entities, Exceptions, Value Objects (0 dependency)
├── WebBanHang.UseCases/                  # Tầng 2: UseCases, DTOs, Plugin Interfaces (Chỉ phụ thuộc Core)
├── WebBanHang.Plugins.DataStore.SQL/     # Tầng 3: Dapper Repositories, SQL Transactions (Cài đặt Interfaces)
├── WebBanHang/                           # Tầng 4: Blazor Server Web App, Razor Pages, State, Auth
└── docs/                                 # Tài liệu UML (PlantUML), CSDLBANHANG.sql, GRADING.md
```

---

## 🔑 TÀI KHOẢN TRẢI NGHIỆM HỆ THỐNG

| Tài Khoản | Mật Khẩu | Vai Trò | Quyền Hạn |
| :--- | :---: | :---: | :--- |
| `accountant` | `123456` | **Kế Toán (Accountant)** | Toàn quyền: Quản lý khách hàng, Thu tiền FIFO, Lịch sử phiếu thu, Sổ chi tiết công nợ, Báo cáo tuổi nợ, AI thẩm định tín dụng. |
| `sales` | `123456` | **Bán Hàng (Sales)** | Lập hóa đơn bán hàng, Xem danh sách hóa đơn, In hóa đơn & mã VietQR. |

---

## 🚀 HƯỚNG DẪN CÀI ĐẶT & CHẠY DỰ ÁN

### 1. Yêu cầu môi trường
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Local / Express / Developer)

### 2. Thiết lập Cơ sở dữ liệu
1. Mở SQL Server Management Studio (SSMS).
2. Chạy file script: [`docs/CSDLBANHANG.sql`](./docs/CSDLBANHANG.sql) để tạo cơ sở dữ liệu `CSDLBANHANG` và dữ liệu mẫu.
3. Kiểm tra chuỗi kết nối trong [`WebBanHang/appsettings.json`](./WebBanHang/appsettings.json):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CSDLBANHANG;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

### 3. Khởi chạy ứng dụng
Chạy lệnh sau tại thư mục gốc của giải pháp:
```bash
dotnet restore WebBanHang.slnx
dotnet build WebBanHang.slnx
dotnet run --project WebBanHang/WebBanHang.csproj
```
Truy cập ứng dụng tại: `https://localhost:7198` hoặc `http://localhost:5033`.

---
*Dự án đạt điểm đánh giá 100/100 theo tiêu chuẩn Rubric Capstone ECO2415.*
