using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Models;
using OrderManagementApi.DTOs;

namespace OrderManagementApi.Controllers
{
    [Route("api/[controller]")] // Endpoint: /api/Customers
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        // take DbContext via Dependency Injection
        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Customers
        // list all customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            // Veritabanından Customer tablosundaki tüm kayıtları asenkron olarak çeker.
            return await _context.Customers.ToListAsync();
        }

        // GET: api/Customers/5
        // Get a customer by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            // Find the customer by ID.
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                // if not found, return 404 Not Found.
                return NotFound();
            }

            // If found, return 200 OK with the customer.
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
            // add the new customer to the DbContext.
            _context.Customers.Add(customer);
            // Save changes to the database.
            await _context.SaveChangesAsync();

            // Return the URL of the created resource (GetCustomer method) (201 Created).
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        // DELETE: api/Customers
        // Delete a customer.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            // Find the customer by ID.
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                // if not found, return 404 Not Found.
                return NotFound();
            }

            // Remove the customer from the DbContext.
            _context.Customers.Remove(customer);
            // Save changes to the database.
            await _context.SaveChangesAsync();

            // Return 204 No Content after successful deletion.
            return NoContent();
        }
    }
}