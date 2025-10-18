using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.DTOs;
using OrderManagementApi.Interfaces;
using OrderManagementApi.Models;

namespace OrderManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        // Constructor'da sadece Order Repository'yi alıyoruz
        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return Ok(await _orderRepository.GetAllOrdersAsync());
        }

        // GET: api/Orders/5
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

        // POST: api/Orders (Temizlenmiş)
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> PostOrder([FromBody] CreateOrderDto orderDto)
        {
            try
            {
                // Tüm iş mantığı artık Repository içinde. Controller sadece çağırır.
                var createdOrder = await _orderRepository.CreateOrderAsync(orderDto);
                var responseDto = new OrderResponseDto
                {
                    Id = createdOrder.Id,
                    OrderDate = createdOrder.OrderDate,
                    Status = createdOrder.Status,
                    TotalAmount = createdOrder.TotalAmount,
                    CustomerId = createdOrder.CustomerId,
                    CustomerFullName = createdOrder.Customer?.FirstName + " " + createdOrder.Customer?.LastName, // Customer'ı yüklediyseniz
                    Items = createdOrder.OrderItems.Select(item => new OrderItemResponseDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name, // Product'ın da yüklenmiş olması gerekir
                        Quantity = item.Quantity,
                        UnitPriceSnapshot = item.UnitPrice
                    }).ToList()
                };
                return CreatedAtAction(nameof(GetOrder), new { id = responseDto.Id }, responseDto);
            }
            catch (ArgumentException ex)
            {
                // Geçersiz ID veya bulunamayan ürün
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Yetersiz stok
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut("{id}")]
    public async Task<IActionResult> PutOrder(int id, [FromBody] UpdateOrderStatusDto statusDto)
    {
        try
        {
            // Tüm iş mantığını Repository'ye devret (Bu basit hali Repository'de)
            await _orderRepository.UpdateOrderAsync(id, statusDto.Status);
            
            // Başarılı güncelleme
            return NoContent(); 
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            // Ek iş mantığı hatalarını da buradan yakalayabiliriz (Örn: Geçersiz durum)
            // Şu anki basit Repository'de bu yok, ama mantık budur.
            return BadRequest(ex.Message); 
        }
    }


        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                // Silme ve stok iadesi işi Repository'de
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