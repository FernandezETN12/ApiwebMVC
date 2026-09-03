using Microsoft.AspNetCore.Mvc;
using ApiwebMVC.Models;

namespace ApiwebMVC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    // Simulamos una base de datos en memoria
    private static List<Pedido> _pedidos = new();
    private static int _idCounter = 1;

    [HttpGet]
    public ActionResult<IEnumerable<Pedido>> GetPedidos()
    {
        return Ok(_pedidos);
    }

    [HttpGet("{id}")]
    public ActionResult<Pedido> GetPedido(int id)
    {
        var pedido = _pedidos.FirstOrDefault(p => p.Id == id);
        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });
        return Ok(pedido);
    }

    [HttpPost]
    public ActionResult<Pedido> CreatePedido([FromBody] Pedido pedido)
    {
        if (pedido.ClienteId <= 0)
            return BadRequest(new { mensaje = "ClienteId es requerido" });

        if (pedido.Detalles == null || pedido.Detalles.Count == 0)
            return BadRequest(new { mensaje = "El pedido debe tener al menos un artículo" });

        pedido.Id = _idCounter++;
        pedido.FechaPedido = DateTime.UtcNow;
        pedido.Estado = EstadoPedido.EsperaDeConfirmacion;
        
        _pedidos.Add(pedido);
        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id}/confirmar")]
    public IActionResult ConfirmarPedido(int id)
    {
        var pedido = _pedidos.FirstOrDefault(p => p.Id == id);
        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });

        if (pedido.Estado != EstadoPedido.EsperaDeConfirmacion)
            return BadRequest(new { mensaje = "El pedido no está en espera de confirmación" });

        pedido.Estado = EstadoPedido.EnPreparacion;
        pedido.FechaConfirmacion = DateTime.UtcNow;
        return Ok(new { mensaje = "Pedido confirmado", pedido });
    }

    [HttpPut("{id}/en-viaje")]
    public IActionResult EnViajePedido(int id)
    {
        var pedido = _pedidos.FirstOrDefault(p => p.Id == id);
        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });

        if (pedido.Estado != EstadoPedido.EnPreparacion)
            return BadRequest(new { mensaje = "El pedido no está en preparación" });

        pedido.Estado = EstadoPedido.EnViaje;
        pedido.FechaEnViaje = DateTime.UtcNow;
        return Ok(new { mensaje = "Pedido en viaje", pedido });
    }

    [HttpPut("{id}/entregar")]
    public IActionResult EntregarPedido(int id)
    {
        var pedido = _pedidos.FirstOrDefault(p => p.Id == id);
        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });

        if (pedido.Estado != EstadoPedido.EnViaje)
            return BadRequest(new { mensaje = "El pedido no está en viaje" });

        pedido.Estado = EstadoPedido.Entregado;
        pedido.FechaEntrega = DateTime.UtcNow;
        return Ok(new { mensaje = "Pedido entregado", pedido });
    }

    [HttpGet("cliente/{clienteId}")]
    public ActionResult<IEnumerable<Pedido>> GetPedidosPorCliente(int clienteId)
    {
        var pedidos = _pedidos.Where(p => p.ClienteId == clienteId).ToList();
        return Ok(pedidos);
    }

    [HttpGet("estado/{estado}")]
    public ActionResult<IEnumerable<Pedido>> GetPedidosPorEstado(EstadoPedido estado)
    {
        var pedidos = _pedidos.Where(p => p.Estado == estado).ToList();
        return Ok(pedidos);
    }

    [HttpDelete("{id}")]
    public IActionResult DeletePedido(int id)
    {
        var pedido = _pedidos.FirstOrDefault(p => p.Id == id);
        if (pedido == null)
            return NotFound();

        if (pedido.Estado != EstadoPedido.EsperaDeConfirmacion)
            return BadRequest(new { mensaje = "Solo se pueden eliminar pedidos en espera de confirmación" });

        _pedidos.Remove(pedido);
        return NoContent();
    }
}
