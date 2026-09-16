namespace ApiwebMVC.Models;

public class Pizza
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public List<string> Ingredientes { get; set; } = new();
    
    public string ImagenURL {get;set;} = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
