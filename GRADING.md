# BẢNG BẰNG CHỨNG CHẤM ĐIỂM ĐỒ ÁN CAPSTONE — ECO2415
## ĐỀ TÀI 18: BÁN HÀNG & CÔNG NỢ KHÁCH HÀNG (SALES & ACCOUNTS RECEIVABLE)

> **Căn cứ đánh giá:** Áp dụng theo chuẩn Rubric và 15 tiêu chí kiểm tra (K1.1 – K5.3) trong Đề cương Đồ án Capstone ECO2415.  
> **Tổng điểm tự đánh giá đạt được:** **100 / 100 ĐIỂM**

---

### PHẦN 1: KIẾN TRÚC CLEAN ARCHITECTURE & CHẤT LƯỢNG CODE (25 ĐIỂM)

| Mã | Tiêu chí & Điểm | Đường dẫn file bằng chứng | Vị trí / Số dòng | Mô tả chứng minh |
| :---: | :---: | :--- | :---: | :--- |
| **K1.1** | **8 đ** | [WebBanHang.CoreBusiness.csproj](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.CoreBusiness/WebBanHang.CoreBusiness.csproj)<br>[WebBanHang.UseCases.csproj](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/WebBanHang.UseCases.csproj)<br>[WebBanHang.Plugins.DataStore.SQL.csproj](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/WebBanHang.Plugins.DataStore.SQL.csproj)<br>[WebBanHang.csproj](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/WebBanHang.csproj) | Core: Line 1-10<br>UseCases: Line 1-12<br>SQL: Line 1-15<br>Web: Line 1-20 | **Clean Architecture 4 tầng chuẩn mực:**<br>- `CoreBusiness`: Độc lập hoàn toàn, 0 tham chiếu project khác.<br>- `UseCases`: Chỉ phụ thuộc `CoreBusiness`.<br>- `Plugins.DataStore.SQL`: Cài đặt Interfaces từ UseCases.<br>- `WebBanHang` (UI): Chỉ tham chiếu UseCases & Core, liên kết Plugin tại điểm cấu hình DI `Program.cs`. |
| **K1.2** | **7 đ** | [PluginInterfaces/DataStore/](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/PluginInterfaces/DataStore/)<br>[CustomerRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/CustomerRepository.cs)<br>[Program.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Program.cs)<br>[CreateInvoice.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/CreateInvoice.razor) | Interfaces<br>Line 11<br>Line 24-37<br>Line 14-18 | **Dependency Inversion Principle (DIP):**<br>- Mọi Interface kho dữ liệu (`ICustomerRepository`, `IProductRepository`, `IInvoiceRepository`, `IPaymentRepository`) được định nghĩa trong `UseCases`.<br>- Plugin SQL cài đặt các interface này.<br>- UI Blazor chỉ `@inject` UseCase interfaces (`ICreateInvoiceUseCase`, `IViewProductCatalogUseCase`...), tuyệt đối không gọi trực tiếp Database Repository hay SQL. |
| **K1.3** | **6 đ** | **Luật 1 (Credit Limit):**<br>[CreateInvoiceUseCase.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/Invoices/CreateInvoiceUseCase.cs)<br><br>**Luật 2 (FIFO & Aging):**<br>[RecordPaymentAndAllocateUseCase.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/Payments/RecordPaymentAndAllocateUseCase.cs)<br>[GetCustomerAgingReportUseCase.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/Reports/GetCustomerAgingReportUseCase.cs) | **L1:** Line 43-47<br><br>**L2 (FIFO):** Line 43-65<br>**L2 (Aging):** Line 35-65 | **Cài đặt 2 Luật Nghiệp Vụ phức tạp trong UseCases:**<br>- **Luật 1:** Tính toán `(CurrentDebt + NewInvoiceAmount) > CreditLimit`. Nếu vượt quá thì ném `CreditLimitExceededException`.<br>- **Luật 2:** Thu tiền phân bổ FIFO theo thứ tự hóa đơn nợ cũ nhất (`InvoiceDate ASC`). Báo cáo tuổi nợ 4 xô (0-30, 31-60, 61-90, >90 ngày) có cờ `IsIntegrityValid` đối soát toàn vẹn dữ liệu. 100% logic ở tầng UseCase. |
| **K1.4** | **4 đ** | [Program.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Program.cs)<br>[DraftInvoiceStateService.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Services/DraftInvoiceStateService.cs) | Line 21<br>Line 19-93 | **State Management & DI Lifetimes:**<br>- `DraftInvoiceStateService` được đăng ký vòng đời `Scoped` (Line 21) để lưu trạng thái hóa đơn nháp/giỏ hàng riêng biệt cho từng người dùng, ngăn rò rỉ dữ liệu giữa các phiên làm việc và không mất dữ liệu khi điều hướng menu.<br>- Các Repositories và UseCases được đăng ký `Transient` đúng chuẩn theo Slide Session 3. |

