using System.ComponentModel.DataAnnotations;

namespace ApiwebMVC.Models;

public class PedidoIndexViewModel
{
    public List<PedidoCarritoItemViewModel> Items { get; set; } = new();
    public decimal Total => Items.Sum(item => item.Subtotal);

    [Required(ErrorMessage = "Ingresa tu nombre.")]
    [StringLength(150)]
    public string ClienteNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa un teléfono de contacto.")]
    [StringLength(50)]
    public string ClienteTelefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa la dirección de entrega.")]
    [StringLength(300)]
    public string ClienteDireccion { get; set; } = string.Empty;
}

public class PedidoCarritoItemViewModel
{
    public Pizza Pizza { get; set; } = new();
    public int Cantidad { get; set; }
    public decimal Subtotal => Pizza.Precio * Cantidad;
}