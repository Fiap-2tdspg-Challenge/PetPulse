using PetPulse.Domain.entites;

namespace PetPulse.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;



/// <summary>
/// Configuração EF para a entidade HistoricoClinico.
/// </summary>
public class HistoricoClinicoConfiguration : IEntityTypeConfiguration<HistoricoClinico>
{
    public void Configure(EntityTypeBuilder<HistoricoClinico> builder)
    {
        builder.ToTable("PP_HistoricoClinicos");

        builder.HasKey(historico => historico.Id);

        builder.Property(historico => historico.Id)
            .HasColumnName("ID_HISTORICO")
            .IsRequired();

        builder.Property(historico => historico.PetId)
            .HasColumnName("ID_PET")
            .IsRequired();

        builder.Property(historico => historico.TipoRegistro)
            .HasColumnName("TIPO_REGISTRO")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(historico => historico.Descricao)
            .HasColumnName("DESCRICAO")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(historico => historico.DataRegistro)
            .HasColumnName("DT_REGISTRO")
            .IsRequired();

        builder.Property(historico => historico.DataRetorno)
            .HasColumnName("DT_RETORNO");

        builder.Property(historico => historico.ProfissionalClinica)
            .HasColumnName("PROFISSIONAL_CLINICA")
            .HasMaxLength(150);

        builder.Property(historico => historico.Observacoes)
            .HasColumnName("OBSERVACOES")
            .HasMaxLength(1000);

        builder.Property(historico => historico.CreatedAt)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();

        builder.Property(historico => historico.Active)
            .HasColumnName("ATIVO")
            .HasColumnType("NUMBER(1)")
            .HasConversion<int>()
            .IsRequired();
        
        builder.HasOne(historico => historico.Pet)
            .WithMany(pet => pet.HistoricosClinicos)
            .HasForeignKey(historico => historico.PetId);
    }
}