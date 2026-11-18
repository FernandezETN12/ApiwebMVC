using ApiwebMVC.Data;
using ApiwebMVC.Exceptions;
using ApiwebMVC.Models;
using ApiwebMVC.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace ApiwebMVC.Tests;

[TestFixture]
public sealed class AdminServiceTests
{
    private static AdminService Crear(params string[] emails)
    {
        var valores = emails.Select((email, i) => new KeyValuePair<string, string?>($"Admin:Emails:{i}", email));
        var config = new ConfigurationBuilder().AddInMemoryCollection(valores).Build();
        return new AdminService(config);
    }

    [Test]
    public void EsAdmin_EmailEnLaLista_True() =>
        Assert.That(Crear("admin@pizzeria.com").EsAdmin("admin@pizzeria.com"), Is.True);

    [Test]
    public void EsAdmin_IgnoraMayusculasYEspacios() =>
        Assert.That(Crear("admin@pizzeria.com").EsAdmin("  ADMIN@Pizzeria.com "), Is.True);

    [Test]
    public void EsAdmin_EmailFueraDeLaLista_False() =>
        Assert.That(Crear("admin@pizzeria.com").EsAdmin("cliente@gmail.com"), Is.False);

    [Test]
    public void EsAdmin_SinListaConfigurada_NadieEsAdmin()
    {
        Assert.That(Crear().EsAdmin("admin@pizzeria.com"), Is.False);
        Assert.That(Crear().EsAdmin(null), Is.False);
        Assert.That(Crear().EsAdmin(""), Is.False);
    }
}

[TestFixture]
public sealed class PedidoEstadosTests
{
    private static async Task<(ApplicationDbContext db, PedidoService service)> ConPedido(string name, EstadoPedido estado)
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);
        db.Pedidos.Add(new Pedido { Id = 1, ClienteId = 1, Estado = estado });
        await db.SaveChangesAsync();
        return (db, new PedidoService(db));
    }

    [Test]
    public async Task FlujoCompleto_EsperaPreparacionViajeEntregado()
    {
        var (db, service) = await ConPedido(nameof(FlujoCompleto_EsperaPreparacionViajeEntregado), EstadoPedido.EsperaDeConfirmacion);
        await using var _ = db;

        Assert.That((await service.ConfirmarAsync(1)).Estado, Is.EqualTo(EstadoPedido.EnPreparacion));
        Assert.That((await service.EnviarAsync(1)).Estado, Is.EqualTo(EstadoPedido.EnViaje));
        var entregado = await service.EntregarAsync(1);
        Assert.That(entregado.Estado, Is.EqualTo(EstadoPedido.Entregado));
        Assert.That(entregado.FechaEntrega, Is.Not.Null);
    }

    [Test]
    public async Task Entregar_SinHaberEnviado_ThrowsConflict()
    {
        var (db, service) = await ConPedido(nameof(Entregar_SinHaberEnviado_ThrowsConflict), EstadoPedido.EnPreparacion);
        await using var _ = db;
        Assert.ThrowsAsync<ConflictException>(() => service.EntregarAsync(1));
    }

    [Test]
    public async Task Cancelar_PedidoYaConfirmado_ThrowsConflict()
    {
        var (db, service) = await ConPedido(nameof(Cancelar_PedidoYaConfirmado_ThrowsConflict), EstadoPedido.EnPreparacion);
        await using var _ = db;
        Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(1));
    }

    [Test]
    public async Task Cancelar_PedidoPendiente_LoElimina()
    {
        var (db, service) = await ConPedido(nameof(Cancelar_PedidoPendiente_LoElimina), EstadoPedido.EsperaDeConfirmacion);
        await using var _ = db;
        await service.DeleteAsync(1);
        Assert.That(await db.Pedidos.CountAsync(), Is.EqualTo(0));
    }
}
