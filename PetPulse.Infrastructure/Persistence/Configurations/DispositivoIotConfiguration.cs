using PetPulse.Domain.entites;

namespace PetPulse.Infrastructure.Persistence.Configurations;


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


/// <summary>
/// Configuração EF para a entidade DispositivoIot.
/// </summary>
public class DispositivoIotConfiguration : IEntityTypeConfiguration<DispositivoIot>
{
    public void Configure(EntityTypeBuilder<DispositivoIot> builder)
    {
        builder.ToTable("T_CLY_DISPOSITIVO_IOT");

        builder.HasKey(dispositivo => dispositivo.Id);

        builder.Property(dispositivo => dispositivo.Id)
            .HasColumnName("ID_DISPOSITIVO")
            .IsRequired();

        builder.Property(dispositivo => dispositivo.PetId)
            .HasColumnName("ID_PET")
            .IsRequired();

        builder.Property(dispositivo => dispositivo.DataVinculacao)
            .HasColumnName("DT_VINCULACAO")
            .IsRequired();

        builder.Property(dispositivo => dispositivo.IntervaloColetaMinutos)
            .HasColumnName("INTERVALO_COLETA_MINUTOS");

        builder.Property(dispositivo => dispositivo.FrequenciaCardiaca)
            .HasColumnName("FREQUENCIA_CARDIACA");

        builder.Property(dispositivo => dispositivo.NivelAtividade)
            .HasColumnName("NIVEL_ATIVIDADE")
            .HasPrecision(5, 2);

        builder.Property(dispositivo => dispositivo.Pressao)
            .HasColumnName("PRESSAO")
            .HasPrecision(6, 2);

        builder.Property(dispositivo => dispositivo.DataUltimaLeitura)
            .HasColumnName("DT_ULTIMA_LEITURA");

        builder.Property(dispositivo => dispositivo.Status)
            .HasColumnName("STATUS")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(dispositivo => dispositivo.CreatedAt)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();

        builder.Property(dispositivo => dispositivo.Active)
            .HasColumnName("ATIVO")
            .IsRequired();

        builder.HasIndex(dispositivo => dispositivo.PetId)
            .IsUnique();

        builder.HasOne(dispositivo => dispositivo.Pet)
            .WithOne(pet => pet.DispositivoIot)
            .HasForeignKey<DispositivoIot>(dispositivo => dispositivo.PetId);
    }
}