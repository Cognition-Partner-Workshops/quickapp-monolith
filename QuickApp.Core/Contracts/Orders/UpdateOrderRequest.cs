// Vendored copy of quickapp-microservices: src/Shared/Shared.Contracts/Orders/UpdateOrderRequest.cs
// Keep in sync with the order-service contract (same namespace so it can be swapped for a package reference).

using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.Orders;

public sealed record UpdateOrderRequest
{
    [Range(0, double.MaxValue)]
    public decimal Discount { get; init; }

    [StringLength(500)]
    public string? Comments { get; init; }
}
