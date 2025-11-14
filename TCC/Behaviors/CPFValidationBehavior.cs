using TCC.Helpers;

namespace TCC.Behaviors
{
    public class CPFValidationBehavior : Behavior<Entry>
    {
        private Entry _entry;

        public static readonly BindableProperty ErrorLabelProperty =
            BindableProperty.Create(nameof(ErrorLabel), typeof(Label), typeof(CPFValidationBehavior));

        public Label ErrorLabel
        {
            get => (Label)GetValue(ErrorLabelProperty);
            set => SetValue(ErrorLabelProperty, value);
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            _entry = bindable;
            _entry.TextChanged += OnTextChanged;
            _entry.Unfocused += OnUnfocused;
            base.OnAttachedTo(bindable);

            System.Diagnostics.Debug.WriteLine("CPFValidationBehavior attached to Entry");
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            if (_entry != null)
            {
                _entry.TextChanged -= OnTextChanged;
                _entry.Unfocused -= OnUnfocused;
                _entry = null;
            }
            base.OnDetachingFrom(bindable);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_entry == null) return;

            // Limpa erro durante a digitação
            ClearError();
        }

        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CPF Entry unfocused - validating...");
            ValidateCPF();
        }

        public bool ValidateCPF()
        {
            if (_entry == null)
            {
                System.Diagnostics.Debug.WriteLine("Entry is null in ValidateCPF");
                return true;
            }

            string cpf = _entry.Text;
            System.Diagnostics.Debug.WriteLine($"Validating CPF: '{cpf}'");

            if (string.IsNullOrWhiteSpace(cpf))
            {
                ClearError();
                System.Diagnostics.Debug.WriteLine("CPF is empty - considering valid");
                return true; // Campo vazio é válido se não for obrigatório
            }

            bool isValid = CPFValidator.IsValid(cpf);
            System.Diagnostics.Debug.WriteLine($"CPF '{cpf}' validation result: {isValid}");

            if (!isValid)
            {
                ShowError("CPF inválido");
                _entry.TextColor = Colors.Red;
                return false;
            }
            else
            {
                ClearError();
                _entry.TextColor = Color.FromArgb("#333333");
                return true;
            }
        }

        private void ShowError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"Showing CPF error: {message}");

            if (ErrorLabel != null)
            {
                ErrorLabel.Text = message;
                ErrorLabel.TextColor = Colors.Red;
                ErrorLabel.IsVisible = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ErrorLabel is null - cannot show error");
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
                _entry.TextColor = Color.FromArgb("#333333");
            }
        }
    }
}