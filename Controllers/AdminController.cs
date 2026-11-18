using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiwebMVC.Exceptions;
using ApiwebMVC.Models;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

/// <summary>Panel del administrador: pedidos (ver y avanzar estado) e imágenes de los productos.</summary>
[Authorize(Policy = Roles.Admin)]
public sealed class AdminController : Controller
{
    private readonly IPedidoService _pedidos;
    private readonly IPizzaService _pizzas;
    private readonly IWebHostEnvironment _environment;

    public AdminController(IPedidoService pedidos, IPizzaService pizzas, IWebHostEnvironment environment)
    {
        _pedidos = pedidos;
        _pizzas = pizzas;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(EstadoPedido? estado = null)
    {
        var pedidos = estado is null
            ? await _pedidos.GetAllAsync()
            : await _pedidos.GetByEstadoAsync(estado.Value);

        ViewBag.Estado = estado;
        return View(pedidos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AvanzarEstado(int id, EstadoPedido estadoActual)
    {
        try
        {
            var pedido = estadoActual switch
            {
                EstadoPedido.EsperaDeConfirmacion => await _pedidos.ConfirmarAsync(id),
                EstadoPedido.EnPreparacion => await _pedidos.EnviarAsync(id),
                EstadoPedido.EnViaje => await _pedidos.EntregarAsync(id),
                _ => throw new ConflictException("El pedido ya fue entregado.")
            };
            TempData["SuccessMessage"] = $"Pedido #{pedido.Id}: {EstadoTexto.De(pedido.Estado)}.";
        }
        catch (Exception ex) when (ex is ConflictException or NotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------- Imágenes de los productos ----------

    [HttpGet]
    public async Task<IActionResult> Imagenes(string? categoria = null)
    {
        var productos = (await _pizzas.GetAllAsync())
            .Where(p => string.IsNullOrWhiteSpace(categoria) || string.Equals(p.Categoria, categoria, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Categoria).ThenBy(p => p.Nombre)
            .ToList();

        ViewBag.Categoria = categoria;
        return View(productos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(FotosProductos.TamanioMaximo + 64 * 1024)]
    public async Task<IActionResult> SubirImagen(int id, IFormFile? foto, string? categoria = null)
    {
        var producto = await _pizzas.GetByIdAsync(id);
        if (producto is null)
            return NotFound();

        try
        {
            var ruta = await FotosProductos.GuardarFotoAsync(_environment.WebRootPath, producto.Nombre, foto!);
            // ?v= cambia en cada subida para que el navegador no muestre la foto vieja guardada en caché.
            producto.ImagenURL = $"{ruta}?v={DateTime.UtcNow.Ticks}";
            await _pizzas.UpdateAsync(id, producto);
            TempData["SuccessMessage"] = $"Foto de «{producto.Nombre}» actualizada.";
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = $"«{producto.Nombre}»: {ex.Errors.Values.SelectMany(e => e).FirstOrDefault()}";
        }

        return RedirectToAction(nameof(Imagenes), new { categoria });
    }
}
