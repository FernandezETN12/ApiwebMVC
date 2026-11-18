using System.ComponentModel.DataAnnotations;

namespace ApiwebMVC.Models.DTOs;

public sealed record CreatePedidoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "ClienteId debe ser mayor que cero.")]
    public int ClienteId { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "El pedido debe tener al menos un artículo.")]
    public required IReadOnlyCollection<CreatePedidoDetalleRequest> Detalles { get; init; }

    [MaxLength(500)]
    public string? NotasDelivery { get; init; }
}

public sealed record CreatePedidoDetalleRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "PizzaId debe ser mayor que cero.")]
    public int PizzaId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { init; get; }
}
