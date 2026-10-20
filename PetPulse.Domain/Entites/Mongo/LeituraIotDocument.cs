using MongoDB.Bson.Serialization.Attributes;

namespace PetPulse.Domain.entites.Mongo;

/// <summary>
/// Leitura de sinais vitais enviada pela coleira IoT (firmware ESP32 do PetPulse-Iot),
/// armazenada no MongoDB.
/// </summary>
/// <remarks>
/// O Oracle guarda apenas o estado ATUAL do dispositivo (última leitura, em PP_DispositivoIots).
/// O MongoDB guarda o HISTÓRICO completo das leituras: uma série temporal de alto volume, que
/// só recebe inserções e tem esquema flexível (o firmware pode passar a enviar novos sensores
/// sem migração de banco) — o caso típico de uso de NoSQL.
/// Segue o mesmo padrão de documento do repositório de referência da disciplina
/// (Recommenda: Domain/Entities/Mongo).
/// </remarks>
public class LeituraIotDocument
{
    [BsonId]
    public Guid Id { get; set; }

    /// <summary>Dispositivo que enviou a leitura (PP_DispositivoIots.ID_DISPOSITIVO no Oracle).</summary>
    public Guid DispositivoId { get; set; }

    /// <summary>Pet monitorado — duplicado no documento para consultar por pet sem precisar de join.</summary>
    public Guid PetId { get; set; }

    /// <summary>Identificador físico da coleira informado pelo firmware (ex.: COLLAR-001).</summary>
    public string? CodigoDispositivo { get; set; }

    /// <summary>Frequência cardíaca, em bpm.</summary>
    public int FrequenciaCardiaca { get; set; }

    /// <summary>Nível de atividade, de 0 a 100 (%).</summary>
    public int? NivelAtividade { get; set; }

    /// <summary>Classificação da atividade feita pelo acelerômetro (repouso, caminhada, corrida).</summary>
    public string? DescAtividade { get; set; }

    /// <summary>Aceleração total medida pelo MPU6050, em m/s².</summary>
    public double? AceleracaoTotal { get; set; }

    /// <summary>Pressão sistólica, em mmHg.</summary>
    public int? PressaoSistolica { get; set; }

    /// <summary>Pressão diastólica, em mmHg.</summary>
    public int? PressaoDiastolica { get; set; }

    /// <summary>Temperatura corporal, em °C.</summary>
    public double? Temperatura { get; set; }

    /// <summary>Score de saúde calculado pela coleira, de 0 a 100.</summary>
    public int? Score { get; set; }

    /// <summary>Estado clínico calculado pela coleira (OK, ALERTA, CRITICO).</summary>
    public string? Estado { get; set; }

    /// <summary>Mensagem de alerta gerada pela coleira, quando houver.</summary>
    public string? Alerta { get; set; }

    /// <summary>Tempo de funcionamento da coleira no momento da leitura, como informado pelo firmware.</summary>
    public long? Uptime { get; set; }

    /// <summary>Momento da leitura (UTC).</summary>
    public DateTime DataLeitura { get; set; }

    /// <summary>Momento em que a API recebeu e gravou a leitura (UTC).</summary>
    public DateTime RegistradoEm { get; set; }
}
