// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

using Shared.Contracts.Orders;
using System.Net;
using System.Net.Http.Json;

namespace QuickApp.Core.Services.Shop
{
    public class OrdersServiceClient(HttpClient httpClient) : IOrdersService
    {
        public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int? customerId = null, string? cashierId = null,
            CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.GetAsync(WithFilter(OrderRoutes.Base, customerId, cashierId),
                cancellationToken);
            return await ReadAsync<List<OrderDto>>(response, cancellationToken);
        }

        public async Task<int> CountOrdersAsync(int? customerId = null, string? cashierId = null,
            CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.GetAsync(WithFilter(OrderRoutes.Count, customerId, cashierId),
                cancellationToken);
            return await ReadAsync<int>(response, cancellationToken);
        }

        public async Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.GetAsync(OrderRoutes.ById(id), cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return await ReadAsync<OrderDto>(response, cancellationToken);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.PostAsJsonAsync(OrderRoutes.Base, request, cancellationToken);
            return await ReadAsync<OrderDto>(response, cancellationToken);
        }

        public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.PutAsJsonAsync(OrderRoutes.ById(id), request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return await ReadAsync<OrderDto>(response, cancellationToken);
        }

        public async Task<bool> DeleteOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.DeleteAsync(OrderRoutes.ById(id), cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;

            await EnsureSuccessAsync(response, cancellationToken);
            return true;
        }

        private static string WithFilter(string path, int? customerId, string? cashierId)
        {
            var query = new List<string>();

            if (customerId is not null)
                query.Add($"customerId={customerId}");

            if (!string.IsNullOrEmpty(cashierId))
                query.Add($"cashierId={Uri.EscapeDataString(cashierId)}");

            return query.Count == 0 ? path : $"{path}?{string.Join('&', query)}";
        }

        private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await EnsureSuccessAsync(response, cancellationToken);

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new OrderServiceException($"order-service returned an empty body for {response.RequestMessage?.RequestUri}");
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new OrderServiceException(
                $"order-service returned {(int)response.StatusCode} for {response.RequestMessage?.Method} " +
                $"{response.RequestMessage?.RequestUri}: {body}", response.StatusCode, body);
        }
    }
}
