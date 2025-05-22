using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TCC.Views
{
    public partial class DriverAdditionalPage : ContentPage
    {
        public DriverAdditionalPage()
        {
            InitializeComponent();
        }

        private async void OnAddPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecione uma foto"
                });

                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var imageSource = ImageSource.FromStream(() => stream);
                    ProfileImage.Source = imageSource;
                    AddPhotoButton.IsVisible = false; // Esconde o bot�o "+" ap�s selecionar uma foto
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"N�o foi poss�vel selecionar a foto: {ex.Message}", "OK");
            }
        }

        // Handlers para os checkboxes de Turno
        private void OnManhaTapped(object sender, EventArgs e)
        {
            ManhaCheckBox.IsChecked = !ManhaCheckBox.IsChecked;
        }

        private void OnTardeTapped(object sender, EventArgs e)
        {
            TardeCheckBox.IsChecked = !TardeCheckBox.IsChecked;
        }

        private void OnNoiteTapped(object sender, EventArgs e)
        {
            NoiteCheckBox.IsChecked = !NoiteCheckBox.IsChecked;
        }

        // Handlers para os radio buttons de Categoria CNH
        private void OnNivelDTapped(object sender, EventArgs e)
        {
            NivelDRadio.IsChecked = true;
        }

        private void OnNivelETapped(object sender, EventArgs e)
        {
            NivelERadio.IsChecked = true;
        }

        // Handlers para os radio buttons de Sexo
        private void OnMasculinoTapped(object sender, EventArgs e)
        {
            MasculinoRadio.IsChecked = true;
        }

        private void OnFemininoTapped(object sender, EventArgs e)
        {
            FemininoRadio.IsChecked = true;
        }

        // Handlers para os radio buttons de Disponibilidade de Viagem
        private void OnSimViagemTapped(object sender, EventArgs e)
        {
            SimViagemRadio.IsChecked = true;
        }

        private void OnNaoViagemTapped(object sender, EventArgs e)
        {
            NaoViagemRadio.IsChecked = true;
        }

        private async void OnRegistrarClicked(object sender, EventArgs e)
        {
            // Verifica se os campos obrigat�rios est�o preenchidos
            if (string.IsNullOrWhiteSpace(CNHEntry.Text))
            {
                await DisplayAlert("Aviso", "Por favor, preencha o n�mero da CNH.", "OK");
                return;
            }

            // Coleta os turnos selecionados
            string turnos = "";
            if (ManhaCheckBox.IsChecked) turnos += "Manh� ";
            if (TardeCheckBox.IsChecked) turnos += "Tarde ";
            if (NoiteCheckBox.IsChecked) turnos += "Noite ";

            if (string.IsNullOrWhiteSpace(turnos))
            {
                await DisplayAlert("Aviso", "Por favor, selecione pelo menos um turno.", "OK");
                return;
            }

            // Coleta outros dados do formul�rio
            string categoriaCNH = NivelDRadio.IsChecked ? "D" : "E";
            string sexo = MasculinoRadio.IsChecked ? "Masculino" : "Feminino";
            string disponibilidadeViagem = SimViagemRadio.IsChecked ? "Sim" : "N�o";

            // Aqui voc� salvaria os dados no banco de dados ou enviaria para um servi�o
            // Exemplo simplificado:
            bool success = await SalvarDadosMotorista(
                CNHEntry.Text,
                turnos.Trim(),
                categoriaCNH,
                sexo,
                ContatoEmergenciaEntry.Text,
                ExperienciaEntry.Text,
                disponibilidadeViagem,
                ObservacoesEditor.Text
            );

            if (success)
            {
                await DisplayAlert("Sucesso", "Dados adicionais registrados com sucesso!", "OK");
                // Navegar para a pr�xima p�gina ou para a p�gina inicial
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Erro", "Ocorreu um erro ao registrar os dados. Tente novamente.", "OK");
            }
        }

        private async Task<bool> SalvarDadosMotorista(string cnh, string turnos, string categoriaCNH,
            string sexo, string contatoEmergencia, string experiencia, string disponibilidadeViagem, string observacoes)
        {
            try
            {
                // Aqui voc� implementaria a l�gica de salvar no banco de dados ou enviar para API
                // Por enquanto, apenas simularemos um sucesso ap�s um breve delay
                await Task.Delay(1000);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Navegar para a p�gina de login
            await Navigation.PopAsync();
        }
    }
}