using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiwebMVC.Exceptions;
using ApiwebMVC.Models;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

/// <summary>Panel del administrador: ver todos los pedidos y avanzar su estado.</summary>
[Authorize(Policy = Roles.Admin)]
public sealed class AdminController : Controller
{
    private readonly IPedidoService _pedidos;

    public AdminController(IPedidoService pedidos)
    {
        _pedidos = pedidos;
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
}
