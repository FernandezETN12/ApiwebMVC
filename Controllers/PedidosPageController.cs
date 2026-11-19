using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiwebMVC.Exceptions;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

/// <summary>"Mis pedidos": el cliente logueado ve sus pedidos y puede cancelar los pendientes.</summary>
[Authorize]
public sealed class PedidosPageController : Controller
{
    private readonly IPedidoApiClient _pedidosApi;

    public PedidosPageController(IPedidoApiClient pedidosApi)
    {
        _pedidosApi = pedidosApi;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var clienteId = User.GetClienteId();
        if (clienteId is null)
            return RedirectToAction("Login", "Account");

        var pedidos = await _pedidosApi.GetByClienteAsync(clienteId.Value);
        return View(pedidos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var pedido = await _pedidosApi.GetByIdAsync(id);
        if (pedido is null || pedido.ClienteId != User.GetClienteId())
            return NotFound();

        try
        {
            await _pedidosApi.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Pedido #{id} cancelado.";
        }
        catch (ConflictException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
