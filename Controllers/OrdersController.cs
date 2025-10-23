using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.DTOs;
using OrderManagementApi.Interfaces;
using OrderManagementApi.Models;

namespace OrderManagementApi.Controller
{   [Authorize]
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
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
           var orders = await _orderRepository.GetAllOrdersAsync();
    
    // NOT: Bu DTO dönüşümünü burada veya Repository'de yapmalısınız.
    // Şimdilik Controller'da yapalım:
    var dtos = orders.Select(o => new OrderResponseDto 
    {
        Id = o.Id,
        OrderDate = o.OrderDate,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        CustomerId = o.CustomerId,
        // İlişkileri güvenli bir şekilde aktar
        CustomerFullName = o.Customer?.FirstName + " " + o.Customer?.LastName, 
        Items = o.OrderItems.Select(oi => new OrderItemResponseDto {
             ProductId = oi.ProductId,
             ProductName = oi.Product?.Name ?? "Bilinmiyor",
             Quantity = oi.Quantity,
             UnitPriceSnapshot = oi.UnitPrice
        }).ToList()
    }).ToList();

    return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            // NOT: Eğer GetOrderByIdAsync DTO dönüşümü yapıyorsa, burası da DTO döndürmelidir.
            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> PostOrder([FromBody] CreateOrderDto orderDto)
        {
            try
            {
                var createdOrder = await _orderRepository.CreateOrderAsync(orderDto);

                // **DTO DÖNÜŞÜMÜ (Artık güvenli çalışmalı)**
                var responseDto = new OrderResponseDto
                {
                    Id = createdOrder.Id,
                    OrderDate = createdOrder.OrderDate,
                    Status = createdOrder.Status,
                    TotalAmount = createdOrder.TotalAmount,
                    CustomerId = createdOrder.CustomerId,
                    CustomerFullName = createdOrder.Customer.FirstName + " " + createdOrder.Customer.LastName,
                    Items = createdOrder.OrderItems.Select(item => new OrderItemResponseDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPriceSnapshot = item.UnitPrice // Modeldeki doğru alan adı kullanıldı
                    }).ToList()
                };
                return CreatedAtAction(nameof(GetOrder), new { id = responseDto.Id }, responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
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