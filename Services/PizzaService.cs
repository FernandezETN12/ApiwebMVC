using ApiwebMVC.Data;
using ApiwebMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiwebMVC.Services;

public class PizzaService : IPizzaService
{
    private readonly ApplicationDbContext _db;

    public PizzaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Pizza>> GetAllAsync()
    {
        return await _db.Pizzas.AsNoTracking().ToListAsync();
    }

    public async Task<Pizza?> GetByIdAsync(int id)
    {
        return await _db.Pizzas.FindAsync(id);
    }

    public async Task<Pizza> CreateAsync(Pizza pizza)
    {
        pizza.FechaCreacion = DateTime.UtcNow;
        _db.Pizzas.Add(pizza);
        await _db.SaveChangesAsync();
        return pizza;
    }

    public async Task<bool> UpdateAsync(int id, Pizza pizza)
    {
        var existing = await _db.Pizzas.FindAsync(id);
        if (existing == null) return false;
        existing.Nombre = pizza.Nombre;
        existing.Descripcion = pizza.Descripcion;
        existing.Precio = pizza.Precio;
        existing.Ingredientes = pizza.Ingredientes;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Pizzas.FindAsync(id);
        if (existing == null) return false;
        _db.Pizzas.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
