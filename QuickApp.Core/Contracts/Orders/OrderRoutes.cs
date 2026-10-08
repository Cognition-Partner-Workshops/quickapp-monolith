// Vendored copy of quickapp-microservices: src/Shared/Shared.Contracts/Orders/OrderRoutes.cs
// Keep in sync with the order-service contract (same namespace so it can be swapped for a package reference).

namespace Shared.Contracts.Orders;

/// <summary>
/// HTTP routes exposed by order-service. Shared so callers and the service cannot drift.
/// </summary>
public static class OrderRoutes
{
    public const string Base = "api/orders";
    public const string Count = Base + "/count";

    /// <summary>Header carrying the shared key that callers must present to order-service.</summary>
    public const string ApiKeyHeader = "X-Internal-Api-Key";

    public static string ById(int id) => $"{Base}/{id}";
}
