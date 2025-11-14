public static class CNHValidator
{
    public static bool IsValid(string cnh)
    {
        if (string.IsNullOrWhiteSpace(cnh))
            return false;

        cnh = cnh.Replace(" ", "");

        if (cnh.Length != 11)
            return false;

        if (!cnh.All(char.IsDigit))
            return false;

        if (cnh.All(c => c == cnh[0]))
            return false;

        int[] digits = cnh.Select(c => int.Parse(c.ToString())).ToArray();

        // Primeiro dígito
        int sum = 0;
        int multiplier = 9;

        for (int i = 0; i < 9; i++)
        {
            sum += digits[i] * multiplier;
            multiplier--;
        }

        int firstVerifier = sum % 11;
        if (firstVerifier >= 10)
            firstVerifier = 0;

        if (digits[9] != firstVerifier)
            return false;

        // Segundo dígito (corrigido)
        sum = 0;
        multiplier = 1;

        for (int i = 0; i < 9; i++)
        {
            sum += digits[i] * multiplier;
            multiplier++;
        }

        // Inclui o primeiro dígito verificador
        sum += firstVerifier * multiplier;

        int secondVerifier = sum % 11;
        if (secondVerifier >= 10)
            secondVerifier = 0;

        return digits[10] == secondVerifier;
    }

    public static string Format(string cnh)
    {
        if (string.IsNullOrWhiteSpace(cnh))
            return string.Empty;

        cnh = cnh.Replace(" ", "");

        if (cnh.Length == 11 && cnh.All(char.IsDigit))
        {
            return $"{cnh.Substring(0, 3)} {cnh.Substring(3, 3)} {cnh.Substring(6, 3)} {cnh.Substring(9, 2)}";
        }

        return cnh;
    }

    public static string RemoveFormat(string cnh)
    {
        if (string.IsNullOrWhiteSpace(cnh))
            return string.Empty;

        return cnh.Replace(" ", "");
    }
}
