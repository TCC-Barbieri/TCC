using Microsoft.Maui.Controls;
using TCC.Helpers;

namespace TCC.Behaviors
{
    public class CNHValidationBehavior : Behavior<Entry>
    {
        public static readonly BindableProperty ErrorLabelProperty =
            BindableProperty.Create(nameof(ErrorLabel), typeof(Label), typeof(CNHValidationBehavior));

        public Label ErrorLabel
        {
            get => (Label)GetValue(ErrorLabelProperty);
            set => SetValue(ErrorLabelProperty, value);
        }

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            entry.TextChanged += OnTextChanged;
            entry.Unfocused += OnUnfocused;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            base.OnDetachingFrom(entry);
            entry.TextChanged -= OnTextChanged;
            entry.Unfocused -= OnUnfocused;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Limpa o erro enquanto digita
            if (ErrorLabel != null)
            {
                ErrorLabel.IsVisible = false;
            }
        }

        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            ValidateCNH();
        }

        public void ValidateCNH()
        {
            var entry = (Entry)BindingContext;
            if (BindingContext is Entry bindingEntry)
            {
                entry = bindingEntry;
            }
            else if (ErrorLabel?.Parent is VisualElement parent)
            {
                entry = FindEntry(parent);
            }

            if (entry == null) return;

            string cnh = entry.Text;

            if (string.IsNullOrWhiteSpace(cnh))
            {
                if (ErrorLabel != null)
                {
                    ErrorLabel.Text = "";
                    ErrorLabel.IsVisible = false;
                }
                entry.TextColor = Colors.Black;
                return;
            }

            bool isValid = CNHValidator.IsValid(cnh);

            if (ErrorLabel != null)
            {
                if (!isValid)
                {
                    ErrorLabel.Text = "CNH inválida";
                    ErrorLabel.IsVisible = true;
                    entry.TextColor = Colors.Red;
                }
                else
                {
                    ErrorLabel.Text = "";
                    ErrorLabel.IsVisible = false;
                    entry.TextColor = Colors.Green;
                }
            }
        }

        private Entry FindEntry(VisualElement parent)
        {
            if (parent is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is Entry entry)
                        return entry;
                    if (child is VisualElement ve)
                    {
                        var found = FindEntry(ve);
                        if (found != null) return found;
                    }
                }
            }
            return null;
        }
    }
}