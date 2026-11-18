using ApiwebMVC.Services;
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
}
