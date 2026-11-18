namespace ApiwebMVC.Models;

public static class EstadoTexto
{
    public static string De(EstadoPedido estado) => estado switch
    {
        EstadoPedido.EsperaDeConfirmacion => "Esperando confirmación",
        EstadoPedido.EnPreparacion => "En preparación",
        EstadoPedido.EnViaje => "En viaje",
        EstadoPedido.Entregado => "Entregado",
        _ => estado.ToString()
    };

    /// <summary>Texto del botón para pasar al siguiente estado (null si ya está entregado).</summary>
    public static string? SiguienteAccion(EstadoPedido estado) => estado switch
    {
        EstadoPedido.EsperaDeConfirmacion => "Confirmar y preparar",
        EstadoPedido.EnPreparacion => "Enviar",
        EstadoPedido.EnViaje => "Marcar entregado",
        _ => null
    };

    public static string Badge(EstadoPedido estado) => estado switch
    {
        EstadoPedido.EsperaDeConfirmacion => "bg-warning text-dark",
        EstadoPedido.EnPreparacion => "bg-info text-dark",
        EstadoPedido.EnViaje => "bg-primary",
        EstadoPedido.Entregado => "bg-success",
        _ => "bg-secondary"
    };
}
