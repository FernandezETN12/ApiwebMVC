using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiwebMVC.Data;
using ApiwebMVC.Models;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

[Route("api/[controller]")]
public class PedidosController : Controller
{
    private readonly ApplicationDbContext _db;

    public PedidosController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/Pedidos")]
    [HttpGet("/Pedidos/Index")]
    public async Task<IActionResult> Index()
    {
        var model = await BuildViewModelAsync(CarritoSession.Get(HttpContext));
        return View(model);
    }

    [HttpPost("/Pedidos/ActualizarCantidad")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int pizzaId, int cantidad)
    {
        var cart = CarritoSession.Get(HttpContext);
        if (cantidad <= 0)
        {
            cart.Remove(pizzaId);
        }
        else if (cart.ContainsKey(pizzaId))
        {
            cart[pizzaId] = Math.Min(cantidad, 99);
        }

        CarritoSession.Save(HttpContext, cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/Pedidos/Quitar")]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int pizzaId)
    {
        var cart = CarritoSession.Get(HttpContext);
        cart.Remove(pizzaId);
        CarritoSession.Save(HttpContext, cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/Pedidos/Confirmar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar([FromForm] PedidoIndexViewModel model)
    {
        var cart = CarritoSession.Get(HttpContext);
        var items = await BuildViewModelAsync(cart);
        items.ClienteNombre = model.ClienteNombre;
        items.ClienteTelefono = model.ClienteTelefono;
        items.ClienteDireccion = model.ClienteDireccion;

        if (!ModelState.IsValid || items.Items.Count == 0)
        {
            if (items.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Agrega al menos un producto antes de confirmar.");
            }

            return View("Index", items);
        }

        var client = new Cliente
        {
            Nombre = model.ClienteNombre.Trim(),
            Telefono = model.ClienteTelefono.Trim(),
            Direccion = model.ClienteDireccion.Trim(),
            FechaRegistro = DateTime.UtcNow
        };
        var order = new Pedido
        {
            Cliente = client,
            Estado = EstadoPedido.EsperaDeConfirmacion,
            FechaPedido = DateTime.UtcNow,
            Total = items.Total,
            Detalles = items.Items.Select(item => new PedidoDetalle
            {
                PizzaId = item.Pizza.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.Pizza.Precio
            }).ToList()
        };

        _db.Pedidos.Add(order);
        await _db.SaveChangesAsync();
        CarritoSession.Clear(HttpContext);
        TempData["SuccessMessage"] = $"Pedido #{order.Id} confirmado. Total: ${order.Total:N0}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<PedidoIndexViewModel> BuildViewModelAsync(Dictionary<int, int> cart)
    {
        var ids = cart.Keys.ToList();
        var pizzas = await _db.Pizzas.AsNoTracking()
            .Where(pizza => ids.Contains(pizza.Id))
            .ToListAsync();

        return new PedidoIndexViewModel
        {
            Items = pizzas.Select(pizza => new PedidoCarritoItemViewModel
            {
                Pizza = pizza,
                Cantidad = cart[pizza.Id]
            }).ToList()
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        var orders = await _db.Pedidos.AsNoTracking()
            .Include(pedido => pedido.Detalles)
            .ThenInclude(detail => detail.Pizza)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var order = await _db.Pedidos.AsNoTracking()
            .Include(pedido => pedido.Detalles)
            .ThenInclude(detail => detail.Pizza)
            .FirstOrDefaultAsync(pedido => pedido.Id == id);
        if (order == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> CreatePedido([FromBody] Pedido pedido)
    {
        if (pedido.ClienteId <= 0 || pedido.Detalles == null || pedido.Detalles.Count == 0)
        {
            return BadRequest(new { mensaje = "El pedido requiere un cliente y al menos un artículo." });
        }

        pedido.Id = 0;
        pedido.FechaPedido = DateTime.UtcNow;
        pedido.Estado = EstadoPedido.EsperaDeConfirmacion;
        foreach (var detail in pedido.Detalles)
        {
            detail.Id = 0;
            detail.PedidoId = 0;
        }

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id:int}/confirmar")]
    public async Task<IActionResult> ConfirmarPedido(int id)
    {
        return await ChangeStatusAsync(id, EstadoPedido.EsperaDeConfirmacion, EstadoPedido.EnPreparacion,
            pedido => { pedido.FechaConfirmacion = DateTime.UtcNow; });
    }

    [HttpPut("{id:int}/en-viaje")]
    public async Task<IActionResult> EnViajePedido(int id)
    {
        return await ChangeStatusAsync(id, EstadoPedido.EnPreparacion, EstadoPedido.EnViaje,
            pedido => { pedido.FechaEnViaje = DateTime.UtcNow; });
    }

    [HttpPut("{id:int}/entregar")]
    public async Task<IActionResult> EntregarPedido(int id)
    {
        return await ChangeStatusAsync(id, EstadoPedido.EnViaje, EstadoPedido.Entregado,
            pedido => { pedido.FechaEntrega = DateTime.UtcNow; });
    }

    private async Task<IActionResult> ChangeStatusAsync(
        int id,
        EstadoPedido expected,
        EstadoPedido next,
        Action<Pedido> setTimestamp)
    {
        var order = await _db.Pedidos.FindAsync(id);
        if (order == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });
        if (order.Estado != expected)
            return BadRequest(new { mensaje = "El pedido no se encuentra en el estado requerido." });

        order.Estado = next;
        setTimestamp(order);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Estado del pedido actualizado", pedido = order });
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosPorCliente(int clienteId)
    {
        var orders = await _db.Pedidos.AsNoTracking()
            .Where(order => order.ClienteId == clienteId)
            .Include(order => order.Detalles)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("estado/{estado}")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosPorEstado(EstadoPedido estado)
    {
        var orders = await _db.Pedidos.AsNoTracking()
            .Where(order => order.Estado == estado)
            .Include(order => order.Detalles)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePedido(int id)
    {
        var order = await _db.Pedidos.FindAsync(id);
        if (order == null)
            return NotFound();
        if (order.Estado != EstadoPedido.EsperaDeConfirmacion)
            return BadRequest(new { mensaje = "Solo se pueden eliminar pedidos en espera de confirmación." });

        _db.Pedidos.Remove(order);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
