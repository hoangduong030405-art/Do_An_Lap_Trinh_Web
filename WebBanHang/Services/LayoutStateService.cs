using System;

namespace WebBanHang.Services
{
    public class LayoutStateService
    {
        public bool IsSidebarCollapsed { get; private set; } = false;
        public bool IsSidebarPinned { get; private set; } = false;
        public bool IsQuickAddOpen { get; private set; } = false;
        public bool IsRoleModalOpen { get; set; } = false;
        public string SelectedCompany { get; set; } = "TỔNG KHO PHÂN PHỐI SỈ & CÔNG NỢ B2B";
        public string FiscalYear { get; set; } = "2024";
        public string SelectedPeriod { get; private set; } = "this_month";

        public void SetSelectedPeriod(string period)
        {
            if (SelectedPeriod != period)
            {
                SelectedPeriod = period;
                NotifyStateChanged();
            }
        }

        public void ToggleRoleModal()
        {
            IsRoleModalOpen = !IsRoleModalOpen;
            NotifyStateChanged();
        }

        public event Action? OnChange;

        public void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
            NotifyStateChanged();
        }

        public void ToggleSidebarPinned()
        {
            IsSidebarPinned = !IsSidebarPinned;
            NotifyStateChanged();
        }

        public void SetSidebarPinned(bool pinned)
        {
            if (IsSidebarPinned != pinned)
            {
                IsSidebarPinned = pinned;
                NotifyStateChanged();
            }
        }

        public void SetSidebarCollapsed(bool collapsed)
        {
            if (IsSidebarCollapsed != collapsed)
            {
                IsSidebarCollapsed = collapsed;
                NotifyStateChanged();
            }
        }

        public void ToggleQuickAdd()
        {
            IsQuickAddOpen = !IsQuickAddOpen;
            NotifyStateChanged();
        }

        public void CloseQuickAdd()
        {
            if (IsQuickAddOpen)
            {
                IsQuickAddOpen = false;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
