using TCC.Models;
using TCC.Services;

namespace TCC.Views
{
    public partial class UserProfilePage : ContentPage
    {
        private readonly DatabaseService _database;
        private Driver _loggedDriver;
        private Passenger _passengerToView;

        public UserProfilePage(Driver loggedDriver, Passenger passengerToView)
        {
            InitializeComponent();
            _database = new DatabaseService();
            _loggedDriver = loggedDriver;
            _passengerToView = passengerToView;

            LoadPassengerData();
        }

        private void LoadPassengerData()
        {
            if (_passengerToView == null)
                return;

            // Dados pessoais
            NameLabel.Text = _passengerToView.Name ?? "Não informado";
            EmailLabel.Text = _passengerToView.Email ?? "Não informado";
            PhoneLabel.Text = FormatPhone(_passengerToView.PhoneNumber);
            AddressLabel.Text = _passengerToView.Address ?? "Não informado";

            // Dados acadêmicos
            SchoolLabel.Text = _passengerToView.School ?? "Não informado";
            ResponsableLabel.Text = _passengerToView.ResponsableName ?? "Não informado";

            // Atendimento especial
            SpecialTreatmentLabel.Text = _passengerToView.SpecialTreatment ? "SIM" : "NÃO";
            ObservationLabel.Text = string.IsNullOrWhiteSpace(_passengerToView.SpecialTreatmentObservations)
                ? "Nenhuma observação"
                : _passengerToView.SpecialTreatmentObservations;

            // Botão deletar visível apenas para motoristas
            DeleteButton.IsVisible = _loggedDriver != null;
        }

        private string FormatPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return "Não informado";

            var numbers = new string(phone.Where(char.IsDigit).ToArray());

            if (numbers.Length == 11)
                return $"({numbers[..2]}) {numbers[2..7]}-{numbers[7..]}";

            if (numbers.Length == 10)
                return $"({numbers[..2]}) {numbers[2..6]}-{numbers[6..]}";

            return phone;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (_passengerToView == null)
                return;

            bool confirm = await DisplayAlert(
                "Confirmar Exclusão",
                $"Deseja realmente excluir o passageiro {_passengerToView.Name}?\n\nEsta ação não pode ser desfeita.",
                "Sim, Excluir",
                "Cancelar"
            );

            if (!confirm)
                return;

            try
            {
                await _database.DeletePassenger(_passengerToView);
                await DisplayAlert("Sucesso", "Passageiro excluído com sucesso.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao excluir passageiro: {ex.Message}", "OK");
            }
        }
    }
}