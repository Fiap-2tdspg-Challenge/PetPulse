namespace PetPulse.Domain.Helpers;

/// <summary>
/// Hash e verificação de senhas com BCrypt.
/// </summary>
/// <remarks>
/// O BCrypt já gera e embute um salt aleatório dentro do próprio hash — por isso
/// não é necessário (nem seguro) controlar um salt separado "na mão". Essa é uma
/// correção em relação ao repositório de referência da disciplina (Recommenda):
/// lá, <c>HashHelper.Hash</c> passava um <c>Guid.ToString("N")</c> como salt para
/// <c>BCrypt.HashPassword(texto, salt)</c> — porém esse método exige um salt no
/// formato real do BCrypt (ex.: gerado por <c>BCrypt.GenerateSalt()</c>), e um
/// Guid não é um salt válido nesse formato, o que lança <c>SaltParseException</c>
/// em tempo de execução. Usar a sobrecarga sem salt explícito evita o problema.
/// </remarks>
public static class HashHelper
{
    /// <summary>
    /// Gera o hash de uma senha em texto puro.
    /// </summary>
    public static string Hash(string senhaCrua) => BCrypt.Net.BCrypt.HashPassword(senhaCrua);

    /// <summary>
    /// Verifica se uma senha em texto puro corresponde a um hash previamente gerado.
    /// </summary>
    /// <remarks>
    /// Se <paramref name="senhaHash"/> não estiver no formato BCrypt — por exemplo, um
    /// registro antigo salvo antes desta implementação, quando a senha ainda era gravada
    /// em texto puro — o BCrypt lança exceção ao tentar interpretá-lo. Isso nunca deveria
    /// virar um 500: um hash inválido não pode ser uma senha correta, então o resultado
    /// certo é simplesmente "não confere" (login falha com 401), não uma falha da requisição.
    /// </remarks>
    public static bool Verify(string senhaCrua, string senhaHash)
    {
        if (string.IsNullOrEmpty(senhaCrua) || string.IsNullOrEmpty(senhaHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(senhaCrua, senhaHash);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
