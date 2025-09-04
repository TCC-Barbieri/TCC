using Microsoft.Maui.Controls;
using System;
using TCC.Helpers;

namespace TCC.Behaviors
{
    public class CPFValidationBehavior : Behavior<Entry>
    {
        private Entry _entry;
        private Label _errorLabel;

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
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            _entry.TextChanged -= OnTextChanged;
            _entry.Unfocused -= OnUnfocused;
            _entry = null;
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
            ValidateCPF();
        }

        public bool ValidateCPF()
        {
            if (_entry == null) return true;

            string cpf = _entry.Text;

            if (string.IsNullOrWhiteSpace(cpf))
            {
                ClearError();
                return true; // Campo vazio é válido se não for obrigatório
            }

            bool isValid = CPFValidator.IsValid(cpf);

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
                _entry.TextColor = Color.FromArgb("#333333");
            }
        }
    }
}