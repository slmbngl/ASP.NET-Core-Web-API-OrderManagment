using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Interfaces; 
using OrderManagementApi.Models;
using OrderManagementApi.DTOs;

namespace OrderManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository; 
        
        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return Ok(await _productRepository.GetAllProductsAsync()); 
        }
        
        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id); 
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(CreateProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity
            };
            
            var createdProduct = await _productRepository.AddProductAsync(product); 

            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }
        
        // PUT: api/Product/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, CreateProductDto productDto)
        {
            if (id != productDto.Id)
            {
                 return BadRequest("ID uyuşmazlığı.");
            }

            var productToUpdate = await _productRepository.GetProductByIdAsync(id);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            
            productToUpdate.Name = productDto.Name;
            productToUpdate.Price = productDto.Price;
            productToUpdate.StockQuantity = productDto.StockQuantity;

            
            await _productRepository.UpdateProductAsync(productToUpdate); 
            
            return NoContent();
        }
        
        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
             var exists = await _productRepository.ExistsAsync(id);
             if (!exists)
             {
                 return NotFound();
             }

             await _productRepository.DeleteProductAsync(id); 
             return NoContent();
        }
    }   
}