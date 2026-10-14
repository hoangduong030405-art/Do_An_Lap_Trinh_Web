using System;

namespace WebBanHang.Services
{
    public class LayoutStateService
    {
        public bool IsSidebarCollapsed { get; private set; } = false;
        public bool IsQuickAddOpen { get; private set; } = false;
        public string SelectedCompany { get; set; } = "CÔNG TY TNHH THƯƠNG MẠI KỸ THUẬT BÁCH KHOA";
        public string FiscalYear { get; set; } = "2024";

        public event Action? OnChange;

        public void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
            NotifyStateChanged();
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
