using TCC.Helpers;

namespace TCC.Behaviors;

/// <summary>
/// Behavior para validação de telefone em tempo real
/// </summary>
public class PhoneValidationBehavior : Behavior<Entry>
{
    private Entry? _entry;

    // Propriedades bindáveis para customização
    public static readonly BindableProperty ValidColorProperty =
        BindableProperty.Create(nameof(ValidColor), typeof(Color), typeof(PhoneValidationBehavior), Colors.Black);

    public static readonly BindableProperty InvalidColorProperty =
        BindableProperty.Create(nameof(InvalidColor), typeof(Color), typeof(PhoneValidationBehavior), Colors.Red);

    public static readonly BindableProperty ShowBorderProperty =
        BindableProperty.Create(nameof(ShowBorder), typeof(bool), typeof(PhoneValidationBehavior), false);

    public static readonly BindableProperty AutoFormatProperty =
        BindableProperty.Create(nameof(AutoFormat), typeof(bool), typeof(PhoneValidationBehavior), true);

    /// <summary>
    /// Cor do texto quando o telefone é válido
    /// </summary>
    public Color ValidColor
    {
        get => (Color)GetValue(ValidColorProperty);
        set => SetValue(ValidColorProperty, value);
    }

    /// <summary>
    /// Cor do texto quando o telefone é inválido
    /// </summary>
    public Color InvalidColor
    {
        get => (Color)GetValue(InvalidColorProperty);
        set => SetValue(InvalidColorProperty, value);
    }

    /// <summary>
    /// Define se deve mostrar borda colorida
    /// </summary>
    public bool ShowBorder
    {
        get => (bool)GetValue(ShowBorderProperty);
        set => SetValue(ShowBorderProperty, value);
    }

    /// <summary>
    /// Define se deve formatar automaticamente o telefone
    /// </summary>
    public bool AutoFormat
    {
        get => (bool)GetValue(AutoFormatProperty);
        set => SetValue(AutoFormatProperty, value);
    }

    /// <summary>
    /// Propriedade para verificar se o telefone é válido externamente
    /// </summary>
    public bool IsValid { get; private set; }

    protected override void OnAttachedTo(Entry entry)
    {
        base.OnAttachedTo(entry);
        _entry = entry;

        // Configura o teclado para telefone
        entry.Keyboard = Keyboard.Telephone;

        // Adiciona evento de mudança de texto
        entry.TextChanged += OnEntryTextChanged;
        entry.Unfocused += OnEntryUnfocused;
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        base.OnDetachingFrom(entry);

        // Remove eventos
        entry.TextChanged -= OnEntryTextChanged;
        entry.Unfocused -= OnEntryUnfocused;

        _entry = null;
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_entry == null || string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            ResetValidation();
            return;
        }

        // Valida o telefone
        IsValid = PhoneValidationHelper.IsValidPhone(e.NewTextValue);

        // Aplica a cor baseada na validação
        _entry.TextColor = IsValid ? ValidColor : InvalidColor;

        // Aplica borda se configurado
        if (ShowBorder)
        {
            // Note: Isso funciona melhor se o Entry estiver dentro de um Frame
            // ou se você estiver usando um controle customizado
        }
    }

    private void OnEntryUnfocused(object? sender, FocusEventArgs e)
    {
        if (_entry == null || string.IsNullOrWhiteSpace(_entry.Text))
            return;

        // Formata automaticamente quando perde o foco (se configurado)
        if (AutoFormat && IsValid)
        {
            string formattedPhone = PhoneValidationHelper.FormatPhone(_entry.Text);
            if (_entry.Text != formattedPhone)
            {
                _entry.Text = formattedPhone;
            }
        }
    }

    private void ResetValidation()
    {
        if (_entry != null)
        {
            _entry.TextColor = ValidColor;
            IsValid = false;
        }
    }
}

/// <summary>
/// Behavior simplificado que apenas formata o telefone
/// </summary>
public class PhoneFormatterBehavior : Behavior<Entry>
{
    private Entry? _entry;

    protected override void OnAttachedTo(Entry entry)
    {
        base.OnAttachedTo(entry);
        _entry = entry;
        entry.Keyboard = Keyboard.Telephone;
        entry.Unfocused += OnEntryUnfocused;
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        base.OnDetachingFrom(entry);
        entry.Unfocused -= OnEntryUnfocused;
        _entry = null;
    }

    private void OnEntryUnfocused(object? sender, FocusEventArgs e)
    {
        if (_entry != null && !string.IsNullOrWhiteSpace(_entry.Text))
        {
            _entry.Text = PhoneValidationHelper.FormatPhone(_entry.Text);
        }
    }
}