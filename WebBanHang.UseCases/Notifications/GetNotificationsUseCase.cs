using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Notifications
{
    public interface IGetNotificationsUseCase
    {
        Task<IEnumerable<NotificationItemDto>> ExecuteAsync(string role, int? customerId = null);
    }

    public class GetNotificationsUseCase : IGetNotificationsUseCase
    {
        private readonly INotificationRepository _repository;

        public GetNotificationsUseCase(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NotificationItemDto>> ExecuteAsync(string role, int? customerId = null)
        {
            return await _repository.GetNotificationsAsync(role, customerId);
        }
    }
}
