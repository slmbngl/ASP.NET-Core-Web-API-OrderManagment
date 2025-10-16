using OrderManagementApi.Models;
namespace OrderManagementApi.Interfaces
{
    public interface ICustomerRepository
    {
        
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer> GetCustomerByIdAsync(int id);
        Task<Customer> AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(int id);
         Task<bool> ExistsAsync(int id);
    }
}