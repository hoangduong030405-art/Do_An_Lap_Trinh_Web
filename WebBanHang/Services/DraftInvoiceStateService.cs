using System;
using System.Collections.Generic;
using System.Linq;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.Services
{
    public class DraftInvoiceItem
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
        public decimal LineCost => Quantity * CostPrice;
        public decimal LineProfit => LineTotal - LineCost;
        public decimal ProfitMargin => UnitPrice > 0 ? Math.Round(((UnitPrice - CostPrice) / UnitPrice) * 100, 1) : 0;
    }

    public class DraftInvoiceStateService
    {
        public int? SelectedCustomerId { get; private set; }
        public Customer? SelectedCustomer { get; private set; }
        public List<DraftInvoiceItem> Items { get; private set; } = new();

        public event Action? OnChange;

        public decimal TotalAmount => Items.Sum(i => i.LineTotal);
        public decimal TotalCost => Items.Sum(i => i.LineCost);
        public decimal TotalProfit => TotalAmount - TotalCost;
        public decimal ProfitMargin => TotalAmount > 0 ? Math.Round((TotalProfit / TotalAmount) * 100, 1) : 0;
        public int TotalItemCount => Items.Sum(i => i.Quantity);

        public void SetCustomer(Customer customer)
        {
            SelectedCustomerId = customer?.Id;
            SelectedCustomer = customer;
            NotifyStateChanged();
        }

        public void AddProduct(Product product, int qty = 1)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += qty;
            }
            else
            {
                Items.Add(new DraftInvoiceItem
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.Name,
                    Unit = product.Unit,
                    UnitPrice = product.UnitPrice,
                    CostPrice = product.CostPrice,
                    Quantity = qty
                });
            }
            NotifyStateChanged();
        }

        public void UpdateQuantity(int productId, int newQty)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = newQty;
                if (item.Quantity <= 0)
                {
                    Items.Remove(item);
                }
                NotifyStateChanged();
            }
        }

        public void RemoveProduct(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                Items.Remove(item);
                NotifyStateChanged();
            }
        }

        public void Clear()
        {
            SelectedCustomerId = null;
            SelectedCustomer = null;
            Items.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
