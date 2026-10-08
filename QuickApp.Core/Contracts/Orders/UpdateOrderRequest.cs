// Vendored copy of quickapp-microservices: src/Shared/Shared.Contracts/Orders/UpdateOrderRequest.cs
// Keep in sync with the order-service contract (same namespace so it can be swapped for a package reference).

using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.Orders;

public sealed record UpdateOrderRequest
{
    /// <summary>Must not exceed the order's line-total sum; checked by order-service against stored items.</summary>
    [Range(0, double.MaxValue), MaxDecimalPlaces(2)]
    public decimal Discount { get; init; }

    [StringLength(500)]
    public string? Comments { get; init; }
}
