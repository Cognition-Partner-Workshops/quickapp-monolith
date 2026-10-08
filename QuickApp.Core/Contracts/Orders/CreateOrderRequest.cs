// Vendored copy of quickapp-microservices: src/Shared/Shared.Contracts/Orders/CreateOrderRequest.cs
// Keep in sync with the order-service contract (same namespace so it can be swapped for a package reference).

using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.Orders;

public sealed record CreateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; init; }

    [StringLength(450)]
    public string? CashierId { get; init; }

    [Range(0, double.MaxValue)]
    public decimal Discount { get; init; }

    [StringLength(500)]
    public string? Comments { get; init; }

    [Required, MinLength(1)]
    public IReadOnlyList<CreateOrderItemRequest> Items { get; init; } = [];
}

public sealed record CreateOrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    [Range(0, double.MaxValue)]
    public decimal Discount { get; init; }
}
