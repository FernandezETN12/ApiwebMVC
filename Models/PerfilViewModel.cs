using System.ComponentModel.DataAnnotations;

namespace ApiwebMVC.Models;

public class PerfilViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool EsAdmin { get; set; }
    public IReadOnlyCollection<Pedido> Pedidos { get; set; } = [];

    [Required(ErrorMessage = "Ingresa el nombre de la calle.")]
    [StringLength(150)]
    public string Calle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa el número de la calle.")]
    [StringLength(20)]
    public string Altura { get; set; } = string.Empty;

    [StringLength(50)]
    public string Telefono { get; set; } = string.Empty;
}
