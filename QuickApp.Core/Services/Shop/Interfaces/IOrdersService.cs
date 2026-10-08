// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

using Shared.Contracts.Orders;

namespace QuickApp.Core.Services.Shop
{
    /// <summary>
    /// Orders are owned by the external order-service. Implementations talk to it over HTTP.
    /// </summary>
    public interface IOrdersService
    {
        Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int? customerId = null, string? cashierId = null,
            CancellationToken cancellationToken = default);

        Task<int> CountOrdersAsync(int? customerId = null, string? cashierId = null,
            CancellationToken cancellationToken = default);

        Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default);

        Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

        Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteOrderAsync(int id, CancellationToken cancellationToken = default);
    }
}
