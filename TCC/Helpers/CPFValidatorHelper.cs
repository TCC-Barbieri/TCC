using System;
using System.Linq;

namespace TCC.Helpers
{
    public static class CPFValidator
    {
        /// <summary>
        /// Valida se o CPF está no formato correto e se os dígitos verificadores estão corretos
        /// </summary>
        /// <param name="cpf">CPF para validar (pode conter pontos e traço)</param>
        /// <returns>True se o CPF for válido, False caso contrário</returns>
        public static bool IsValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove pontos, traços e espaços
            cpf = cpf.Replace(".", "").Replace("-", "").Replace(" ", "");

            // Verifica se tem 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais (CPFs inválidos conhecidos)
            if (cpf.All(c => c == cpf[0]))
                return false;

            // Verifica se todos os caracteres são números
            if (!cpf.All(char.IsDigit))
                return false;

            // Converte para array de inteiros
            int[] digits = cpf.Select(c => int.Parse(c.ToString())).ToArray();

            // Calcula o primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += digits[i] * (10 - i);
            }

            int remainder = sum % 11;
            int firstVerifier = remainder < 2 ? 0 : 11 - remainder;

            // Verifica se o primeiro dígito verificador está correto
            if (digits[9] != firstVerifier)
                return false;

            // Calcula o segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += digits[i] * (11 - i);
            }

            remainder = sum % 11;
            int secondVerifier = remainder < 2 ? 0 : 11 - remainder;

            // Verifica se o segundo dígito verificador está correto
            return digits[10] == secondVerifier;
        }

        /// <summary>
        /// Formata o CPF para exibição (000.000.000-00)
        /// </summary>
        /// <param name="cpf">CPF sem formatação</param>
        /// <returns>CPF formatado ou string vazia se inválido</returns>
        public static string Format(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            // Remove formatação existente
            cpf = cpf.Replace(".", "").Replace("-", "").Replace(" ", "");

            if (cpf.Length == 11 && cpf.All(char.IsDigit))
            {
                return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
            }

            return cpf;
        }

        /// <summary>
        /// Remove a formatação do CPF, deixando apenas os números
        /// </summary>
        /// <param name="cpf">CPF formatado</param>
        /// <returns>CPF apenas com números</returns>
        public static string RemoveFormat(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            return cpf.Replace(".", "").Replace("-", "").Replace(" ", "");
        }
    }
}