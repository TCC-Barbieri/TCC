using TCC.Helpers;

namespace TCC.Behaviors
{
    public class PhoneValidationBehavior : Behavior<Entry>
    {
        private Entry _entry;

        public static readonly BindableProperty ValidColorProperty =
            BindableProperty.Create(nameof(ValidColor), typeof(Color), typeof(PhoneValidationBehavior), Color.FromArgb("#333333"));

        public static readonly BindableProperty InvalidColorProperty =
            BindableProperty.Create(nameof(InvalidColor), typeof(Color), typeof(PhoneValidationBehavior), Colors.Red);

        public Color ValidColor
        {
            get => (Color)GetValue(ValidColorProperty);
            set => SetValue(ValidColorProperty, value);
        }

        public Color InvalidColor
        {
            get => (Color)GetValue(InvalidColorProperty);
            set => SetValue(InvalidColorProperty, value);
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

            // Mantém a cor preta durante a digitação
            _entry.TextColor = ValidColor;
        }

        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            ValidatePhone();
        }

        public bool ValidatePhone()
        {
            if (_entry == null)
                return true;

            string phone = _entry.Text;

            if (string.IsNullOrWhiteSpace(phone))
            {
                _entry.TextColor = ValidColor;
                return true; // Campo vazio é válido se não for obrigatório
            }

            bool isValid = PhoneValidationHelper.IsValidPhone(phone);

            // Define a cor baseado na validação
            _entry.TextColor = isValid ? ValidColor : InvalidColor;

            return isValid;
        }
    }
}