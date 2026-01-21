using Microsoft.EntityFrameworkCore;
using BethersaApp.Models;
using System.IO;

namespace BethersaApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Vendedor> Vendedores { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<DetallePedido> DetallesPedido { get; set; }
    public DbSet<MovimientoStock> MovimientosStock { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bethersa.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vendedor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Vendedor)
                  .WithMany(v => v.Clientes)
                  .HasForeignKey(e => e.VendedorId)
                  .IsRequired(false);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PorcentajeDescuento).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Categoria)
                  .WithMany(c => c.Productos)
                  .HasForeignKey(e => e.CategoriaId)
                  .IsRequired(false);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Pedidos)
                  .HasForeignKey(e => e.ClienteId);
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Pedido)
                  .WithMany(p => p.Detalles)
                  .HasForeignKey(e => e.PedidoId);
            entity.HasOne(e => e.Producto)
                  .WithMany(p => p.DetallesPedido)
                  .HasForeignKey(e => e.ProductoId)
                  .IsRequired(false);
        });

        modelBuilder.Entity<MovimientoStock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Producto)
                  .WithMany()
                  .HasForeignKey(e => e.ProductoId);
            entity.HasOne(e => e.Pedido)
                  .WithMany()
                  .HasForeignKey(e => e.PedidoId)
                  .IsRequired(false);
        });
    }
}
