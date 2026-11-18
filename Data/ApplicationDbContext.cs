using Microsoft.EntityFrameworkCore;
using ApiwebMVC.Models;

namespace ApiwebMVC.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pizza> Pizzas => Set<Pizza>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pizza>()
            .Property(p => p.Ingredientes)
            .IsRequired(false)
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v),
                v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v))
            .Metadata.SetValueComparer(
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (left, right) => ReferenceEquals(left, right) || (left != null && right != null && left.SequenceEqual(right)),
                    value => value == null ? 0 : value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    value => value == null ? null! : value.ToList()));

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.GoogleSubject)
            .IsUnique();
    }
}
