using ApiwebMVC.Models;
namespace ApiwebMVC.Services;

public interface IPizzaService
{
    Task<IEnumerable<Pizza>> GetAllAsync();
    Task<Pizza?> GetByIdAsync(int id);
    Task<Pizza> CreateAsync(Pizza pizza);
    Task<bool> UpdateAsync(int id, Pizza pizza);
    Task<bool> DeleteAsync(int id);
}
