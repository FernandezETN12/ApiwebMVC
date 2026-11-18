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

    public const long TamanioMaximo = 5 * 1024 * 1024; // 5 MB

    /// <summary>
    /// Guarda la foto subida con el nombre del producto (reemplaza la anterior) y
    /// devuelve su ruta web. Solo acepta JPG, PNG o WEBP de hasta 5 MB, y revisa el
    /// contenido real del archivo, no solo la extensión.
    /// </summary>
    public static async Task<string> GuardarFotoAsync(string webRootPath, string nombreProducto, IFormFile archivo)
    {
        if (archivo is null || archivo.Length == 0)
            throw new Exceptions.ValidationException(new Dictionary<string, string[]> { ["foto"] = ["Elegí una imagen."] });
        if (archivo.Length > TamanioMaximo)
            throw new Exceptions.ValidationException(new Dictionary<string, string[]> { ["foto"] = ["La imagen pesa más de 5 MB."] });

        var cabecera = new byte[12];
        await using (var lectura = archivo.OpenReadStream())
        {
            _ = await lectura.ReadAtLeastAsync(cabecera, cabecera.Length, throwOnEndOfStream: false);
        }

        var extension = DetectarExtension(cabecera)
            ?? throw new Exceptions.ValidationException(new Dictionary<string, string[]> { ["foto"] = ["El archivo no es una imagen JPG, PNG o WEBP."] });

        var nombre = NombreArchivo(nombreProducto);
        var carpeta = Path.Combine(webRootPath, "img", "productos");
        Directory.CreateDirectory(carpeta);

        // Borra la foto anterior del producto, aunque tenga otra extensión.
        foreach (var ext in Extensiones)
        {
            var anterior = Path.Combine(carpeta, nombre + ext);
            if (File.Exists(anterior)) File.Delete(anterior);
        }

        var destino = Path.Combine(carpeta, nombre + extension);
        await using (var salida = File.Create(destino))
        {
            await archivo.CopyToAsync(salida);
        }

        return $"{CarpetaWeb}/{nombre}{extension}";
    }

    /// <summary>Reconoce el formato por los primeros bytes del archivo.</summary>
    private static string? DetectarExtension(byte[] b)
    {
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return ".jpg";
        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return ".png";
        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
            && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50) return ".webp";
        return null;
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