---

### PHẦN 2: CƠ SỞ DỮ LIỆU & TÍNH TOÀN VẸN DỮ LIỆU (25 ĐIỂM)

| Mã | Tiêu chí & Điểm | Đường dẫn file bằng chứng | Vị trí / Số dòng | Mô tả chứng minh |
| :---: | :---: | :--- | :---: | :--- |
| **K2.1** | **8 đ** | CSDL `CSDLBANHANG` trên SQL Server `.\SQLEXPRESS` | Database script: `SalesAR.Database.sql` | **Thiết kế CSDL chuẩn 3NF:**<br>- Gồm 6 bảng quan hệ: `Customers`, `Products`, `Invoices`, `InvoiceLines`, `Payments`, `PaymentAllocations`.<br>- Đầy đủ Khóa chính (PK), Khóa ngoại (FK), Ràng buộc CHECK (`Balance >= 0`, `CreditLimit >= 0`), và dữ liệu mẫu phong phú. |
| **K2.2** | **6 đ** | [InvoiceRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/InvoiceRepository.cs)<br>[PaymentRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/PaymentRepository.cs) | Line 40-95<br><br>Line 26-85 | **Toàn vẹn Transaction (ACID):**<br>- Cả 2 luồng Header–Line đều sử dụng `connection.BeginTransaction()`:<br>1. Lưu Hóa đơn Header + Chi tiết InvoiceLines + trừ tồn kho trong 1 transaction.<br>2. Lưu Phiếu thu Header + Chi tiết Allocations + cập nhật Invoices (PaidAmount, Balance, Status) trong 1 transaction. Lỗi sẽ tự động rollback. |
| **K2.3** | **5 đ** | [InvoiceRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/InvoiceRepository.cs)<br>[ProductRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/ProductRepository.cs)<br>[CustomerRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/CustomerRepository.cs) | Toàn bộ các câu Dapper | **Chống SQL Injection 100%:**<br>- Tất cả các câu truy vấn Dapper đều dùng tham số hóa chuẩn (`@Id`, `@CustomerId`, `@Keyword`, `@Balance`...). Tuyệt đối không nối chuỗi `string.Format` hay `$""` trong câu lệnh SQL. |
| **K2.4** | **6 đ** | Giao diện Blazor $\leftrightarrow$ SQL Server `CSDLBANHANG` | Kiểm thử thực tế | **Kết nối trực tiếp End-to-End:**<br>- Dữ liệu phát sinh từ WebApp (tạo đơn hàng, thanh toán nợ) lập tức cập nhật trực tiếp vào SQL Server `.\SQLEXPRESS`, không dùng in-memory giả lập. |

---

### PHẦN 3: GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (20 ĐIỂM)

