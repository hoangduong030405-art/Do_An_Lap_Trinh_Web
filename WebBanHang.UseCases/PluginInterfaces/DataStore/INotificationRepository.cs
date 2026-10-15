using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.PluginInterfaces.DataStore
{
    public interface INotificationRepository
    {
        Task<IEnumerable<NotificationItemDto>> GetNotificationsAsync(string role, int? customerId = null);
    }
}
