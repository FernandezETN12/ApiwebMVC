using ApiwebMVC.Controllers;
using ApiwebMVC.Data;
using ApiwebMVC.Exceptions;
using ApiwebMVC.Models;
using ApiwebMVC.Models.DTOs;
using ApiwebMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ApiwebMVC.Tests;

[TestFixture]
public sealed class PedidoServiceTests
{
    private static ApplicationDbContext CreateContext(string name)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options;
        return new ApplicationDbContext(options);
    }

    [Test]
    public async Task CreateAsync_WithValidData_CreatesPedidoAndCalculatesTotal()
    {
        await using var db = CreateContext(nameof(CreateAsync_WithValidData_CreatesPedidoAndCalculatesTotal));
        db.Clientes.Add(new Cliente { Id = 1, Nombre = "Ana", Telefono = "123", Direccion = "Centro" });
        db.Pizzas.Add(new Pizza { Id = 1, Nombre = "Napolitana", Precio = 1250, Ingredientes = ["Tomate", "Ajo"] });
        await db.SaveChangesAsync();

        var pedido = await new PedidoService(db).CreateAsync(new CreatePedidoRequest
        {
            ClienteId = 1,
            Detalles = [new CreatePedidoDetalleRequest { PizzaId = 1, Cantidad = 2 }]
        });

        Assert.That(pedido.Total, Is.EqualTo(2500m));
        Assert.That(pedido.Estado, Is.EqualTo(EstadoPedido.EsperaDeConfirmacion));
        Assert.That(pedido.Detalles, Has.Count.EqualTo(1));
    }

    [Test]
    public void CreateAsync_WithoutDetails_ThrowsValidationException()
    {
        using var db = CreateContext(nameof(CreateAsync_WithoutDetails_ThrowsValidationException));

        Assert.ThrowsAsync<ValidationException>(() => new PedidoService(db).CreateAsync(new CreatePedidoRequest
        {
            ClienteId = 1,
            Detalles = []
        }));
    }

    [Test]
    public void CreateAsync_WithUnknownPizza_ThrowsNotFoundException()
    {
        using var db = CreateContext(nameof(CreateAsync_WithUnknownPizza_ThrowsNotFoundException));
        db.Clientes.Add(new Cliente { Id = 1, Nombre = "Ana", Telefono = "123", Direccion = "Centro" });
        db.SaveChanges();

        Assert.ThrowsAsync<NotFoundException>(() => new PedidoService(db).CreateAsync(new CreatePedidoRequest
        {
            ClienteId = 1,
            Detalles = [new CreatePedidoDetalleRequest { PizzaId = 99, Cantidad = 1 }]
        }));
    }

    [Test]
    public async Task ConfirmarAsync_ChangesStateToPreparation()
    {
        await using var db = CreateContext(nameof(ConfirmarAsync_ChangesStateToPreparation));
        db.Pedidos.Add(new Pedido { Id = 1, ClienteId = 1, Estado = EstadoPedido.EsperaDeConfirmacion });
        await db.SaveChangesAsync();

        var pedido = await new PedidoService(db).ConfirmarAsync(1);

        Assert.That(pedido.Estado, Is.EqualTo(EstadoPedido.EnPreparacion));
        Assert.That(pedido.FechaConfirmacion, Is.Not.Null);
    }
}

[TestFixture]
public sealed class PizzasControllerTests
{
    [Test]
    public async Task GetPizza_WhenMissing_ReturnsNotFound()
    {
        var action = await new PizzasController(new StubPizzaService()).GetPizza(999);
        Assert.That(action.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CreatePizza_WithValidData_ReturnsCreatedAtAction()
    {
        var action = await new PizzasController(new StubPizzaService()).CreatePizza(new Pizza { Nombre = "Muzzarella", Precio = 1000 });
        Assert.That(action.Result, Is.InstanceOf<CreatedAtActionResult>());
    }

    private sealed class StubPizzaService : IPizzaService
    {
        public Task<IEnumerable<Pizza>> GetAllAsync() => Task.FromResult<IEnumerable<Pizza>>([]);
        public Task<Pizza?> GetByIdAsync(int id) => Task.FromResult<Pizza?>(null);
        public Task<Pizza> CreateAsync(Pizza pizza) { pizza.Id = 1; return Task.FromResult(pizza); }
        public Task<bool> UpdateAsync(int id, Pizza pizza) => Task.FromResult(false);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(false);
    }
}
