using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.DTOs;
using OrderManagementApi.Interfaces;
using OrderManagementApi.Models;
using OrderManagementApi.Repositories;

namespace OrderManagementApi.Controller
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrders()
        {
            var orders = await _orderRepository.GetOrdersByApplicationUserIdAsync();
            if (!orders.Any())
                return NoContent();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto orderDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _orderRepository.CreateOrderAsync(orderDto);

            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                await _orderRepository.UpdateOrderAsync(id, statusDto.Status);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                await _orderRepository.DeleteOrderWithStockReturnAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}