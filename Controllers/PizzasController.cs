using Microsoft.AspNetCore.Mvc;
using ApiwebMVC.Models;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzasController : ControllerBase
{
    private readonly IPizzaService _service;

    public PizzasController(IPizzaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pizza>>> GetPizzas()
    {
        var pizzas = await _service.GetAllAsync();
        return Ok(pizzas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Pizza>> GetPizza(int id)
    {
        var pizza = await _service.GetByIdAsync(id);
        if (pizza == null) return NotFound();
        return Ok(pizza);
    }

    [HttpPost]
    public async Task<ActionResult<Pizza>> CreatePizza([FromBody] Pizza pizza)
    {
        var created = await _service.CreateAsync(pizza);
        return CreatedAtAction(nameof(GetPizza), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePizza(int id, [FromBody] Pizza pizza)
    {
        var ok = await _service.UpdateAsync(id, pizza);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePizza(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
