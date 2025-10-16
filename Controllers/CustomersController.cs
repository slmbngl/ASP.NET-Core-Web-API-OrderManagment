using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Models;
using OrderManagementApi.DTOs;
using OrderManagementApi.Interfaces;

namespace OrderManagementApi.Controllers
{
    [Route("api/[controller]")] // Endpoint: /api/Customers
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;


        public CustomersController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        // GET: api/Customers
        // list all customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            // Veritabanından Customer tablosundaki tüm kayıtları asenkron olarak çeker.
            return Ok(await _customerRepository.GetAllCustomersAsync());
        }

        // GET: api/Customers/5
        // Get a customer by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            // Find the customer by ID.
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return customer;
            
        }

        // POST: api/Customers
        // Create a new customer.
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(CreateCustomerDto customerDto)
        {
            // Map DTO to EF Core model
            var customer = new Customer
            {
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email
            };
            var createdCustomer = await _customerRepository.AddCustomerAsync(customer); // Repository metodu kullanıldı


            // Return the URL of the created resource (GetCustomer method) (201 Created).
            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
        }

        // DELETE: api/Customers
        // Delete a customer.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var exists = await _customerRepository.ExistsAsync(id);
            if (!exists)
             {
                 return NotFound();
             }

            await _customerRepository.DeleteCustomerAsync(id);
            return NoContent();
        }
    }
}