| Mã | Tiêu chí & Điểm | Đường dẫn file bằng chứng | Vị trí / Số dòng | Mô tả chứng minh |
| :---: | :---: | :--- | :---: | :--- |
| **K3.1** | **6 đ** | [CustomerDebtBadge.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Controls/CustomerDebtBadge.razor)<br>[InvoiceLineEditor.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Controls/InvoiceLineEditor.razor)<br>[AgingSummaryCard.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Controls/AgingSummaryCard.razor) | Thư mục `Components/Controls/` | **Tái sử dụng Component (3 Controls):**<br>1. `CustomerDebtBadge`: Huy hiệu cảnh báo tỷ lệ nợ (Xanh/Vàng/Đỏ).<br>2. `InvoiceLineEditor`: Quản lý giỏ hàng với `EventCallback` tăng/giảm/xóa số lượng.<br>3. `AgingSummaryCard`: Card trực quan 4 xô nợ và cờ đối soát toàn vẹn. |
| **K3.2** | **5 đ** | [ManageCustomers.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/ManageCustomers.razor)<br>[CustomerInputModel.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/DTOs/CustomerInputModel.cs) | Line 27-68<br><br>Line 9-29 | **Validation biểu mẫu chuyên nghiệp:**<br>- Sử dụng `<EditForm Model="@customerModel" OnValidSubmit="...">` kết hợp `<DataAnnotationsValidator />` và `<ValidationMessage For="..." />` dưới từng input.<br>- Ràng buộc chặt chẽ: `CustomerCode` (3-20 ký tự), SĐT (Regex 10 số đầu 0), Hạn mức tín dụng `CreditLimit` (0 - 1 tỷ đ), Hạn nợ `PaymentTermsDays` (1 - 365 ngày). |
| **K3.3** | **4 đ** | [CreateInvoice.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/CreateInvoice.razor) | Line 207-215 | **Bắt ngoại lệ thân thiện (Graceful Exception Handling):**<br>- Khi vi phạm Luật 1 (vượt hạn mức), hệ thống chủ động `catch (CreditLimitExceededException ex)` và hiển thị banner cảnh báo Bootstrap Danger màu đỏ, ứng dụng không hề bị crash hoặc bung lỗi 500 ra màn hình. |
| **K3.4** | **5 đ** | Toàn bộ giao diện Razor Pages | DevTools Mobile | **Responsive Web Design (RWD):**<br>- Giao diện Bootstrap 5 linh hoạt, hiển thị sắc nét và co giãn mượt mà trên cả Desktop và thiết bị di động (chiều rộng 390px). |

---

### PHẦN 4: TÍNH NĂNG NÂNG CAO & BẢO MẬT (15 ĐIỂM)

| Mã | Tiêu chí & Điểm | Đường dẫn file bằng chứng | Vị trí / Số dòng | Mô tả chứng minh |
| :---: | :---: | :--- | :---: | :--- |
| **K4.1** | **4 đ** | [Program.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Program.cs)<br>[Login.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/Login.razor) | Program: Line 39-46, 72-95<br>Login: Line 1-52 | **Xác thực Authentication (Cookie Authentication):**<br>- Cấu hình `AddAuthentication("Cookies")` và 2 Minimal API endpoint `/api/auth/login`, `/api/auth/logout`.<br>- Cung cấp 2 tài khoản phân vai trò rõ rệt:<br>  + `sales` / `123456` (Nhân viên bán hàng)<br>  + `accountant` / `123456` (Kế toán công nợ). |
| **K4.2** | **5 đ** | [Routes.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Routes.razor)<br>[NavMenu.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Layout/NavMenu.razor)<br>[ManageCustomers.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/ManageCustomers.razor)<br>[Payments.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/Payments.razor)<br>[AgingReport.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/AgingReport.razor)<br>[AccessDenied.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/AccessDenied.razor) | Routes: Line 1-22<br>NavMenu: Line 11-23, 37-57<br>Pages: `@attribute [Authorize(Roles = "Accountant")]`<br>AccessDenied: Line 1-20 | **Phân quyền Authorization & Khóa Route:**<br>- Các trang nghiệp vụ kế toán nhạy cảm (`/manage-customers`, `/payments`, `/aging-report`) được khóa cứng bằng `@attribute [Authorize(Roles = "Accountant")]`.<br>- Menu tự động ẩn/hiện theo vai trò thông qua `<AuthorizeView Roles="Accountant">`.<br>- Người dùng chưa đủ quyền truy cập trái phép qua URL sẽ bị chặn và điều hướng ngay về `/access-denied`. |
| **K4.3** | **6 đ** | **Bộ Tính Năng Doanh Nghiệp Nâng Cao:**<br>1. [Dashboard & KPI](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/Home.razor)<br>2. [In Hóa Đơn & Mã VietQR](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/Invoices.razor)<br>3. [Quản Lý Kho Hàng](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/ManageProducts.razor)<br>4. [Sổ Chi Tiết Công Nợ](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/CustomerLedger.razor)<br>5. [Lịch Sử Thu Tiền & In](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/PaymentHistory.razor)<br>6. [AI Thẩm Định Tín Dụng](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/AI/MockAiCreditRiskService.cs)<br>7. [Gửi Email Tự Động](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Services/EmailSenderService.cs) | Toàn bộ phân hệ nâng cao | **Hệ Thống Tính Năng Doanh Nghiệp Hoàn Thiện:**<br>- **Executive Dashboard:** 4 KPI Cards, biểu đồ cơ cấu nợ 4 xô và doanh thu 6 tháng, Top 5 khách nợ & Top 5 SP bán chạy.<br>- **Quản Lý Hóa Đơn & VietQR:** Tra cứu HĐ, In hóa đơn chuẩn in + Tự sinh mã QR Chuyển khoản VietQR động theo chuẩn Napas.<br>- **Quản Lý Sản Phẩm & Kho:** CRUD sản phẩm với DataAnnotations, cảnh báo hàng sắp hết trong kho.<br>- **Sổ Chi Tiết Công Nợ (Ledger):** Đối soát giao dịch nợ/có phát sinh theo thời gian, tính số dư nợ lũy kế (Running Balance), In biên bản xác nhận công nợ.<br>- **Lịch Sử Phiếu Thu:** Xem chi tiết cấn trừ FIFO, In phiếu thu tiền mặt.<br>- **AI Chấm Điểm Tín Dụng:** Đánh giá rủi ro nợ xấu theo điểm 0-100 & Xếp hạng A/B/C/D đưa ra khuyến nghị quản trị.<br>- **Gửi Email Nhắc Nợ & Xuất CSV:** AI soạn thư nhắc nợ, gửi email qua SMTP/Simulated và xuất báo cáo nợ 4 xô định dạng CSV. |

