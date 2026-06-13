using EnviosRapidosGT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnviosRapidosGT.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Envio> Envios => Set<Envio>();
    public DbSet<HistorialEstado> Historiales => Set<HistorialEstado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Envio>(e =>
        {
            e.HasIndex(x => x.CodigoRastreo).IsUnique();

            e.Property(x => x.PesoKg).HasColumnType("decimal(10,2)");
            e.Property(x => x.TarifaBase).HasColumnType("decimal(10,2)");
            e.Property(x => x.TarifaFinal).HasColumnType("decimal(10,2)");

            e.HasOne(x => x.Remitente)
                .WithMany()
                .HasForeignKey(x => x.RemitenteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Destinatario)
                .WithMany()
                .HasForeignKey(x => x.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Historial)
                .WithOne(h => h.Envio!)
                .HasForeignKey(h => h.EnvioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
