using ApiwebMVC.Models;
using ApiwebMVC.Models.DTOs;

namespace ApiwebMVC.Services;

public interface IPedidoService
{
    Task<IReadOnlyCollection<Pedido>> GetAllAsync();
    Task<Pedido?> GetByIdAsync(int id);
    Task<IReadOnlyCollection<Pedido>> GetByClienteAsync(int clienteId);
    Task<IReadOnlyCollection<Pedido>> GetByEstadoAsync(EstadoPedido estado);
    Task<Pedido> CreateAsync(CreatePedidoRequest request);
    Task<Pedido> ConfirmarAsync(int id);
    Task<Pedido> EnviarAsync(int id);
    Task<Pedido> EntregarAsync(int id);
    Task DeleteAsync(int id);
}