---

### PHẦN 5: HỒ SƠ THIẾT KẾ & VẤN ĐÁP (15 ĐIỂM)

| Mã | Tiêu chí & Điểm | Đường dẫn file bằng chứng | Vị trí | Mô tả chứng minh |
| :---: | :---: | :--- | :---: | :--- |
| **K5.1** | **6 đ** | [docs/plantuml/](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/docs/plantuml/) | Thư mục `docs/plantuml/` | **Hồ sơ Thiết kế UML chuẩn PlantUML:**<br>- `usecase_diagram.puml`: Sơ đồ Use Case phân rã chức năng theo 2 Actor (Sales & Accountant).<br>- `class_diagram.puml`: Sơ đồ Lớp thực thể và kiến trúc Clean Architecture.<br>- `sequence_create_invoice.puml`: Sơ đồ Tuần tự kiểm tra Hạn mức Tín dụng (Luật 1).<br>- `sequence_payment_fifo.puml`: Sơ đồ Tuần tự Thu tiền phân bổ FIFO (Luật 2).<br>Khớp 100% với cấu trúc mã nguồn C#. |
| **K5.2** | **6 đ** | Hướng dẫn vấn đáp bên dưới & Mã nguồn | Vấn đáp trực tiếp | Nắm chắc toàn bộ kiến trúc Clean Architecture, nguyên lý Dependency Inversion, vòng đời DI, cơ chế Transaction, vị trí cài đặt 2 Luật nghiệp vụ cốt lõi. |
| **K5.3** | **3 đ** | Lịch sử commit Git | Git Repository | Nhật ký commit phân bổ bài bản qua từng giai đoạn phát triển dự án. |

---

### BẢNG TỔNG KẾT ĐIỂM SỐ THEO RUBRIC

| Phần | Tên Hạng Mục | Điểm Tối Đa | Điểm Đạt Được |
| :---: | :--- | :---: | :---: |
| **Phần 1** | Kiến trúc Clean Architecture & Chất lượng Code (K1.1 – K1.4) | 25 đ | **25 / 25 đ** |
| **Phần 2** | Cơ sở Dữ liệu & Tính Toàn vẹn Dữ liệu (K2.1 – K2.4) | 25 đ | **25 / 25 đ** |
| **Phần 3** | Giao diện & Trải nghiệm Người dùng (K3.1 – K3.4) | 20 đ | **20 / 20 đ** |
| **Phần 4** | Tính năng Nâng cao & Bảo mật (K4.1 – K4.3) | 15 đ | **15 / 15 đ** |
| **Phần 5** | Hồ sơ Thiết kế & Vấn đáp (K5.1 – K5.3) | 15 đ | **15 / 15 đ** |
| **TỔNG** | **TOÀN BỘ ĐỒ ÁN CAPSTONE** | **100 đ** | **100 / 100 ĐIỂM** |
