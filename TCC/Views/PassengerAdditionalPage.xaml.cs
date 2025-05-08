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

            // Define a data padrão como hoje
            DataNascimentoPicker.Date = DateTime.Today;
        }

        private async void OnAddPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                // Verifica se o recurso de foto está disponível no dispositivo
                if (!MediaPicker.IsCaptureSupported)
                {
                    await DisplayAlert("Erro", "Captura de foto não suportada neste dispositivo", "OK");
                    return;
                }

                // Mostra opções para o usuário
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
                await DisplayAlert("Erro", $"Não foi possível carregar a imagem: {ex.Message}", "OK");
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
            // Validações básicas
            if (string.IsNullOrWhiteSpace(ContatoEmergenciaEntry.Text))
            {
                await DisplayAlert("Atenção", "Por favor, informe um contato de emergência", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ResponsavelEntry.Text))
            {
                await DisplayAlert("Atenção", "Por favor, informe o nome do responsável", "OK");
                return;
            }

            // Verificação adicional para atendimento especial
            if (AtendimentoSimRadio.IsChecked && string.IsNullOrWhiteSpace(AtendimentoEspecialEditor.Text))
            {
                await DisplayAlert("Atenção", "Por favor, descreva o atendimento especial necessário", "OK");
                return;
            }

            // Aqui você implementaria a lógica para salvar os dados
            // Para fins de demonstração, apenas mostramos uma mensagem de sucesso
            await DisplayAlert("Sucesso", "Dados adicionais registrados com sucesso!", "OK");

            // Navegar para a próxima página ou página principal
            // await Navigation.PushAsync(new HomePage());
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Navegar para a página de login
            await Navigation.PushAsync(new LoginPage());
        }
    }
}