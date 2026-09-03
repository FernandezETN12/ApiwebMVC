using Microsoft.AspNetCore.Mvc;
using ApiwebMVC.Models;

namespace ApiwebMVC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    // Simulamos una base de datos en memoria
    private static List<Cliente> _clientes = new()
    {
        new Cliente 
        { 
            Id = 1, 
            Nombre = "Juan Pérez", 
            Telefono = "1234567890",
            Direccion = "Calle 1, 100"
        }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Cliente>> GetClientes()
    {
        return Ok(_clientes);
    }

    [HttpGet("{id}")]
    public ActionResult<Cliente> GetCliente(int id)
    {
        var cliente = _clientes.FirstOrDefault(c => c.Id == id);
        if (cliente == null)
            return NotFound();
        return Ok(cliente);
    }

    [HttpPost]
    public ActionResult<Cliente> CreateCliente([FromBody] Cliente cliente)
    {
        cliente.Id = _clientes.Count > 0 ? _clientes.Max(c => c.Id) + 1 : 1;
        cliente.FechaRegistro = DateTime.UtcNow;
        _clientes.Add(cliente);
        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCliente(int id, [FromBody] Cliente cliente)
    {
        var existing = _clientes.FirstOrDefault(c => c.Id == id);
        if (existing == null)
            return NotFound();

        existing.Nombre = cliente.Nombre;
        existing.Telefono = cliente.Telefono;
        existing.Direccion = cliente.Direccion;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCliente(int id)
    {
        var cliente = _clientes.FirstOrDefault(c => c.Id == id);
        if (cliente == null)
            return NotFound();

        _clientes.Remove(cliente);
        return NoContent();
    }
}
