using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

[Authorize]
public sealed class PedidosPageController : Controller
{
    private readonly IPedidoService _pedidos;

    public PedidosPageController(IPedidoService pedidos)
    {
        _pedidos = pedidos;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var clienteIdClaim = User.FindFirst("ClienteId")?.Value;
        if (!int.TryParse(clienteIdClaim, out var clienteId))
            return RedirectToAction("Login", "Account");

        var pedidos = await _pedidos.GetByClienteAsync(clienteId);
        return View(pedidos);
    }
}
