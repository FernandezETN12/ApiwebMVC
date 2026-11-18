using ApiwebMVC.Models;

namespace ApiwebMVC.Services;

public sealed class InMemoryStore
{
    private readonly object _sync = new();

    public List<Cliente> Clientes { get; } =
    [
        new Cliente { Id = 1, Nombre = "Cliente invitado", Telefono = "Sin teléfono", Direccion = "Sin dirección" }
    ];

    public List<Pizza> Pizzas { get; } =
    [
        new Pizza { Id = 1, Nombre = "Muzzarella Clásica", Descripcion = "Muzzarella, salsa de tomate y orégano.", Precio = 8500, Ingredientes = ["Muzzarella", "Salsa de tomate", "Orégano"] },
        new Pizza { Id = 2, Nombre = "Napolitana con Ajo", Descripcion = "Tomate, ajo, muzzarella y albahaca.", Precio = 9800, Ingredientes = ["Tomate", "Ajo", "Muzzarella", "Albahaca"] },
        new Pizza { Id = 3, Nombre = "Fugazzeta Especial", Descripcion = "Cebolla dulce y abundante muzzarella.", Precio = 9200, Ingredientes = ["Cebolla", "Muzzarella", "Aceite de oliva"] },
        new Pizza { Id = 4, Nombre = "Calabresa Picante", Descripcion = "Longaniza calabresa, morrones y picante.", Precio = 11200, Ingredientes = ["Calabresa", "Morrones", "Ají"] },
        new Pizza { Id = 5, Nombre = "Cuatro Quesos", Descripcion = "Una combinación de cuatro quesos.", Precio = 11800, Ingredientes = ["Muzzarella", "Fontina", "Roquefort", "Provolone"] }
    ];

    public List<Pedido> Pedidos { get; } = [];

    public T WithLock<T>(Func<T> action)
    {
        lock (_sync) return action();
    }

    public void WithLock(Action action)
    {
        lock (_sync) action();
    }
}
