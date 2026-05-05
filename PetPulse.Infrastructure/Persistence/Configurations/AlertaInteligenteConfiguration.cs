using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetPulse.Domain.entites;

namespace PetPulse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF para a entidade AlertaInteligente.
/// </summary>
public class AlertaInteligenteConfiguration : IEntityTypeConfiguration<AlertaInteligente>
{
    public void Configure(EntityTypeBuilder<AlertaInteligente> builder)
    {
        builder.ToTable("PP_AlertasInteligentes");

        builder.HasKey(alerta => alerta.Id);

        builder.Property(alerta => alerta.Id)
            .HasColumnName("ID_ALERTA")
            .IsRequired();

        builder.Property(alerta => alerta.PetId)
            .HasColumnName("ID_PET")
            .IsRequired();

        builder.Property(alerta => alerta.TipoAlerta)
            .HasColumnName("TIPO_ALERTA")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(alerta => alerta.NivelRisco)
            .HasColumnName("NIVEL_RISCO")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(alerta => alerta.OrigemAlerta)
            .HasColumnName("ORIGEM_ALERTA")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(alerta => alerta.Mensagem)
            .HasColumnName("MENSAGEM")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(alerta => alerta.Recomendacao)
            .HasColumnName("RECOMENDACAO")
            .HasMaxLength(1000);

        builder.Property(alerta => alerta.DataGeracao)
            .HasColumnName("DT_GERACAO")
            .IsRequired();

        builder.Property(alerta => alerta.Status)
            .HasColumnName("STATUS")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(alerta => alerta.CreatedAt)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();

        builder.Property(alerta => alerta.Active)
            .HasColumnName("ATIVO")
            .HasColumnType("NUMBER(1)")
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(alerta => alerta.Pet)
            .WithMany(pet => pet.AlertasInteligentes)
            .HasForeignKey(alerta => alerta.PetId);
    }
}