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

    public async Task<IActionResult> Index()
    {
        var pizzas = await _pizzas.GetAllAsync();
        return View(pizzas);
    }
}
