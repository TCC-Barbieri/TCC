using System.Text.RegularExpressions;

namespace TCC.Behaviors
{
    /// <summary>
    /// Behavior que permite apenas números em um Entry
    /// </summary>
    public class NumericValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry)
                return;

            if (string.IsNullOrWhiteSpace(e.NewTextValue))
                return;

            // Remove qualquer caractere que não seja número
            var isValid = Regex.IsMatch(e.NewTextValue, @"^[0-9]+$");

            if (!isValid)
            {
                // Remove todos os caracteres não numéricos
                string numericText = Regex.Replace(e.NewTextValue, @"[^0-9]", "");
                entry.Text = numericText;
            }
        }
    }
}