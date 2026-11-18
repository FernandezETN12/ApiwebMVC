using ApiwebMVC.Exceptions;
using ApiwebMVC.Services;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace ApiwebMVC.Tests;

[TestFixture]
public sealed class FotosProductosTests
{
    [TestCase("Muzzarella", "muzzarella")]
    [TestCase("Jamón y Morrones", "jamon-y-morrones")]
    [TestCase("Rúcula & Parmesano", "rucula-parmesano")]
    [TestCase("Coca-Cola", "coca-cola")]
    [TestCase("Empanada de Jamón y Queso", "empanada-de-jamon-y-queso")]
    public void NombreArchivo_QuitaAcentosYUsaGuiones(string producto, string esperado) =>
        Assert.That(FotosProductos.NombreArchivo(producto), Is.EqualTo(esperado));

    [Test]
    public void BuscarFotoLocal_ConArchivo_DevuelveRutaWeb()
    {
        var raiz = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(raiz, "img", "productos"));
        File.WriteAllBytes(Path.Combine(raiz, "img", "productos", "fugazzeta.png"), [1]);
        try
        {
            Assert.That(FotosProductos.BuscarFotoLocal(raiz, "Fugazzeta"), Is.EqualTo("/img/productos/fugazzeta.png"));
            Assert.That(FotosProductos.BuscarFotoLocal(raiz, "Diavola"), Is.Null);
        }
        finally
        {
            Directory.Delete(raiz, true);
        }
    }

    private static IFormFile Archivo(byte[] contenido, string nombre) =>
        new FormFile(new MemoryStream(contenido), 0, contenido.Length, "foto", nombre);

    [Test]
    public async Task GuardarFoto_Jpg_LaGuardaConElNombreDelProducto()
    {
        var raiz = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            byte[] jpg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 0x4A, 0x46, 0x49, 0x46, 0, 1];
            var ruta = await FotosProductos.GuardarFotoAsync(raiz, "Jamón y Morrones", Archivo(jpg, "descarga (3).jpeg"));
            Assert.That(ruta, Is.EqualTo("/img/productos/jamon-y-morrones.jpg"));
            Assert.That(File.Exists(Path.Combine(raiz, "img", "productos", "jamon-y-morrones.jpg")), Is.True);
        }
        finally
        {
            if (Directory.Exists(raiz)) Directory.Delete(raiz, true);
        }
    }

    [Test]
    public void GuardarFoto_ArchivoQueNoEsImagen_EsRechazado()
    {
        var raiz = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var texto = System.Text.Encoding.UTF8.GetBytes("hola, no soy una imagen");
        Assert.ThrowsAsync<ValidationException>(() => FotosProductos.GuardarFotoAsync(raiz, "Diavola", Archivo(texto, "foto.jpg")));
        Assert.That(Directory.Exists(raiz), Is.False);
    }
}
