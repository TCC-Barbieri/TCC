using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TCC.Views
{
    public partial class PassengerAdditionalPage : ContentPage
    {
        private string _photoPath;

        public PassengerAdditionalPage()
        {
            InitializeComponent();

            // Define a data padr�o como hoje
            DataNascimentoPicker.Date = DateTime.Today;
        }

        private async void OnAddPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                // Verifica se o recurso de foto est� dispon�vel no dispositivo
                if (!MediaPicker.IsCaptureSupported)
                {
                    await DisplayAlert("Erro", "Captura de foto n�o suportada neste dispositivo", "OK");
                    return;
                }

                // Mostra op��es para o usu�rio
                string action = await DisplayActionSheet("Foto do passageiro", "Cancelar", null, "Tirar foto", "Escolher da galeria");

                FileResult photo = null;

                switch (action)
                {
                    case "Tirar foto":
                        photo = await MediaPicker.CapturePhotoAsync();
                        break;
                    case "Escolher da galeria":
                        photo = await MediaPicker.PickPhotoAsync();
                        break;
                    default:
                        return;
                }

                if (photo != null)
                {
                    // Salva a foto localmente
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);
                    await sourceStream.CopyToAsync(localFileStream);

                    // Salva o caminho e mostra a imagem
                    _photoPath = localFilePath;
                    ProfileImage.Source = ImageSource.FromFile(localFilePath);
                    AddPhotoButton.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"N�o foi poss�vel carregar a imagem: {ex.Message}", "OK");
            }
        }

        private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
        {
            // Mostrar ou ocultar campos adicionais de atendimento especial
            AtendimentoEspecialDetalhesLayout.IsVisible = e.Value;
        }

        private void OnSimTapped(object sender, TappedEventArgs e)
        {
            AtendimentoSimRadio.IsChecked = true;
            AtendimentoNaoRadio.IsChecked = false;
            AtendimentoEspecialDetalhesLayout.IsVisible = true;
        }

        private void OnNaoTapped(object sender, TappedEventArgs e)
        {
            AtendimentoSimRadio.IsChecked = false;
            AtendimentoNaoRadio.IsChecked = true;
            AtendimentoEspecialDetalhesLayout.IsVisible = false;
        }

        private async void OnRegistrarClicked(object sender, EventArgs e)
        {
            // Valida��es b�sicas
            if (string.IsNullOrWhiteSpace(ContatoEmergenciaEntry.Text))
            {
                await DisplayAlert("Aten��o", "Por favor, informe um contato de emerg�ncia", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ResponsavelEntry.Text))
            {
                await DisplayAlert("Aten��o", "Por favor, informe o nome do respons�vel", "OK");
                return;
            }

            // Verifica��o adicional para atendimento especial
            if (AtendimentoSimRadio.IsChecked && string.IsNullOrWhiteSpace(AtendimentoEspecialEditor.Text))
            {
                await DisplayAlert("Aten��o", "Por favor, descreva o atendimento especial necess�rio", "OK");
                return;
            }

            // Aqui voc� implementaria a l�gica para salvar os dados
            // Para fins de demonstra��o, apenas mostramos uma mensagem de sucesso
            await DisplayAlert("Sucesso", "Dados adicionais registrados com sucesso!", "OK");

            // Navegar para a pr�xima p�gina ou p�gina principal
            // await Navigation.PushAsync(new HomePage());
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Navegar para a p�gina de login
            await Navigation.PushAsync(new LoginPage());
        }
    }
}