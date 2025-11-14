using System;
using System.Linq;

namespace TCC.Helpers
{
    public static class RGValidator
    {
        /// <summary>
        /// Valida se o RG está no formato correto
        /// </summary>
        /// <param name="rg">RG para validar (pode conter pontos e traço)</param>
        /// <returns>True se o RG for válido, False caso contrário</returns>
        public static bool IsValid(string rg)
        {
            if (string.IsNullOrWhiteSpace(rg))
                return false;

            // Remove pontos, traços e espaços
            rg = rg.Replace(".", "").Replace("-", "").Replace(" ", "");

            // Verifica se tem 9 caracteres (8 dígitos + 1 verificador)
            if (rg.Length != 9)
                return false;

            // Verifica se todos os dígitos são iguais (RGs inválidos conhecidos)
            if (rg.All(c => c == rg[0]))
                return false;

            // Verifica se os primeiros 8 caracteres são números
            if (!rg.Substring(0, 8).All(char.IsDigit))
                return false;

            // O último caractere pode ser número ou X
            char lastChar = rg[8];
            if (!char.IsDigit(lastChar) && lastChar != 'X' && lastChar != 'x')
                return false;

            // Validação do dígito verificador (algoritmo usado em SP)
            return ValidateVerificationDigit(rg);
        }

        private static bool ValidateVerificationDigit(string rg)
        {
            try
            {
                // Converte os primeiros 8 dígitos
                int[] digits = rg.Substring(0, 8).Select(c => int.Parse(c.ToString())).ToArray();

                // Calcula o dígito verificador
                int sum = 0;
                for (int i = 0; i < 8; i++)
                {
                    sum += digits[i] * (i + 2);
                }

                int remainder = sum % 11;
                string expectedVerifier;

                if (remainder == 0)
                    expectedVerifier = "0";
                else if (remainder == 1)
                    expectedVerifier = "X";
                else
                    expectedVerifier = (11 - remainder).ToString();

                // Compara com o dígito verificador fornecido
                return rg[8].ToString().ToUpper() == expectedVerifier.ToUpper();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formata o RG para exibição (00.000.000-0)
        /// </summary>
        /// <param name="rg">RG sem formatação</param>
        /// <returns>RG formatado ou string vazia se inválido</returns>
        public static string Format(string rg)
        {
            if (string.IsNullOrWhiteSpace(rg))
                return string.Empty;

            // Remove formatação existente
            rg = rg.Replace(".", "").Replace("-", "").Replace(" ", "");

            if (rg.Length == 9)
            {
                return $"{rg.Substring(0, 2)}.{rg.Substring(2, 3)}.{rg.Substring(5, 3)}-{rg.Substring(8, 1)}";
            }

            return rg;
        }

        /// <summary>
        /// Remove a formatação do RG, deixando apenas os caracteres válidos
        /// </summary>
        /// <param name="rg">RG formatado</param>
        /// <returns>RG apenas com números e X</returns>
        public static string RemoveFormat(string rg)
        {
            if (string.IsNullOrWhiteSpace(rg))
                return string.Empty;

            return rg.Replace(".", "").Replace("-", "").Replace(" ", "").ToUpper();
        }
    }
}