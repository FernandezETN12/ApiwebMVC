using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiwebMVC.Data;
using ApiwebMVC.Exceptions;
using ApiwebMVC.Models;
using ApiwebMVC.Models.DTOs;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

/// <summary>
/// Carrito y confirmación (páginas web) + API REST de pedidos en /api/pedidos.
/// - Cualquiera puede armar el carrito.
/// - Para confirmar un pedido hay que iniciar sesión.
/// - Un cliente solo ve y cancela sus propios pedidos; el admin ve todos y cambia estados.
/// </summary>
[Route("api/[controller]")]
public class PedidosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPedidoService _pedidos;
    private readonly IClienteService _clientes;

    public PedidosController(ApplicationDbContext db, IPedidoService pedidos, IClienteService clientes)
    {
        _db = db;
        _pedidos = pedidos;
        _clientes = clientes;
    }

    // ---------- Páginas web (carrito) ----------

    [HttpGet("/Pedidos")]
    [HttpGet("/Pedidos/Index")]
    public async Task<IActionResult> Index()
    {
        var model = await BuildViewModelAsync(CarritoSession.Get(HttpContext));
        await PrefillClienteAsync(model);
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

    [Authorize]
    [HttpPost("/Pedidos/Confirmar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar([FromForm] PedidoIndexViewModel model)
    {
        var clienteId = User.GetClienteId();
        if (clienteId is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Pedidos" });

        var cart = CarritoSession.Get(HttpContext);
        var items = await BuildViewModelAsync(cart);
        items.ClienteNombre = User.Identity?.Name ?? model.ClienteNombre;
        items.ClienteTelefono = model.ClienteTelefono;
        items.ClienteDireccion = model.ClienteDireccion;

        // El nombre sale de la cuenta, no del formulario.
        ModelState.Remove(nameof(PedidoIndexViewModel.ClienteNombre));

        if (items.Items.Count == 0)
            ModelState.AddModelError(string.Empty, "Agrega al menos un producto antes de confirmar.");

        if (!ModelState.IsValid)
            return View("Index", items);

        await _clientes.ActualizarContactoAsync(clienteId.Value, model.ClienteTelefono.Trim(), model.ClienteDireccion.Trim());

        var pedido = await _pedidos.CreateAsync(new CreatePedidoRequest
        {
            ClienteId = clienteId.Value,
            Detalles = items.Items
                .Select(item => new CreatePedidoDetalleRequest { PizzaId = item.Pizza.Id, Cantidad = item.Cantidad })
                .ToList()
        });

        CarritoSession.Clear(HttpContext);
        TempData["SuccessMessage"] = $"Pedido #{pedido.Id} confirmado. Total: ${pedido.Total:N0}. Podés seguirlo en \"Mis pedidos\".";
        return RedirectToAction(nameof(Index));
    }

    private async Task PrefillClienteAsync(PedidoIndexViewModel model)
    {
        var clienteId = User.GetClienteId();
        if (clienteId is null) return;

        var cliente = await _clientes.GetByIdAsync(clienteId.Value);
        model.ClienteNombre = User.Identity?.Name ?? cliente?.Nombre ?? string.Empty;
        model.ClienteTelefono = cliente?.Telefono ?? string.Empty;
        model.ClienteDireccion = cliente?.Direccion ?? string.Empty;
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

    // ---------- API REST /api/pedidos ----------

    [Authorize(Policy = Roles.Admin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos() =>
        Ok(await _pedidos.GetAllAsync());

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var pedido = await _pedidos.GetByIdAsync(id);
        if (pedido is null || !PuedeVerCliente(pedido.ClienteId))
            throw new NotFoundException("Pedido no encontrado.");

        return Ok(pedido);
    }

    [Authorize]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<ActionResult<Pedido>> CreatePedido([FromBody] CreatePedidoRequest? request)
    {
        // JSON mal formado o con tipos incorrectos: respondemos 400 con el detalle, no un 500.
        if (request is null || !ModelState.IsValid)
            throw new ValidationException(ErroresDeModelo());

        // Un cliente solo puede crear pedidos a su nombre; el admin puede indicar cualquier cliente.
        if (!User.EsAdmin())
        {
            var clienteId = User.GetClienteId();
            if (clienteId is null) return Forbid();
            request = request with { ClienteId = clienteId.Value };
        }

        var pedido = await _pedidos.CreateAsync(request);
        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    [Authorize(Policy = Roles.Admin)]
    [HttpPut("{id:int}/confirmar")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ConfirmarPedido(int id) =>
        Ok(new { mensaje = "Estado del pedido actualizado", pedido = await _pedidos.ConfirmarAsync(id) });

    [Authorize(Policy = Roles.Admin)]
    [HttpPut("{id:int}/en-viaje")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> EnViajePedido(int id) =>
        Ok(new { mensaje = "Estado del pedido actualizado", pedido = await _pedidos.EnviarAsync(id) });

    [Authorize(Policy = Roles.Admin)]
    [HttpPut("{id:int}/entregar")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> EntregarPedido(int id) =>
        Ok(new { mensaje = "Estado del pedido actualizado", pedido = await _pedidos.EntregarAsync(id) });

    [Authorize]
    [HttpGet("cliente/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosPorCliente(int clienteId)
    {
        if (!PuedeVerCliente(clienteId))
            return Forbid();

        return Ok(await _pedidos.GetByClienteAsync(clienteId));
    }

    [Authorize(Policy = Roles.Admin)]
    [HttpGet("estado/{estado}")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosPorEstado(EstadoPedido estado) =>
        Ok(await _pedidos.GetByEstadoAsync(estado));

    /// <summary>Cancela un pedido. Solo mientras está en espera de confirmación.</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeletePedido(int id)
    {
        var pedido = await _pedidos.GetByIdAsync(id);
        if (pedido is null || !PuedeVerCliente(pedido.ClienteId))
            throw new NotFoundException("Pedido no encontrado.");

        await _pedidos.DeleteAsync(id);
        return NoContent();
    }

    private Dictionary<string, string[]> ErroresDeModelo()
    {
        var errores = ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => string.IsNullOrEmpty(entry.Key) ? "body" : entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "El JSON enviado no es válido." : error.ErrorMessage)
                    .ToArray());

        if (errores.Count == 0)
            errores["body"] = ["Falta el cuerpo JSON del pedido."];
        return errores;
    }

    private bool PuedeVerCliente(int clienteId) =>
        User.EsAdmin() || User.GetClienteId() == clienteId;
}
