using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Interfaces;
using OrderManagementApi.DTOs;
using OrderManagementApi.Models;

namespace OrderManagementApi.Controllers
{
    [Route("api/[controller]")] // Genellikle /api/OrderItems
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemRepository _itemRepository;

        public OrderItemsController(IOrderItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        // GET: api/OrderItems/ByOrder/5
        // Belirli bir siparişe ait tüm kalemleri getirir
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
        {
            var items = await _itemRepository.GetOrderIdAsync();
            
            // // DTO'ya dönüştürme (Sonsuz döngüden kaçınmak için kritik)
            // var dtos = items.Select(item => new OrderItemResponseDto
            // {
            //     ProductId = item.ProductId,
            //     ProductName = item.Product?.Name ?? "Bilinmiyor", // Product nesnesi yüklendiyse adını al
            //     Quantity = item.Quantity,
            //     UnitPriceSnapshot = item.UnitPrice
            // }).ToList();

            return Ok(items);
        }
        [HttpGet("ByOrder/{orderId}")]
        public async Task<ActionResult<IEnumerable<OrderItemResponseDto>>> GetOrderItems(int orderId)
        {
            var items = await _itemRepository.GetItemsByOrderIdAsync(orderId);
            
            // DTO'ya dönüştürme (Sonsuz döngüden kaçınmak için kritik)
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
        // Belirli bir kalemi ID'si ile getirir
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
        
        // NOT: POST, PUT ve DELETE işlemleri, stok yönetimi ve Transaction bütünlüğü nedeniyle,
        // YALNIZCA OrderController/OrderRepository üzerinden yapılmalıdır.
        // Aksi takdirde stokları yönetmek çok zorlaşır.
    }
}