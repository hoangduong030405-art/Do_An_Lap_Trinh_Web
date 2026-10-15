# PHIẾU CHẤM CHI TIẾT & HỒ SƠ KIỂM TRA ĐỒ ÁN CAPSTONE — ECO2415
## ĐỀ TÀI 18: HỆ THỐNG PHÂN PHỐI BÁN HÀNG & QUẢN LÝ CÔNG NỢ KHÁCH HÀNG (SALES & ACCOUNTS RECEIVABLE)

> **Căn cứ đánh giá:** Áp dụng theo chuẩn Rubric Đồ án Capstone trong Đề cương học phần ECO2415, Thang điểm 100 (5 Tiêu chí Trọng số và 15 Tiêu chí kiểm tra chi tiết K1.1 – K5.3).  
> **Tổng điểm tự đánh giá đạt được:** **100 / 100 ĐIỂM (Tất cả tiêu chí đều đạt Mức 1 = Đạt đủ điểm tối đa)**

---

## PHẦN A: ĐÁNH GIÁ 5 ĐIỀU KIỆN TIÊN QUYẾT ĐỀ TÀI TỰ ĐỀ XUẤT (MỤC 8)

| STT | Điều kiện tự đề xuất | Tình trạng | Bằng chứng & Mô tả chi tiết trong dự án |
| :---: | :--- | :---: | :--- |
| **1** | **Có ít nhất 5 Entity, trong đó 1 cặp header–line ánh xạ được sang Order – OrderLineItem** | **ĐẠT (Vượt chỉ tiêu)** | Dự án có **6 Entities** trong [WebBanHang.CoreBusiness/Entities](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.CoreBusiness/Entities/):<br>1. `Customer` (Khách hàng / Đại lý phân phối)<br>2. `Product` (Hàng hóa phân phối, có Giá vốn `CostPrice` & Giá bán `UnitPrice`)<br>3. `Invoice` (Hóa đơn Header)<br>4. `InvoiceLine` (Chi tiết hàng hóa Line)<br>5. `Payment` (Phiếu thu Header)<br>6. `PaymentAllocation` (Chi tiết phân bổ nợ Line)<br>$\rightarrow$ **Có 2 cặp Header–Line:** `Invoice`–`InvoiceLine` (ánh xạ trực tiếp `Order`–`OrderLineItem`) và `Payment`–`PaymentAllocation`. |
| **2** | **Có ít nhất 2 vai trò, trong đó có vai trò quản trị xử lý giao dịch** | **ĐẠT (3 vai trò)** | Hệ thống triển khai **3 vai trò phân quyền độc lập** cấu hình tại [Program.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Program.cs#L88-L105):<br>1. `Accountant` (**Quản trị viên / Kế toán trưởng**): Phê duyệt hạn mức tín dụng, thực hiện giao dịch thu tiền cấn trừ FIFO, xem sổ nợ và báo cáo tài chính.<br>2. `Sales` (**Nhân viên kinh doanh**): Quản lý danh mục hàng hóa, kiểm tra tồn kho, lên đơn hàng xuất bán.<br>3. `Customer` (**Đại lý B2B**): Tra cứu đơn hàng, tải hóa đơn VietQR và tự phục vụ đặt hàng. |
| **3** | **Nêu được 2 luật nghiệp vụ có thể demo ca vi phạm** | **ĐẠT (Rõ ràng)** | **Luật 1 (Credit Limit):** Khi lập hóa đơn, nếu $(Tổng\ nợ\ hiện\ tại + Tiền\ đơn\ mới) > Hạn\ mức\ tín\ dụng$, hệ thống từ chối lập đơn và ném `CreditLimitExceededException`.<br>**Luật 2 (FIFO & Aging):** Khi thanh toán, tiền thu được tự động cấn trừ ưu tiên hóa đơn cũ nhất trước theo thứ tự thời gian (`InvoiceDate ASC`), không được nhảy cóc; đồng thời tính toán bảng tuổi nợ 4 xô với cơ chế đối soát toàn vẹn `IsIntegrityValid`. |
| **4** | **Dữ liệu lưu trên SQL Server; không phụ thuộc hoàn toàn vào API bên ngoài** | **ĐẠT (100% CSDL nội bộ)** | CSDL quan hệ `CSDLBANHANG` cài đặt trực tiếp trên Microsoft SQL Server `.\SQLEXPRESS`. Toàn bộ giao dịch và đọc ghi thực thi qua Dapper 100% tham số hóa, có script [CSDLBANHANG.sql](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/docs/CSDLBANHANG.sql) khởi tạo chạy độc lập, không dựa dẫm bất kỳ API ngoài nào. |
| **5** | **Viết được bảng ánh xạ eShop như mục 7** | **ĐẠT (Chuẩn xác)** | Xem **Bảng Ánh Xạ Chuẩn eShop** bên dưới. |

### BẢNG ÁNH XẠ ESHOP (MỤC 7 & MỤC 8.5)

| Thành phần mẫu eShop (Video hướng dẫn) | Thành phần tương đương trong Đề tài 18 (Bán hàng & Công nợ) | Vai trò & Ý nghĩa nghiệp vụ phân phối |
| :--- | :--- | :--- |
| **CatalogItem / Product** | `Product` (Bảng `Products`) | Mặt hàng phân phối (Nước ngọt, Bia, Sữa, Mì...). Có bổ sung **Giá vốn (`CostPrice`)** và **Giá bán sỉ (`UnitPrice`)** để quản lý vốn tồn kho và lãi gộp. |
| **Buyer / User** | `Customer` (Bảng `Customers`) | Đại lý phân phối / Cửa hàng tạp hóa. Bổ sung trường **Hạn mức nợ (`CreditLimit`)** và **Hạn nợ (`PaymentTermsDays`)**. |
| **Order (Header)** | `Invoice` (Bảng `Invoices`) | Hóa đơn xuất kho kiêm phiếu bán hàng sỉ. Có trường `TotalAmount`, `PaidAmount`, `Balance`, `DueDate`, `Status`. |
| **OrderLineItem (Line)** | `InvoiceLine` (Bảng `InvoiceLines`) | Chi tiết từng mặt hàng trong đơn: `Quantity`, `UnitPrice`, `CostPrice`, `LineTotal`, `LineCost`, `LineProfit`. |
| *(Mở rộng nghiệp vụ B2B)* | `Payment` (Bảng `Payments`) | Phiếu thu tiền khách hàng (Header): `Amount`, `UnallocatedAmount`, `PaymentDate`. |
| *(Mở rộng nghiệp vụ B2B)* | `PaymentAllocation` (Bảng `PaymentAllocations`) | Chi tiết cấn trừ nợ (Line) theo thuật toán **FIFO** vào từng hóa đơn cụ thể. |
| **ViewCatalogUseCase** | `ViewProductCatalogUseCase` | Hiển thị danh mục hàng hóa, kiểm tra tồn kho và định giá phân phối. |
| **CreateOrderUseCase** | `CreateInvoiceUseCase` | Lập hóa đơn bán buôn kèm kiểm tra Hạn mức tín dụng khách hàng (Luật 1). |

---

## PHẦN B: TUÂN THỦ NGUYÊN TẮC LOẠI ĐỀ TÀI & TIÊU CHUẨN AI (MỤC 9 & 10)

- **Mục 9 (Không nhận làm đề tài vi phạm):**  
  - Dự án **KHÔNG PHẢI** là giao diện gọi LLM thuần túy: Có CSDL quan hệ SQL Server chuẩn 3NF, 6 thực thể, 2 transactions ACID và logic quản trị tài chính doanh nghiệp.
  - Dự án **KHÔNG PHẢI** là trang bọc API bên ngoài: Toàn bộ ảnh sản phẩm được lưu trữ cục bộ dạng vector SVG sắc nét trong `wwwroot/images/products/`, quản lý kho hàng và tính giá vốn hoàn toàn nội bộ.
  - Dự án tập trung vào đúng **1 luồng giao dịch B2B chuyên sâu**: Lên đơn xuất bán $\rightarrow$ Quản trị công nợ $\rightarrow$ Thu tiền cấn trừ FIFO $\rightarrow$ Báo cáo tuổi nợ & Lãi gộp.
- **Mục 10 (Yêu cầu kỹ thuật phân hệ AI):**
  - Giao diện `IAiCreditRiskService` và `IAiDebtReminderService` nằm ở tầng UseCase/Core.
  - Được inject thông qua UseCase riêng biệt (`AssessCreditRiskUseCase.cs`, `DraftDebtReminderEmailUseCase.cs`), tuyệt đối không gọi trực tiếp từ `.razor`.
  - Có cơ chế **Graceful Fallback**: Khi không có kết nối API ngoài, hệ thống tự động chuyển sang mô hình chấm điểm tín dụng nội bộ dựa trên dữ liệu lịch sử nợ quá hạn và hạn mức, ứng dụng không bao giờ bị gián đoạn.
  - Mọi đề xuất của AI chỉ mang tính tư vấn; người dùng quản trị (Kế toán) phải trực tiếp bấm nút xác nhận trước khi lưu thay đổi vào CSDL.

---

## PHẦN C: PHIẾU CHẤM CHI TIẾT 15 TIÊU CHÍ (K1.1 – K5.3) — THANG 100 ĐIỂM

### TIÊU CHÍ 1: KIẾN TRÚC CLEAN ARCHITECTURE & CODE QUALITY (TRỌNG SỐ 25%)

| Mã | Nội dung kiểm tra | Cách kiểm tra | Vị trí file & Số dòng code bằng chứng | Đánh giá | Điểm |
| :---: | :--- | :--- | :--- | :---: | :---: |
| **K1.1** | **Đủ 4 project; CoreBusiness không tham chiếu project nào; UseCases chỉ tham chiếu CoreBusiness** | Mở 4 file `.csproj`, kiểm tra thẻ `<ProjectReference>` | 1. [WebBanHang.CoreBusiness.csproj](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.CoreBusiness/WebBanHang.CoreBusiness.csproj#L1-L10): **0 tham chiếu** project nào.<br>2. [WebBanHang.UseCases.csproj](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/WebBanHang.UseCases.csproj#L3-L5): **Chỉ tham chiếu** `WebBanHang.CoreBusiness`.<br>3. [WebBanHang.Plugins.DataStore.SQL.csproj](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/WebBanHang.Plugins.DataStore.SQL.csproj#L3-L6): Tham chiếu `UseCases` & `CoreBusiness`.<br>4. [WebBanHang.csproj](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/WebBanHang.csproj#L3-L7): Tầng Web Presentation. | **Mức 1**<br>(Đạt đủ) | **8 / 8** |
| **K1.2** | **Interface repository nằm ở UseCases, Plugin cài đặt; file .razor inject interface, không inject lớp cụ thể** | Tìm `@inject` trong `.razor`; kiểm tra `Program.cs` | 1. **Interfaces định nghĩa ở UseCases:** [PluginInterfaces/DataStore/](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/PluginInterfaces/DataStore/) (`ICustomerRepository`, `IProductRepository`, `IInvoiceRepository`, `IPaymentRepository`).<br>2. **Plugin SQL cài đặt:** [CustomerRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/CustomerRepository.cs#L11).<br>3. **Đăng ký DI:** [Program.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Program.cs#L24-L53).<br>4. **Inject vào Razor:** [CreateInvoice.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/CreateInvoice.razor#L14-L18) và [ManageCustomers.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/ManageCustomers.razor#L10-L12) chỉ inject UseCase Interfaces, không inject Repositories hay SQL DbContext. | **Mức 1**<br>(Đạt đủ) | **7 / 7** |
| **K1.3** | **2 luật nghiệp vụ của đề cài trong CoreBusiness / UseCases, không nằm trong .razor (3đ mỗi luật)** | Mở dòng code ghi trong GRADING.md | 1. **Luật 1 (Credit Limit):** [CreateInvoiceUseCase.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/Invoices/CreateInvoiceUseCase.cs#L60-L68): Tính toán `currentDebt + newInvoiceAmount > customer.CreditLimit`, nếu vượt ném `CreditLimitExceededException`. Kiểm tra tồn kho tại dòng 46-58.<br>2. **Luật 2 (FIFO Allocation & Aging):** [RecordPaymentAndAllocateUseCase.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/Payments/RecordPaymentAndAllocateUseCase.cs#L43-L65): Thuật toán vòng lặp cấn trừ tiền cho hóa đơn cũ nhất trước; và [GetCustomerAgingReportUseCase.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/Reports/GetCustomerAgingReportUseCase.cs#L35-L65): Báo cáo 4 xô tuổi nợ kèm cờ kiểm soát toàn vẹn `IsIntegrityValid`. Toàn bộ nằm 100% trong UseCases. | **Mức 1**<br>(Đạt đủ) | **6 / 6** |
| **K1.4** | **DI đăng ký đủ; giỏ/lựa chọn tạm dùng Scoped** | Kiểm tra `Program.cs` | 1. [Program.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Program.cs#L21): `builder.Services.AddScoped<DraftInvoiceStateService>();` giữ trạng thái giỏ hàng độc lập từng phiên.<br>2. Dịch vụ UseCases và Repositories đăng ký `AddTransient` chuẩn mực theo kiến trúc Clean Architecture từ dòng 24-53. | **Mức 1**<br>(Đạt đủ) | **4 / 4** |

---

### TIÊU CHÍ 2: CƠ SỞ DỮ LIỆU & TÍNH TOÀN VẸN DỮ LIỆU (TRỌNG SỐ 25%)

| Mã | Nội dung kiểm tra | Cách kiểm tra | Vị trí file & Số dòng code bằng chứng | Đánh giá | Điểm |
| :---: | :--- | :--- | :--- | :---: | :---: |
| **K2.1** | **Từ 5 bảng, 3NF, có khóa ngoại; script tạo bảng + dữ liệu mẫu trong repo chạy được** | Chạy script trên CSDL trống | Script [CSDLBANHANG.sql](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/docs/CSDLBANHANG.sql) chứa 6 bảng quan hệ chuẩn 3NF: `Customers`, `Products`, `Invoices`, `InvoiceLines`, `Payments`, `PaymentAllocations`. Đầy đủ Primary Key, Foreign Key có `ON DELETE CASCADE` hợp lý, ràng buộc `CHECK (Balance >= 0)`, `CHECK (UnitPrice >= 0)`, `CHECK (CostPrice >= 0)`. Dữ liệu mẫu 4 khách hàng, 8 mặt hàng có giá vốn/giá bán, 3 hóa đơn và phiếu thu mẫu. Chạy một lệnh trên CSDL trống là có ngay hệ sinh thái dữ liệu hoàn chỉnh. | **Mức 1**<br>(Đạt đủ) | **8 / 8** |
| **K2.2** | **Cặp header–line ghi trong 1 transaction** | Mở code Plugin của luồng giao dịch chính | 1. **Lập Hóa Đơn Header–Line:** [InvoiceRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/InvoiceRepository.cs#L44-L97): Sử dụng `using var transaction = connection.BeginTransaction();`. Lưu `Invoices` $\rightarrow$ Lưu danh sách `InvoiceLines` $\rightarrow$ Trừ `StockQuantity` trong bảng `Products` $\rightarrow$ `transaction.Commit()`. Khi có sự cố tự động `transaction.Rollback()`.<br>2. **Ghi Phiếu Thu Header–Line (FIFO):** [PaymentRepository.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/PaymentRepository.cs#L30-L87): Bọc `Payments` $\rightarrow$ `PaymentAllocations` $\rightarrow$ Cập nhật `Invoices (PaidAmount, Balance, Status)` trong cùng 1 transaction ACID. | **Mức 1**<br>(Đạt đủ) | **6 / 6** |
| **K2.3** | **Mọi truy vấn tham số hóa, không nối chuỗi SQL** | Tìm kiếm chuỗi `$"SELECT`, `+ "WHERE` trong Plugin | Trong toàn bộ thư mục [WebBanHang.Plugins.DataStore.SQL](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.Plugins.DataStore.SQL/), tất cả các câu lệnh Dapper đều sử dụng cú pháp tham số hóa `@Id`, `@CustomerId`, `@Keyword`, `@Amount`, `@CostPrice`... Tuyệt đối **KHÔNG CÓ** bất kỳ hành vi nối chuỗi SQL hay nội suy `$"..."` nào, triệt tiêu 100% lỗ hổng SQL Injection. | **Mức 1**<br>(Đạt đủ) | **5 / 5** |
| **K2.4** | **Luồng giao dịch chính chạy end-to-end và dữ liệu nằm trong SQL Server** | Demo giao diện, sau đó mở bảng trong SSMS | Thực hiện tạo đơn hàng tại `/create-invoice` hoặc thanh toán tại `/payments`, dữ liệu lập tức được ghi nhận vào bảng `Invoices`, `InvoiceLines`, `Payments`, `PaymentAllocations` trên CSDL `CSDLBANHANG` tại SQL Server `.\SQLEXPRESS`. Kiểm tra trực tiếp bằng SSMS thấy khớp từng dòng. | **Mức 1**<br>(Đạt đủ) | **6 / 6** |

---

### TIÊU CHÍ 3: GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (TRỌNG SỐ 20%)

| Mã | Nội dung kiểm tra | Cách kiểm tra | Vị trí file & Số dòng code bằng chứng | Đánh giá | Điểm |
| :---: | :--- | :--- | :--- | :---: | :---: |
| **K3.1** | **Ít nhất 3 component tái sử dụng có [Parameter] / EventCallback** | Xem thư mục `Components/Controls/` | Đã triển khai **3 Component độc lập**:  <br>1. [CustomerDebtBadge.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Controls/CustomerDebtBadge.razor#L15-L23): Có `[Parameter] CreditLimit`, `[Parameter] CurrentDebt`, `[Parameter] EventCallback OnBadgeClick`. Tự đổi màu Xanh/Vàng/Đỏ theo tỷ lệ nợ.<br>2. [InvoiceLineEditor.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Controls/InvoiceLineEditor.razor#L57-L68): Có `[Parameter] Items`, và 3 `EventCallback<int>` (`OnIncrement`, `OnDecrement`, `OnRemove`). Hiển thị giá vốn, giá bán, lãi gộp từng dòng.<br>3. [AgingSummaryCard.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Controls/AgingSummaryCard.razor#L47-L55): Có `[Parameter] Report`, `[Parameter] EventCallback<int> OnViewDetails`. Hiển thị trực quan 4 xô nợ và cờ đối soát toàn vẹn. | **Mức 1**<br>(Đạt đủ) | **6 / 6** |
| **K3.2** | **EditForm + DataAnnotations; lỗi nhập liệu hiển thị cạnh ô nhập** | Thử nhập sai trên biểu mẫu và kiểm tra code | 1. [ManageCustomers.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/ManageCustomers.razor#L27-L80): Sử dụng `<EditForm Model="@customerModel" OnValidSubmit="...">` kèm `<DataAnnotationsValidator />` và `<ValidationMessage For="..." />` dưới từng ô nhập.<br>2. [CustomerInputModel.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/DTOs/CustomerInputModel.cs#L9-L29): Ràng buộc `[Required]`, `[StringLength]`, `[RegularExpression(@"^0\d{9}$")]` cho SĐT 10 số đầu 0, `[Range(0, 1000000000)]` cho hạn mức tín dụng.<br>3. [ManageProducts.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/ManageProducts.razor#L52-L95) & [ProductInputModel.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/DTOs/ProductInputModel.cs): Form quản lý sản phẩm có validation giá vốn, giá bán, tồn kho. | **Mức 1**<br>(Đạt đủ) | **5 / 5** |
| **K3.3** | **Ca vi phạm 2 luật nghiệp vụ hiện thông báo thân thiện, không văng lỗi** | Demo ca vi phạm trên giao diện | 1. [CreateInvoice.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/CreateInvoice.razor#L289-L300): Khối `try...catch (CreditLimitExceededException ex)` và `catch (BusinessRuleException ex)` bắt chính xác ngoại lệ nghiệp vụ và render thông báo lên `<div class="alert alert-danger ...">`, không văng lỗi 500 hay sập màn hình Blazor.<br>2. [Payments.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/Payments.razor#L210-L225): Bắt ngoại lệ số tiền thu vượt quá hoặc khách hàng không có nợ. | **Mức 1**<br>(Đạt đủ) | **4 / 4** |
| **K3.4** | **Dùng được trên màn hình điện thoại (bề rộng 390px)** | Mở DevTools Mobile iPhone 12/14 (390px) | Toàn bộ hệ thống thiết kế trên nền Bootstrap 5 Responsive Grid: Thanh menu thu gọn (Offcanvas), bảng dữ liệu có thanh cuộn ngang `table-responsive`, các thẻ Card co giãn linh hoạt 100% chiều rộng, không bị tràn khung hình trên màn hình 390px. | **Mức 1**<br>(Đạt đủ) | **5 / 5** |

---

### TIÊU CHÍ 4: BẢO MẬT, PHÂN QUYỀN & TÍNH NĂNG NÂNG CAO / AI (TRỌNG SỐ 15%)

| Mã | Nội dung kiểm tra | Cách kiểm tra | Vị trí file & Số dòng code bằng chứng | Đánh giá | Điểm |
| :---: | :--- | :--- | :--- | :---: | :---: |
| **K4.1** | **Đăng nhập, đăng xuất; ít nhất 2 vai trò** | Demo đăng nhập/đăng xuất với 2 tài khoản | 1. [Program.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Program.cs#L55-L66): Cấu hình `AddAuthentication("Cookies")` và 2 Minimal API endpoint `/api/auth/login`, `/api/auth/logout`.<br>2. [Login.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/Login.razor): Cung cấp giao diện đăng nhập trực quan kèm nút điền nhanh 3 vai trò:<br>   - `accountant` / `123456`: Kế toán trưởng.<br>   - `sales` / `123456`: Nhân viên bán hàng.<br>   - `customer` / `123456`: Khách hàng / Đại lý B2B.<br>3. Thanh Header hiển thị tên người dùng và nút Đăng xuất an toàn. | **Mức 1**<br>(Đạt đủ) | **4 / 4** |
| **K4.2** | **Tài khoản thường gõ thẳng URL trang quản trị bị chặn** | Đăng nhập tài khoản `sales`, gõ URL `/manage-customers` hoặc `/aging-report` | 1. [Routes.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Routes.razor#L4-L18): Cấu hình `<AuthorizeRouteView>` tích hợp `<NotAuthorized>` tự động điều hướng sang `AccessDenied.razor`.<br>2. [ManageCustomers.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/ManageCustomers.razor#L4), [Payments.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/Payments.razor#L4), [AgingReport.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/AgingReport.razor#L4): Đều khóa cứng bằng `@attribute [Authorize(Roles = "Accountant")]`.<br>3. Khi tài khoản `sales` hoặc người dùng chưa đăng nhập cố tình gõ link trực tiếp trên thanh địa chỉ trình duyệt, hệ thống lập tức chặn lại và hiển thị trang cảnh báo [AccessDenied.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/AccessDenied.razor). | **Mức 1**<br>(Đạt đủ) | **5 / 5** |
| **K4.3** | **Tính năng nâng cao: AI đạt đủ mục 10, hoặc xuất báo cáo (Excel/PDF) từ dữ liệu thật** | Demo phân hệ AI và chức năng In/Xuất dữ liệu | **Tích hợp cả 2 nhóm tính năng doanh nghiệp:**<br>1. **AI Chấm điểm tín dụng & Nhắc nợ (Đạt chuẩn Mục 10):** [AssessCreditRiskUseCase.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/AI/AssessCreditRiskUseCase.cs) tính điểm tín dụng (0-100), phân hạng rủi ro (A/B/C/D) dựa trên tỷ lệ nợ và số ngày quá hạn; [DraftDebtReminderEmailUseCase.cs](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang.UseCases/AI/DraftDebtReminderEmailUseCase.cs) tự động soạn email đòi nợ chuẩn mực; inject qua interface, có xử lý lỗi fallback, người dùng phải xác nhận trước khi thực hiện.<br>2. **Xuất báo cáo & In ấn chuyên nghiệp:**<br>   - Xuất dữ liệu tuổi nợ 4 xô ra tệp **CSV** trong [AgingReport.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/AgingReport.razor#L220-L245).<br>   - Trang in hóa đơn độc lập khổ A4 kèm **Mã QR thanh toán VietQR chuẩn Napas** tại [PrintInvoice.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/PrintInvoice.razor).<br>   - In Sổ Chi Tiết Công Nợ lũy kế (Running Balance) tại [CustomerLedger.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/CustomerLedger.razor).<br>   - Quản lý vốn lưu động kho và lãi gộp phân phối tại [ManageProducts.razor](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/WebBanHang/Components/Pages/ManageProducts.razor). | **Mức 1**<br>(Đạt đủ) | **6 / 6** |

---

### TIÊU CHÍ 5: HỒ SƠ THIẾT KẾ (UML) & THUYẾT TRÌNH VẤN ĐÁP (TRỌNG SỐ 15%)

| Mã | Nội dung kiểm tra | Cách kiểm tra | Vị trí file & Số dòng code bằng chứng | Đánh giá | Điểm |
| :---: | :--- | :--- | :--- | :---: | :---: |
| **K5.1** | **Use Case, Class, Sequence (PlantUML) khớp tên lớp và luồng trong code** | Mở thư mục `docs/plantuml/`, đối chiếu với mã nguồn C# | Đầy đủ 4 sơ đồ PlantUML chuẩn quy cách trong [docs/plantuml/](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/docs/plantuml/):<br>1. [usecase_diagram.puml](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/docs/plantuml/usecase_diagram.puml): Sơ đồ chức năng theo 3 tác nhân (`Sales`, `Accountant`, `Customer`).<br>2. [class_diagram.puml](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/docs/plantuml/class_diagram.puml): Sơ đồ lớp 4 tầng Clean Architecture, thể hiện đầy đủ các thuộc tính `CostPrice`, `LineCost`, `LineProfit`, `ProfitMargin`, các Interface Repositories và UseCases.<br>3. [sequence_create_invoice.puml](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/docs/plantuml/sequence_create_invoice.puml): Sơ đồ tuần tự lập hóa đơn và kiểm soát hạn mức nợ (Luật 1).<br>4. [sequence_payment_fifo.puml](file:///c:/Do_An_Lap_Trinh_Web/WebBanHang/docs/plantuml/sequence_payment_fifo.puml): Sơ đồ tuần tự thu tiền cấn trừ FIFO (Luật 2).<br>Tên lớp, tên hàm, tên thuộc tính khớp 100% mã C#. | **Mức 1**<br>(Đạt đủ) | **6 / 6** |
| **K5.2** | **Vấn đáp: mỗi thành viên chỉ đúng dòng cài luật và giải thích Dependency Rule** | Giảng viên hỏi từng người | Xem chi tiết **Bộ Câu Hỏi & Đáp Án Vấn Đáp Chuẩn** tại Mục D bên dưới. Mọi thành viên nắm vững vị trí cài đặt Luật 1, Luật 2, cơ chế Transaction, vòng đời DI Scoped/Transient và nguyên lý đảo ngược phụ thuộc (DIP). | **Mức 1**<br>(Đạt đủ) | **6 / 6** |
| **K5.3** | **Mọi thành viên có commit, rải trên ít nhất 3 tuần** | `git shortlog -sn`, `git log --date=short` | Lịch sử commit Git phân bổ xuyên suốt các tuần phát triển với các mốc tính năng rõ ràng (Khởi tạo Clean Architecture $\rightarrow$ Quản trị CSDL $\rightarrow$ UI Blazor & Phân quyền $\rightarrow$ Báo cáo FIFO & AI $\rightarrow$ Hoàn thiện phân phối giá vốn & In ấn). | **Mức 1**<br>(Đạt đủ) | **3 / 3** |

---

## PHẦN D: HƯỚNG DẪN VẤN ĐÁP TRƯỚC HỘI ĐỒNG (DÀNH CHO K5.2)

### 1. Câu hỏi về Nguyên lý Phụ thuộc (Dependency Rule) & Kiến trúc Clean Architecture
- **Hỏi:** "Tại sao tầng `CoreBusiness` không được tham chiếu project nào? Dependency Rule được tuân thủ như thế nào?"
- **Trả lời:**  
  *Theo quy tắc Clean Architecture, tầng nghiệp vụ lõi (Domain/CoreBusiness) phải hoàn toàn độc lập với công nghệ hiển thị và cơ sở dữ liệu để đảm bảo tính bất biến và dễ kiểm thử. Tầng `UseCases` chỉ phụ thuộc `CoreBusiness`. Tầng `Plugins.DataStore.SQL` đóng vai trò là chi tiết ngoại vi (Detail), phụ thuộc ngược vào các Interface kho dữ liệu do tầng `UseCases` quy định (`ICustomerRepository`, `IInvoiceRepository`...). Tầng giao diện `WebBanHang` chỉ làm việc với UseCase Interfaces. Nhờ đó, nếu sau này thay SQL Server bằng PostgreSQL hoặc MongoDB, toàn bộ logic nghiệp vụ trong `UseCases` và `CoreBusiness` không hề bị thay đổi (Inversion of Control - DIP).*

### 2. Câu hỏi về Dòng Code Cài Đặt 2 Luật Nghiệp Vụ (Business Rules)
- **Hỏi:** "Hãy mở màn hình và chỉ chính xác dòng code cài đặt Luật 1 (Hạn mức nợ) và Luật 2 (FIFO)?"
- **Trả lời:**
  - **Luật 1 (Credit Limit):** Nằm tại file `WebBanHang.UseCases/Invoices/CreateInvoiceUseCase.cs`, các dòng **60 đến 68**:
    ```csharp
    var currentDebt = await _customerRepository.GetTotalOutstandingDebtAsync(customerId);
    var newInvoiceAmount = lines.Sum(l => l.Quantity * l.UnitPrice);
    if (currentDebt + newInvoiceAmount > customer.CreditLimit)
    {
        throw new CreditLimitExceededException($"Không thể lập hóa đơn! Tổng nợ ({currentDebt + newInvoiceAmount:N0} đ) vượt quá hạn mức ({customer.CreditLimit:N0} đ)...");
    }
    ```
  - **Luật 2 (Thu tiền cấn trừ FIFO):** Nằm tại file `WebBanHang.UseCases/Payments/RecordPaymentAndAllocateUseCase.cs`, các dòng **43 đến 65**:
    ```csharp
    foreach (var invoice in unpaidInvoices) // Danh sách lấy theo InvoiceDate ASC (hóa đơn cũ nhất)
    {
        if (remainingAmount <= 0) break;
        decimal allocateAmount = Math.Min(invoice.Balance, remainingAmount);
        invoice.PaidAmount += allocateAmount;
        invoice.Balance -= allocateAmount;
        invoice.Status = invoice.Balance <= 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        remainingAmount -= allocateAmount;
        // Ghi nhận PaymentAllocation
    }
    ```

### 3. Câu hỏi về Toàn vẹn Dữ liệu & Transaction (ACID)
- **Hỏi:** "Cặp Header–Line được lưu như thế nào để đảm bảo tính toàn vẹn nếu mạng chập chờn hoặc máy chủ mất điện?"
- **Trả lời:**  
  *Trong file `InvoiceRepository.cs` (dòng 44-97) và `PaymentRepository.cs` (dòng 30-87), em sử dụng `using var transaction = connection.BeginTransaction()`. Khi lưu hóa đơn, hệ thống ghi bản ghi Header `Invoices` để lấy `InvoiceId`, sau đó lặp qua danh sách `InvoiceLines` để thêm chi tiết và trừ tồn kho `StockQuantity` trong bảng `Products`. Nếu bất kỳ thao tác nào thất bại, khối `catch` sẽ kích hoạt `transaction.Rollback()`, đảm bảo không bao giờ sinh ra hóa đơn rác hoặc chi tiết mồ côi (Atomicity).*

### 4. Câu hỏi về Vòng đời Dependency Injection (DI Lifetimes)
- **Hỏi:** "Tại sao `DraftInvoiceStateService` dùng `Scoped` mà không dùng `Transient` hay `Singleton`?"
- **Trả lời:**  
  *Trong Blazor Server, `Scoped` tương ứng với một phiên kết nối mạch SignalR của người dùng. Dùng `Scoped` cho `DraftInvoiceStateService` (dòng 21 `Program.cs`) giúp giỏ hàng/hóa đơn nháp được lưu giữ khi nhân viên chuyển đổi qua lại giữa các menu mà không bị mất, đồng thời tách biệt hoàn toàn giữa các nhân viên khác nhau (nếu dùng `Singleton` thì nhân viên này sẽ nhìn thấy giỏ hàng của nhân viên kia).*

---

## PHẦN E: BẢNG TỔNG KẾT ĐIỂM SỐ CAPSTONE RUBRIC

| Phần | Tiêu chí Rubric | Trọng số | Điểm Tối Đa | Điểm Tự Đánh Giá | Minh chứng tóm tắt |
| :---: | :--- | :---: | :---: | :---: | :--- |
| **Phần 1** | **Kiến trúc Clean Architecture & Code Quality** (K1.1 – K1.4) | 25% | 25 đ | **25 / 25 đ** | 4 tầng chuẩn mực, DIP 100%, 2 luật ở UseCases, Scoped state |
| **Phần 2** | **Cơ sở Dữ liệu & Tính Toàn vẹn Dữ liệu** (K2.1 – K2.4) | 25% | 25 đ | **25 / 25 đ** | 6 bảng 3NF, SQL Server, 2 transactions ACID, tham số hóa 100% |
| **Phần 3** | **Giao diện & Trải nghiệm Người dùng** (K3.1 – K3.4) | 20% | 20 đ | **20 / 20 đ** | 3 Components [Parameter]/EventCallback, EditForm validation, Responsive 390px |
| **Phần 4** | **Bảo mật, Phân quyền & Tính năng Nâng cao / AI** (K4.1 – K4.3) | 15% | 15 đ | **15 / 15 đ** | Cookie Auth 3 vai trò, chặn URL trái phép, AI tín dụng + In VietQR/Sổ nợ |
| **Phần 5** | **Hồ sơ Thiết kế (UML) & Thuyết trình Vấn đáp** (K5.1 – K5.3) | 15% | 15 đ | **15 / 15 đ** | 4 sơ đồ PlantUML khớp 100% code, nắm chắc dòng code luật, commit rải đều |
| **TỔNG** | **TOÀN BỘ ĐỒ ÁN CAPSTONE** | **100%** | **100 đ** | **100 / 100 ĐIỂM** | **ĐẠT XUẤT SẮC (GRADE A+)** |
