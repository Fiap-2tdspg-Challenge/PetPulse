using PetPulse.Domain.entites;

namespace PetPulse.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;



/// <summary>
/// Configuração EF para a entidade Pet.
/// </summary>
public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("PP_Pets");

        builder.HasKey(pet => pet.Id);

        builder.Property(pet => pet.Id)
            .HasColumnName("ID_PET")
            .IsRequired();

        builder.Property(pet => pet.UsuarioId)
            .HasColumnName("ID_USUARIO")
            .IsRequired();

        builder.Property(pet => pet.Nome)
            .HasColumnName("NOME")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pet => pet.Especie)
            .HasColumnName("ESPECIE")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pet => pet.Raca)
            .HasColumnName("RACA")
            .HasMaxLength(100);

        builder.Property(pet => pet.DataNascimento)
            .HasColumnName("DT_NASCIMENTO");

        builder.Property(pet => pet.Peso)
            .HasColumnName("PESO")
            .HasPrecision(6, 2);

        builder.Property(pet => pet.Sexo)
            .HasColumnName("SEXO")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pet => pet.Castrado)
            .HasColumnName("CASTRADO")
            .IsRequired();

        builder.Property(pet => pet.Porte)
            .HasColumnName("PORTE")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pet => pet.CreatedAt)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();

        builder.Property(pet => pet.Active)
            .HasColumnName("ATIVO")
            .IsRequired();

        builder.HasOne(pet => pet.Usuario)
            .WithMany(usuario => usuario.Pets)
            .HasForeignKey(pet => pet.UsuarioId);

        builder.HasMany(pet => pet.HistoricosClinicos)
            .WithOne(historico => historico.Pet)
            .HasForeignKey(historico => historico.PetId);

        builder.HasMany(pet => pet.AlertasInteligentes)
            .WithOne(alerta => alerta.Pet)
            .HasForeignKey(alerta => alerta.PetId);

        builder.HasOne(pet => pet.DispositivoIot)
            .WithOne(dispositivo => dispositivo.Pet)
            .HasForeignKey<DispositivoIot>(dispositivo => dispositivo.PetId);
    }
}