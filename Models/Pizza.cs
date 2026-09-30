using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiwebMVC.Models;

public class Pizza
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
    public decimal Precio { get; set; }
    public string Categoria { get; set; } = "Pizzas";
    public List<string>? Ingredientes { get; set; } = new();
    
    public string ImagenURL {get;set;} = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
