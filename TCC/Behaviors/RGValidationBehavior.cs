using Microsoft.Maui.Controls;
using TCC.Helpers;

namespace TCC.Behaviors
{
    public class RGValidationBehavior : Behavior<Entry>
    {
        private Entry _entry;

        public static readonly BindableProperty ErrorLabelProperty =
            BindableProperty.Create(nameof(ErrorLabel), typeof(Label), typeof(RGValidationBehavior));

        public Label ErrorLabel
        {
            get => (Label)GetValue(ErrorLabelProperty);
            set => SetValue(ErrorLabelProperty, value);
        }

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            _entry = entry;
            _entry.TextChanged += OnTextChanged;
            _entry.Unfocused += OnUnfocused;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            base.OnDetachingFrom(entry);
            if (_entry != null)
            {
                _entry.TextChanged -= OnTextChanged;
                _entry.Unfocused -= OnUnfocused;
                _entry = null;
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Limpa o erro enquanto digita e mantém a cor preta
            ClearError();
        }

        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            ValidateRG();
        }

        public bool ValidateRG()
        {
            if (_entry == null)
                return true;

            string rg = _entry.Text;

            if (string.IsNullOrWhiteSpace(rg))
            {
                ClearError();
                return true; // Campo vazio é válido se não for obrigatório
            }

            bool isValid = RGValidatorHelper.IsValid(rg);

            if (!isValid)
            {
                ShowError("RG inválido");
                _entry.TextColor = Colors.Red;
                return false;
            }
            else
            {
                ClearError();
                return true;
            }
        }

        private void ShowError(string message)
        {
            if (ErrorLabel != null)
            {
                ErrorLabel.Text = message;
                ErrorLabel.TextColor = Colors.Red;
                ErrorLabel.IsVisible = true;
            }
        }

        private void ClearError()
        {
            if (ErrorLabel != null)
            {
                ErrorLabel.Text = string.Empty;
                ErrorLabel.IsVisible = false;
            }

            if (_entry != null)
            {
                _entry.TextColor = Color.FromArgb("#333333"); // Preto padrão
            }
        }
    }
}