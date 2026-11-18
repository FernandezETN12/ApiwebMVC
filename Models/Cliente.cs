namespace ApiwebMVC.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    // Null para clientes viejos (creados antes del login). MySQL permite varios NULL en un índice único.
    public string? Email { get; set; }
    public string? GoogleSubject { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
