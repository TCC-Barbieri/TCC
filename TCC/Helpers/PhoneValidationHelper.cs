namespace TCC.Helpers;

/// <summary>
/// Helper para validação de números de telefone brasileiros
/// </summary>
public static class PhoneValidationHelper
{
    /// <summary>
    /// Valida se um número de telefone brasileiro é válido
    /// </summary>
    /// <param name="phone">Número de telefone (pode conter formatação)</param>
    /// <returns>True se válido, False se inválido</returns>
    public static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Remove todos os caracteres não numéricos
        string numbers = GetOnlyNumbers(phone);

        // Telefone brasileiro deve ter 10 (fixo) ou 11 (celular) dígitos
        if (numbers.Length != 10 && numbers.Length != 11)
            return false;

        // Verifica se o DDD é válido (11 a 99)
        if (numbers.Length >= 2)
        {
            int ddd = int.Parse(numbers.Substring(0, 2));
            if (ddd < 11 || ddd > 99)
                return false;
        }

        // Para celular (11 dígitos), o terceiro dígito deve ser 9
        if (numbers.Length == 11)
        {
            if (numbers[2] != '9')
                return false;
        }

        // Verifica se não é um número com todos dígitos iguais (ex: 11111111111)
        if (numbers.Distinct().Count() == 1)
            return false;

        return true;
    }

    /// <summary>
    /// Remove todos os caracteres não numéricos de uma string
    /// </summary>
    /// <param name="text">Texto a ser processado</param>
    /// <returns>String contendo apenas dígitos</returns>
    public static string GetOnlyNumbers(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return new string(text.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Formata um número de telefone brasileiro
    /// </summary>
    /// <param name="phone">Número sem formatação</param>
    /// <returns>Número formatado</returns>
    public static string FormatPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
            return string.Empty;

        string numbers = GetOnlyNumbers(phone);

        if (numbers.Length == 11)
        {
            // Celular: (XX) 9XXXX-XXXX
            return $"({numbers.Substring(0, 2)}) {numbers.Substring(2, 5)}-{numbers.Substring(7, 4)}";
        }
        else if (numbers.Length == 10)
        {
            // Fixo: (XX) XXXX-XXXX
            return $"({numbers.Substring(0, 2)}) {numbers.Substring(2, 4)}-{numbers.Substring(6, 4)}";
        }

        return phone;
    }

    /// <summary>
    /// Obtém uma mensagem de erro amigável para telefone inválido
    /// </summary>
    /// <returns>Mensagem de erro</returns>
    public static string GetValidationErrorMessage()
    {
        return "Número de telefone inválido. Por favor, insira um número de telefone brasileiro válido:\n\n" +
               "• Celular: (XX) 9XXXX-XXXX (11 dígitos)";
    }

    /// <summary>
    /// Verifica se o telefone é celular (11 dígitos)
    /// </summary>
    /// <param name="phone">Número de telefone</param>
    /// <returns>True se for celular</returns>
    public static bool IsCellPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        string numbers = GetOnlyNumbers(phone);
        return numbers.Length == 11 && numbers[2] == '9';
    }

    /// <summary>
    /// Verifica se o telefone é fixo (10 dígitos)
    /// </summary>
    /// <param name="phone">Número de telefone</param>
    /// <returns>True se for telefone fixo</returns>
    public static bool IsLandline(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        string numbers = GetOnlyNumbers(phone);
        return numbers.Length == 10;
    }

    /// <summary>
    /// Extrai o DDD do telefone
    /// </summary>
    /// <param name="phone">Número de telefone</param>
    /// <returns>DDD ou string vazia</returns>
    public static string GetDDD(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        string numbers = GetOnlyNumbers(phone);

        if (numbers.Length >= 2)
            return numbers.Substring(0, 2);

        return string.Empty;
    }
}