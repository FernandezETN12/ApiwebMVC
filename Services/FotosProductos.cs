using System.Globalization;
using System.Text;

namespace ApiwebMVC.Services;

/// <summary>
/// Fotos de los productos guardadas dentro del proyecto, en wwwroot/img/productos.
/// Cada archivo se llama como el producto, sin acentos y con guiones:
/// "Jamón y Morrones" → jamon-y-morrones.jpg (también .jpeg, .png o .webp).
/// </summary>
public static class FotosProductos
{
    public const string CarpetaWeb = "/img/productos";
    private static readonly string[] Extensiones = [".webp", ".jpg", ".jpeg", ".png"];

    /// <summary>Nombre de archivo esperado para un producto, sin extensión.</summary>
    public static string NombreArchivo(string nombreProducto)
    {
        var sinAcentos = new StringBuilder();
        foreach (var c in nombreProducto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sinAcentos.Append(c);
        }

        var slug = new StringBuilder();
        foreach (var c in sinAcentos.ToString().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(c))
                slug.Append(c);
            else if (slug.Length > 0 && slug[^1] != '-')
                slug.Append('-');
        }

        return slug.ToString().Trim('-');
    }

    /// <summary>Ruta web de la foto local del producto, o null si no hay archivo.</summary>
    public static string? BuscarFotoLocal(string webRootPath, string nombreProducto)
    {
        var nombre = NombreArchivo(nombreProducto);
        if (string.IsNullOrEmpty(nombre)) return null;

        var carpeta = Path.Combine(webRootPath, "img", "productos");
        foreach (var extension in Extensiones)
        {
            if (File.Exists(Path.Combine(carpeta, nombre + extension)))
                return $"{CarpetaWeb}/{nombre}{extension}";
        }

        return null;
    }
}
