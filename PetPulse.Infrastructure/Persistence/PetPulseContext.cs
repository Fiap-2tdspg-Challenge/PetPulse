using Microsoft.EntityFrameworkCore;
using PetPulse.Domain.entites;

namespace PetPulse.Infrastructure.Persistence;


/// <summary>
/// Contexto EF Core do projeto Clyvo Vet.
/// Configura o mapeamento das entidades para o banco de dados.
/// </summary>
public class PetPulseContext(DbContextOptions<PetPulseContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Pet> Pets { get; set; }

    public DbSet<HistoricoClinico> HistoricosClinicos { get; set; }

    public DbSet<DispositivoIot> DispositivosIot { get; set; }

    public DbSet<AlertaInteligente> AlertasInteligentes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetPulseContext).Assembly);
    }
}