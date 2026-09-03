namespace ApiwebMVC.Models;

public enum EstadoPedido
{
    EsperaDeConfirmacion = 0,
    EnPreparacion = 1,
    EnViaje = 2,
    Entregado = 3
}

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public List<PedidoDetalle> Detalles { get; set; } = new();
    public EstadoPedido Estado { get; set; } = EstadoPedido.EsperaDeConfirmacion;
    public decimal Total { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
    public DateTime? FechaConfirmacion { get; set; }
    public DateTime? FechaPreparacion { get; set; }
    public DateTime? FechaEnViaje { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string? NotasDelivery { get; set; }
}

public class PedidoDetalle
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int PizzaId { get; set; }
    public Pizza? Pizza { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
