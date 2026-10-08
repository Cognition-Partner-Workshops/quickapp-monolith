// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickApp.Core.Services.Shop;
using QuickApp.Server.Attributes;
using QuickApp.Server.ViewModels.Shop;
using Shared.Contracts.Orders;

namespace QuickApp.Server.Controllers
{
    /// <summary>
    /// Facade over order-service so the Angular client keeps calling the monolith.
    /// </summary>
    [Route("api/orders")]
    [Authorize]
    [OrderServiceUnavailable]
    public class OrdersController(ILogger<OrdersController> logger, IMapper mapper, IOrdersService ordersService)
        : BaseApiController(logger, mapper)
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<OrderVM>))]
        public async Task<IActionResult> GetOrders([FromQuery] int? customerId, [FromQuery] string? cashierId,
            CancellationToken cancellationToken)
        {
            var orders = await ordersService.GetOrdersAsync(customerId, cashierId, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<OrderVM>>(orders));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(200, Type = typeof(OrderVM))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetOrder(int id, CancellationToken cancellationToken)
        {
            var order = await ordersService.GetOrderAsync(id, cancellationToken);
            return order == null ? NotFound(id) : Ok(_mapper.Map<OrderVM>(order));
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(OrderVM))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderVM order, CancellationToken cancellationToken)
        {
            var request = _mapper.Map<CreateOrderRequest>(order) with { CashierId = GetCurrentUserId() };
            var created = await ordersService.CreateOrderAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, _mapper.Map<OrderVM>(created));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(200, Type = typeof(OrderVM))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderVM order,
            CancellationToken cancellationToken)
        {
            var updated = await ordersService.UpdateOrderAsync(id, _mapper.Map<UpdateOrderRequest>(order),
                cancellationToken);
            return updated == null ? NotFound(id) : Ok(_mapper.Map<OrderVM>(updated));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken)
        {
            return await ordersService.DeleteOrderAsync(id, cancellationToken) ? NoContent() : NotFound(id);
        }
    }
}
