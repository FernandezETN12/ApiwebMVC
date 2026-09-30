using Microsoft.AspNetCore.Mvc;
using ApiwebMVC.Models;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

public class MenuController : Controller
{
    private readonly IPizzaService _pizzas;

    public MenuController(IPizzaService pizzas)
    {
        _pizzas = pizzas;
    }

    public async Task<IActionResult> Index(string? categoria)
    {
        var pizzas = await _pizzas.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(categoria))
        {
            pizzas = pizzas.Where(pizza => string.Equals(
                pizza.Categoria, categoria, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View(pizzas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int pizzaId)
    {
        var pizza = await _pizzas.GetByIdAsync(pizzaId);
        if (pizza == null)
        {
            return NotFound();
        }

        var cart = CarritoSession.Get(HttpContext);
        cart[pizzaId] = Math.Min(cart.GetValueOrDefault(pizzaId) + 1, 99);
        CarritoSession.Save(HttpContext, cart);

        return RedirectToAction("Index", "Pedidos");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var pizza = await _pizzas.GetByIdAsync(id);
        if (pizza == null)
        {
            return NotFound();
        }

        return View(new MenuPizzaEditViewModel
        {
            Id = pizza.Id,
            Nombre = pizza.Nombre,
            Descripcion = pizza.Descripcion,
            Precio = pizza.Precio,
            IngredientesTexto = string.Join(", ", pizza.Ingredientes ?? new List<string>()),
            Categoria = pizza.Categoria,
            ImagenURL = pizza.ImagenURL
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MenuPizzaEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var pizza = new Pizza
        {
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Precio = model.Precio,
            Ingredientes = model.IngredientesTexto?
                .Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Categoria = model.Categoria,
            ImagenURL = model.ImagenURL
        };

        if (!await _pizzas.UpdateAsync(id, pizza))
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "La pizza se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
