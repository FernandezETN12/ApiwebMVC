using ApiwebMVC.Models;
using ApiwebMVC.Models.DTOs;

namespace ApiwebMVC.Services;

public interface IPedidoApiClient
{
    Task<IReadOnlyCollection<Pedido>> GetByClienteAsync(int clienteId, CancellationToken cancellationToken = default);
    Task<Pedido?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Pedido> CreateAsync(CreatePedidoRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
