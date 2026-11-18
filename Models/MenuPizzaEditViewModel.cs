using System.ComponentModel.DataAnnotations;

namespace ApiwebMVC.Models;

public class MenuPizzaEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true)]
    public decimal Precio { get; set; }

    public string? IngredientesTexto { get; set; }

    public string Categoria { get; set; } = "Pizzas";

    // Una dirección web (https://…) o una foto guardada en el proyecto (/img/productos/…).
    [RegularExpression(@"^(https?://\S+|/img/productos/[A-Za-z0-9._-]+(\?v=[0-9]+)?)$",
        ErrorMessage = "Usá una dirección https://… o una foto local /img/productos/archivo.jpg.")]
    public string? ImagenURL { get; set; }
}