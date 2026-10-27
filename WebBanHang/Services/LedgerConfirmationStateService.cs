using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WebBanHang.Services
{
    public class CustomerLedgerConfirmation
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal EndingBalance { get; set; }
        public DateTime ConfirmedAt { get; set; }
    }

    public class CustomerLedgerDispute
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
    }

    public enum DebtMessageType
    {
        Dispute,              // Báo lệch số liệu
        LimitOverrideRequest, // Xin duyệt vượt hạn mức
        BalanceConfirmation,  // Khớp số dư
        DebtReminderSent      // Nhật ký AI Nhắc nợ
    }

    public class DebtMessageItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DebtMessageType Type { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string ActionUrl { get; set; } = "/customer-ledger";
    }

    public class LedgerConfirmationStateService
    {
        private readonly ConcurrentDictionary<int, CustomerLedgerConfirmation> _confirmations = new();
        private readonly List<CustomerLedgerDispute> _disputes = new();
        private readonly List<DebtMessageItem> _messages = new();

        public event Action? OnChange;

        public LedgerConfirmationStateService()
        {
            // Initial state is clean. Real messages will appear when events occur in the system.
        }

        public void SimulateSampleDispute()
        {
            AddDispute(2, "Siêu Thị Mini Tuấn Hương", "HD-2026-002", "Đại lý báo đã thanh toán 3.500.000đ qua ACB ngày 15/10 nhưng chưa thấy cấn trừ trên sổ đối soát.");
        }

        public void ResolveDispute(int customerId, string invoiceNumber)
        {
            lock (_disputes)
            {
                _disputes.RemoveAll(d => d.CustomerId == customerId && (string.IsNullOrEmpty(invoiceNumber) || d.InvoiceNumber == invoiceNumber));
            }
            lock (_messages)
            {
                var msg = _messages.FirstOrDefault(m => m.CustomerId == customerId && m.Type == DebtMessageType.Dispute);
                if (msg != null)
                {
                    msg.IsRead = true;
                }
            }
            OnChange?.Invoke();
        }

        public void ConfirmBalance(int customerId, string customerName, decimal balance)
        {
            _confirmations[customerId] = new CustomerLedgerConfirmation
            {
                CustomerId = customerId,
                CustomerName = customerName,
                EndingBalance = balance,
                ConfirmedAt = DateTime.Now
            };

            lock (_messages)
            {
                _messages.Insert(0, new DebtMessageItem
                {
                    Type = DebtMessageType.BalanceConfirmation,
                    CustomerId = customerId,
                    CustomerName = customerName,
                    Title = $"Xác nhận đối soát: {customerName} đã chốt số dư {balance:N0} đ",
                    Content = $"Biên bản đối soát công nợ đã được đại lý ký duyệt điện tử lúc {DateTime.Now:HH:mm dd/MM/yyyy}.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    ActionUrl = $"/customer-ledger?customerId={customerId}"
                });
            }

            OnChange?.Invoke();
        }

        public CustomerLedgerConfirmation? GetConfirmation(int customerId)
        {
            _confirmations.TryGetValue(customerId, out var confirmation);
            return confirmation;
        }

        public void AddDispute(int customerId, string customerName, string invoiceNumber, string note)
        {
            lock (_disputes)
            {
                _disputes.Add(new CustomerLedgerDispute
                {
                    CustomerId = customerId,
                    CustomerName = customerName,
                    InvoiceNumber = invoiceNumber,
                    Note = note,
                    ReportedAt = DateTime.Now
                });
            }

            lock (_messages)
            {
                _messages.Insert(0, new DebtMessageItem
                {
                    Type = DebtMessageType.Dispute,
                    CustomerId = customerId,
                    CustomerName = customerName,
                    Title = $"Báo lệch số liệu: {customerName} (HĐ #{invoiceNumber})",
                    Content = note,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    ActionUrl = $"/customer-ledger?customerId={customerId}"
                });
            }

            OnChange?.Invoke();
        }

        public void AddLimitOverrideRequest(int customerId, string customerName, decimal cartTotal, decimal availableLimit)
        {
            lock (_messages)
            {
                _messages.Insert(0, new DebtMessageItem
                {
                    Type = DebtMessageType.LimitOverrideRequest,
                    CustomerId = customerId,
                    CustomerName = customerName,
                    Title = $"Yêu cầu duyệt vượt hạn mức: {customerName}",
                    Content = $"Đơn hàng giá trị {cartTotal:N0} đ vượt quá trần khả dụng ({availableLimit:N0} đ). Đại lý xin phê duyệt tạm thời.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    ActionUrl = "/manage-customers"
                });
            }
            OnChange?.Invoke();
        }

        public void AddReminderSent(int customerId, string customerName, string tone, string content)
        {
            lock (_messages)
            {
                _messages.Insert(0, new DebtMessageItem
                {
                    Type = DebtMessageType.DebtReminderSent,
                    CustomerId = customerId,
                    CustomerName = customerName,
                    Title = $"Nhật ký AI Nhắc nợ ({tone}): Đã gửi cho {customerName}",
                    Content = content.Length > 120 ? content.Substring(0, 120) + "..." : content,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    ActionUrl = "/manage-customers"
                });
            }
            OnChange?.Invoke();
        }

        public List<CustomerLedgerDispute> GetDisputes(int customerId)
        {
            lock (_disputes)
            {
                return _disputes.FindAll(d => d.CustomerId == customerId);
            }
        }

        public List<DebtMessageItem> GetMessages()
        {
            lock (_messages)
            {
                return _messages.ToList();
            }
        }

        public int GetUnreadMessageCount()
        {
            lock (_messages)
            {
                return _messages.Count(m => !m.IsRead);
            }
        }

        public void MarkAllAsRead()
        {
            lock (_messages)
            {
                foreach (var m in _messages)
                {
                    m.IsRead = true;
                }
            }
            OnChange?.Invoke();
        }

        public void MarkAsRead(string messageId)
        {
            lock (_messages)
            {
                var msg = _messages.FirstOrDefault(m => m.Id == messageId);
                if (msg != null) msg.IsRead = true;
            }
            OnChange?.Invoke();
        }
    }
}
