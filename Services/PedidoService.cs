using ApiwebMVC.Data;
using ApiwebMVC.Exceptions;
using ApiwebMVC.Models;
using ApiwebMVC.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ApiwebMVC.Services;

public sealed class PedidoService : IPedidoService
{
    private readonly ApplicationDbContext _db;

    public PedidoService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<Pedido>> GetAllAsync() =>
        await _db.Pedidos.AsNoTracking().Include(p => p.Detalles).ThenInclude(d => d.Pizza).ToListAsync();

    public async Task<Pedido?> GetByIdAsync(int id) =>
        await _db.Pedidos.AsNoTracking().Include(p => p.Detalles).ThenInclude(d => d.Pizza)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyCollection<Pedido>> GetByClienteAsync(int clienteId) =>
        await _db.Pedidos.AsNoTracking().Include(p => p.Detalles).Where(p => p.ClienteId == clienteId).ToListAsync();

    public async Task<IReadOnlyCollection<Pedido>> GetByEstadoAsync(EstadoPedido estado) =>
        await _db.Pedidos.AsNoTracking().Include(p => p.Detalles).Where(p => p.Estado == estado).ToListAsync();

    public async Task<Pedido> CreateAsync(CreatePedidoRequest request)
    {
        if (request.Detalles.Count == 0)
            throw new ValidationException(new Dictionary<string, string[]> { ["Detalles"] = ["El pedido debe tener al menos un artículo."] });

        if (!await _db.Clientes.AnyAsync(c => c.Id == request.ClienteId))
            throw new NotFoundException("El cliente indicado no existe.");

        var pedido = new Pedido
        {
            ClienteId = request.ClienteId,
            NotasDelivery = request.NotasDelivery,
            Estado = EstadoPedido.EsperaDeConfirmacion,
            FechaPedido = DateTime.UtcNow
        };

        foreach (var item in request.Detalles)
        {
            if (item.Cantidad <= 0)
                throw new ValidationException(new Dictionary<string, string[]> { ["Cantidad"] = ["La cantidad debe ser mayor que cero."] });

            var pizza = await _db.Pizzas.FindAsync(item.PizzaId)
                ?? throw new NotFoundException($"La pizza {item.PizzaId} no existe.");
            if (pizza.Precio <= 0)
                throw new ConflictException($"La pizza {pizza.Nombre} no tiene un precio válido.");

            pedido.Detalles.Add(new PedidoDetalle
            {
                PizzaId = pizza.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = pizza.Precio
            });
        }

        pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();
        return pedido;
    }

    public async Task<Pedido> ConfirmarAsync(int id) => await CambiarEstadoAsync(id, EstadoPedido.EsperaDeConfirmacion, EstadoPedido.EnPreparacion, "confirmar");
    public async Task<Pedido> EnviarAsync(int id) => await CambiarEstadoAsync(id, EstadoPedido.EnPreparacion, EstadoPedido.EnViaje, "poner en viaje");
    public async Task<Pedido> EntregarAsync(int id) => await CambiarEstadoAsync(id, EstadoPedido.EnViaje, EstadoPedido.Entregado, "entregar");

    private async Task<Pedido> CambiarEstadoAsync(int id, EstadoPedido esperado, EstadoPedido nuevo, string accion)
    {
        var pedido = await _db.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Pedido no encontrado.");
        if (pedido.Estado != esperado)
            throw new ConflictException($"No se puede {accion} el pedido en el estado actual.");

        pedido.Estado = nuevo;
        if (nuevo == EstadoPedido.EnPreparacion) pedido.FechaConfirmacion = DateTime.UtcNow;
        if (nuevo == EstadoPedido.EnViaje) pedido.FechaEnViaje = DateTime.UtcNow;
        if (nuevo == EstadoPedido.Entregado) pedido.FechaEntrega = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return pedido;
    }

    public async Task DeleteAsync(int id)
    {
        var pedido = await _db.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Pedido no encontrado.");
        if (pedido.Estado != EstadoPedido.EsperaDeConfirmacion)
            throw new ConflictException("Solo se pueden eliminar pedidos en espera de confirmación.");

        _db.Pedidos.Remove(pedido);
        await _db.SaveChangesAsync();
    }
}
