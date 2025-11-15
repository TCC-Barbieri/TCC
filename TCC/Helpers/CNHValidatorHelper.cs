using System;
using System.Linq;
using System.Text.RegularExpressions;

public static class CNHValidatorHelper
{
    private static string OnlyDigits(string s) =>
        string.IsNullOrWhiteSpace(s) ? string.Empty : Regex.Replace(s, @"\D", "");

    public static bool IsValid(string cnh)
    {
        cnh = OnlyDigits(cnh);
        if (cnh.Length != 11) return false;
        if (cnh.All(c => c == cnh[0])) return false;

        int[] d = cnh.Select(ch => ch - '0').ToArray();

        // 1º DV: pesos 9..1
        int[] w1 = { 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        int sum1 = 0;
        for (int i = 0; i < 9; i++) sum1 += d[i] * w1[i];
        int r1 = sum1 % 11;
        int dv1 = (r1 < 2) ? 0 : 11 - r1;
        if (d[9] != dv1) return false;

        // 2º DV: pesos 10..2 (inclui o primeiro DV)
        int[] w2 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int sum2 = 0;
        for (int i = 0; i < 9; i++) sum2 += d[i] * w2[i];
        sum2 += dv1 * 2; // último peso é 2 (para o 10º dígito)
        int r2 = sum2 % 11;
        int dv2 = (r2 < 2) ? 0 : 11 - r2;
        return d[10] == dv2;
    }

    public static string Format(string cnh)
    {
        cnh = OnlyDigits(cnh);
        if (cnh.Length != 11) return cnh;
        return $"{cnh.Substring(0, 3)} {cnh.Substring(3, 3)} {cnh.Substring(6, 3)} {cnh.Substring(9, 2)}";
    }

    public static string RemoveFormat(string cnh) => OnlyDigits(cnh);
}
