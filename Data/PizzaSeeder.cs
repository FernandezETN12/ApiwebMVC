using ApiwebMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiwebMVC.Data;

public static class PizzaSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var existingNames = (await context.Pizzas
            .Select(pizza => pizza.Nombre)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var createdAt = DateTime.UtcNow;
        var pizzas = new[]
        {
            new Pizza
            {
                Nombre = "Muzzarella",
                Descripcion = "La clásica pizza argentina con salsa de tomate, muzzarella y orégano.",
                Precio = 8500.00m,
                Ingredientes = new List<string> { "Salsa de tomate", "Muzzarella", "Orégano" },
                FechaCreacion = createdAt,
                ImagenURL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTiv6PvL2gD48msLbXb4J7H66gtlEZXRz_hoiusv8_Iauzfqrf--5AyHMqr&s=10"
            },
            new Pizza
            {
                Nombre = "Napolitana",
                Descripcion = "Pizza con muzzarella, rodajas de tomate y orégano.",
                Precio = 9500.00m,
                Ingredientes = new List<string> { "Salsa de tomate", "Muzzarella", "Tomate", "Orégano" },
                FechaCreacion = createdAt,
                ImagenURL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSTIdLIoB8enL1kftBxlu5wP35taYZpdKOTnMASgWz6ntAKQgW8SenPcZA&s=10"
            },
            new Pizza
            {
                Nombre = "Jamón y Morrones",
                Descripcion = "Pizza con muzzarella, jamón y morrones.",
                Precio = 10500.00m,
                Ingredientes = new List<string> { "Salsa de tomate", "Muzzarella", "Jamón", "Morrones" },
                FechaCreacion = createdAt,
                ImagenURL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTzOE0BX7CQJyfwzEhmeyX-V4XaWm_ieQDrJ9jOBH-JX4E0nBXagjNumlh4&s=10"
            },
            new Pizza
            {
                Nombre = "Fugazzeta",
                Descripcion = "Pizza con abundante muzzarella, cebolla y orégano.",
                Precio = 11000.00m,
                Ingredientes = new List<string> { "Muzzarella", "Cebolla", "Orégano" },
                FechaCreacion = createdAt,
                ImagenURL = "https://latorrepizza.com.ar/wp-content/uploads/2023/05/fugazzeta-con-jamon.png"
            },
            new Pizza
            {
                Nombre = "Calabresa",
                Descripcion = "Pizza con muzzarella, longaniza calabresa y orégano.",
                Precio = 11500.00m,
                Ingredientes = new List<string> { "Salsa de tomate", "Muzzarella", "Longaniza calabresa", "Orégano" },
                FechaCreacion = createdAt,
                ImagenURL = "https://img.magnific.com/psd-gratis/deliciosa-pizza-pepperoni-es-delicia-culinaria_632498-24206.jpg?semt=ais_hybrid&w=740&q=80"
            },
            new Pizza
            {
                Nombre = "Tomate con Albahaca",
                Descripcion = "Pizza con tomate, muzzarella y albahaca fresca.",
                Precio = 12000.00m,
                Ingredientes = new List<string> { "Tomate", "Muzzarella", "Albahaca" },
                FechaCreacion = createdAt,
                ImagenURL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQdhRuSkQ6cppQwCb2CSJ1Ty3HuGQWW9g3VWdxpi5L9Iw&s=10"
            },
                new Pizza
                {
                    Nombre = "Margherita",
                    Descripcion = "Tomate, mozzarella, albahaca y aceite de oliva.",
                    Precio = 18000m,
                    Ingredientes = new List<string> { "Tomate", "Mozzarella", "Albahaca" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Rúcula & Parmesano",
                    Descripcion = "Rúcula fresca, parmesano y mozzarella.",
                    Precio = 22000m,
                    Ingredientes = new List<string> { "Rúcula", "Parmesano", "Mozzarella" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Diavola",
                    Descripcion = "Salami picante, mozzarella, ajo y un final ahumado.",
                    Precio = 24000m,
                    Ingredientes = new List<string> { "Salami picante", "Mozzarella", "Ajo" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Empanada de Carne",
                    Descripcion = "Empanada de carne suave, cebolla y especias.",
                    Precio = 1800m,
                    Ingredientes = new List<string> { "Carne", "Cebolla", "Especias" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Empanada de Jamón y Queso",
                    Descripcion = "Empanada de jamón cocido y queso.",
                    Precio = 1800m,
                    Ingredientes = new List<string> { "Jamón", "Queso" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Empanada de Pollo",
                    Descripcion = "Empanada de pollo desmenuzado y vegetales.",
                    Precio = 1800m,
                    Ingredientes = new List<string> { "Pollo", "Cebolla", "Morrones" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Empanada de Humita",
                    Descripcion = "Empanada cremosa de choclo.",
                    Precio = 1800m,
                    Ingredientes = new List<string> { "Choclo", "Cebolla", "Salsa blanca" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Empanada de Papa",
                    Descripcion = "Empanada de papa y cebolla sazonadas.",
                    Precio = 1800m,
                    Ingredientes = new List<string> { "Papa", "Cebolla" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Empanada de Verdura",
                    Descripcion = "Empanada de acelga y queso.",
                    Precio = 1800m,
                    Ingredientes = new List<string> { "Acelga", "Queso" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Coca-Cola",
                    Descripcion = "Gaseosa Coca-Cola.",
                    Precio = 2500m,
                    Ingredientes = new List<string> { "Bebida" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Sprite",
                    Descripcion = "Gaseosa Sprite.",
                    Precio = 2500m,
                    Ingredientes = new List<string> { "Bebida" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Pepsi",
                    Descripcion = "Gaseosa Pepsi.",
                    Precio = 2500m,
                    Ingredientes = new List<string> { "Bebida" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Agua",
                    Descripcion = "Agua mineral sin gas.",
                    Precio = 1500m,
                    Ingredientes = new List<string> { "Agua" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Paso de los Toros",
                    Descripcion = "Gaseosa de pomelo Paso de los Toros.",
                    Precio = 2500m,
                    Ingredientes = new List<string> { "Bebida" },
                    FechaCreacion = createdAt
                },
                new Pizza
                {
                    Nombre = "Agua con gas",
                    Descripcion = "Agua mineral con gas.",
                    Precio = 1500m,
                    Ingredientes = new List<string> { "Agua" },
                    FechaCreacion = createdAt
                }
        };

        var beverageNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Coca-Cola", "Sprite", "Pepsi", "Agua", "Paso de los Toros", "Agua con gas"
        };

        foreach (var product in pizzas)
        {
            product.Categoria = product.Nombre.StartsWith("Empanada de ", StringComparison.OrdinalIgnoreCase)
                ? "Empanadas"
                : beverageNames.Contains(product.Nombre) ? "Bebidas" : "Pizzas";
        }

        var missingPizzas = pizzas
            .Where(pizza => !existingNames.Contains(pizza.Nombre))
            .ToList();

        if (missingPizzas.Count == 0)
        {
            return;
        }

        await context.Pizzas.AddRangeAsync(missingPizzas);
        await context.SaveChangesAsync();
    }
}