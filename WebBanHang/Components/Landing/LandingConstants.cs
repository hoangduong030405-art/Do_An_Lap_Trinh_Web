namespace WebBanHang.Components.Landing
{
    /// <summary>
    /// Nguồn hằng số duy nhất (Single Source of Truth) cho các số liệu minh họa trên Landing Page.
    /// Đảm bảo tính nhất quán giữa Hero Mockup, Aging Demo và AI Copilot Section.
    /// </summary>
    public static class LandingConstants
    {
        // 1. NHÃN BẢN QUYỀN & MÔ TẢ HỆ THỐNG
        public const string PlatformName = "HUEGO B2B PLATFORM";
        public const string SubTitle = "Nền Tảng Phân Phối Sỉ & Quản Trị Tín Dụng Thương Mại";
        public const string CourseCode = "ECO2415";
        public const string TopicName = "Đề tài 18: Bán hàng & Quản trị Công nợ B2B (Sales & AR)";

        // 2. SỐ LIỆU MINH HỌA ĐỒNG NHẤT CHO HERO VÀ DEMO CÔNG NỢ
        public static class MockAging
        {
            public const decimal TotalOutstandingDebt = 100_000_000m; // 100 Trđ
            
            // 4 xô tuổi nợ khớp 100% tổng nợ
            public const decimal Bucket1Amount = 55_000_000m; // 55%
            public const int Bucket1Percent = 55;
            public const string Bucket1Label = "0 - 30 Ngày";
            public const string Bucket1Status = "Trong hạn thanh toán";

            public const decimal Bucket2Amount = 25_000_000m; // 25%
            public const int Bucket2Percent = 25;
            public const string Bucket2Label = "31 - 60 Ngày";
            public const string Bucket2Status = "Quá hạn nhẹ";

            public const decimal Bucket3Amount = 12_000_000m; // 12%
            public const int Bucket3Percent = 12;
            public const string Bucket3Label = "61 - 90 Ngày";
            public const string Bucket3Status = "Cần cảnh báo nợ";

            public const decimal Bucket4Amount = 8_000_000m; // 8%
            public const int Bucket4Percent = 8;
            public const string Bucket4Label = "> 90 Ngày";
            public const string Bucket4Status = "Nợ xấu khó đòi";

            public const bool IsIntegrityValid = true; // 55 + 25 + 12 + 8 = 100
        }

        // 3. THẺ MOCKUP HERO NỔI (FLOATING CARDS)
        public static class HeroMockup
        {
            public const string TotalCreditAllocated = "200.0 Trđ";
            public const string DemoDebtCollectionRate = "98.2%";
            
            // Thẻ phụ 1: Cảnh báo AVA AI
            public const string AvaAlertCustomer = "Tạp Hóa Bà Ba";
            public const string AvaAlertMsg = "Còn 3 ngày đến hạn hóa đơn #001 (15.000.000 đ)";
            
            // Thẻ phụ 2: Phiếu thu cấn trừ FIFO
            public const string FifoReceiptNo = "PT-2026-088";
            public const string FifoReceiptAmount = "5.000.000 đ";
            public const string FifoReceiptDesc = "Đã cấn trừ tự động vào HĐ #001 (hóa đơn phát sinh sớm nhất)";

            // Thẻ phụ 3: Hạng tín nhiệm đối tác
            public const string CreditScore = "86 / 100";
            public const string CreditGrade = "Hạng A (Rất Uy Tín)";
        }

        // 4. THÔNG ĐIỆP GIÁ TRỊ (VALUE PROPOSITIONS) THAY THẾ CHO SỐ NHỎ
        public static class ValueProps
        {
            public const string AcidTransactions = "100% Giao Dịch ACID";
            public const string FourAgingBuckets = "4 Xô Tuổi Nợ Chuẩn Mực";
            public const string ThreeRolesSecurity = "3 Vai Trò Phân Quyền";
            public const string ZeroManualFifo = "0 Lần Nhập Tay Khi Cấn Trừ";
            public const string NoDataYet = "Chưa có dữ liệu";
        }
    }
}
