using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Interfaces;
using OrderManagementApi.DTOs;
using OrderManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OrderManagementApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemRepository _itemRepository;

        public OrderItemsController(IOrderItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        // GET: api/OrderItems/ByOrder/5
        // Get all order's item by specific order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var items = await _itemRepository.GetItemsByApplicationUserIdAsync(userId);

            if (!items.Any())
                return NoContent();

            return Ok(items);
        }
        [HttpGet("ByOrder/{orderId}")]
        public async Task<ActionResult<IEnumerable<OrderItemResponseDto>>> GetOrderItems(int orderId)
        {
            var items = await _itemRepository.GetItemsByOrderIdAsync(orderId);

            // convert to DTO
            var dtos = items.Select(item => new OrderItemResponseDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Bilinmiyor", // Product nesnesi yüklendiyse adını al
                Quantity = item.Quantity,
                UnitPriceSnapshot = item.UnitPrice
            }).ToList();

            return Ok(dtos);
        }

        // GET: api/OrderItems/12
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemResponseDto>> GetOrderItem(int id)
        {
            var item = await _itemRepository.GetItemByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            // DTO'ya dönüştürme
            var dto = new OrderItemResponseDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Bilinmiyor",
                Quantity = item.Quantity,
                UnitPriceSnapshot = item.UnitPrice
            };

            return Ok(dto);
        }
    }
}