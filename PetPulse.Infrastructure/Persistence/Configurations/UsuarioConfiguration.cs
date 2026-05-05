using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetPulse.Domain.entites;

namespace PetPulse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF para a entidade Usuario.
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
       builder.ToTable("PP_Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Id)
            .HasColumnName("ID_USUARIO")
            .IsRequired();

        builder.Property(usuario => usuario.Nome)
            .HasColumnName("NOME")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(usuario => usuario.Cpf)
            .HasColumnName("CPF")
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(usuario => usuario.Email)
            .HasColumnName("EMAIL")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(usuario => usuario.Senha)
            .HasColumnName("SENHA")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(usuario => usuario.Telefone)
            .HasColumnName("TELEFONE")
            .HasMaxLength(20);

        builder.Property(usuario => usuario.Endereco)
            .HasColumnName("ENDERECO")
            .HasMaxLength(255);

        builder.Property(usuario => usuario.CreatedAt)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();

        builder.Property(usuario => usuario.Active)
            .HasColumnName("ATIVO")
            .HasColumnType("NUMBER(1)")
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(usuario => usuario.Cpf)
            .IsUnique();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique();

        builder.HasMany(usuario => usuario.Pets)
            .WithOne(pet => pet.Usuario)
            .HasForeignKey(pet => pet.UsuarioId);
    }
}