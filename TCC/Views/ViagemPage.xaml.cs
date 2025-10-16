using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;

namespace TCC.Views
{
    public partial class ViagemPage : ContentPage
    {
        private GraphicsOverlay _userLocationOverlay;
        private CancellationTokenSource _locationUpdateCts;
        private const int LocationUpdateIntervalMs = 1000; // Atualizar a cada 1 segundo

        public ViagemPage()
        {
            InitializeComponent();
            InitializeMapAsync();
        }

        private async Task InitializeMapAsync()
        {
            try
            {
                // Solicitar permissão de localização
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status == PermissionStatus.Granted)
                {
                    // Criar mapa com Esri OpenStreetMap Basemap
                    var map = new Esri.ArcGISRuntime.Mapping.Map(BasemapStyle.ArcGISStreets);

                    // Atribuir o mapa ao MapView
                    MyMapView.Map = map;

                    // Inicializar a camada de gráficos para o indicador de localização
                    InitializeUserLocationOverlay();

                    // Obter localização inicial e iniciar monitoramento
                    await GetInitialLocationAndStartTrackingAsync();
                }
                else
                {
                    await DisplayAlert("Permissão Negada", "É necessária permissão de localização para usar o mapa", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao inicializar o mapa: {ex.Message}", "OK");
            }
        }

        private void InitializeUserLocationOverlay()
        {
            // Criar overlay para exibir a localização do usuário
            _userLocationOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_userLocationOverlay);
        }

        private async Task GetInitialLocationAndStartTrackingAsync()
        {
            try
            {
                // Obter localização inicial
                var location = await GetUserLocationAsync();

                if (location != null)
                {
                    var userMapPoint = new MapPoint(
                        location.Longitude,
                        location.Latitude,
                        SpatialReferences.Wgs84);

                    // Navegar para a localização inicial
                    await MyMapView.SetViewpointCenterAsync(userMapPoint, 50000);

                    // Adicionar indicador na localização
                    UpdateUserLocationIndicator(userMapPoint);

                    // Iniciar rastreamento contínuo
                    await StartLocationTrackingAsync();
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível obter a localização", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao obter localização inicial: {ex.Message}", "OK");
            }
        }

        private async Task StartLocationTrackingAsync()
        {
            _locationUpdateCts = new CancellationTokenSource();

            try
            {
                while (!_locationUpdateCts.Token.IsCancellationRequested)
                {
                    var location = await GetUserLocationAsync();

                    if (location != null)
                    {
                        var userMapPoint = new MapPoint(
                            location.Longitude,
                            location.Latitude,
                            SpatialReferences.Wgs84);

                        // Atualizar indicador de localização
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            UpdateUserLocationIndicator(userMapPoint);
                        });
                    }

                    await Task.Delay(LocationUpdateIntervalMs, _locationUpdateCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Rastreamento foi cancelado
                Debug.WriteLine("Rastreamento de localização foi interrompido");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro durante rastreamento: {ex.Message}");
            }
        }

        private void UpdateUserLocationIndicator(MapPoint userLocation)
        {
            // Limpar gráficos anteriores
            _userLocationOverlay.Graphics.Clear();

            // Criar símbolo para o círculo externo (pulso/halo)
            var haloSymbol = new SimpleMarkerSymbol(
                SimpleMarkerSymbolStyle.Circle,
                System.Drawing.Color.FromArgb(100, 0, 122, 255), // Azul com transparência
                12);

            // Criar gráfico para o halo
            var haloGraphic = new Graphic(userLocation, haloSymbol);
            _userLocationOverlay.Graphics.Add(haloGraphic);

            // Criar símbolo para o ponto central (azul sólido)
            var centerMarkerSymbol = new SimpleMarkerSymbol(
                SimpleMarkerSymbolStyle.Circle,
                System.Drawing.Color.FromArgb(255, 0, 122, 255), // Azul vibrante
                8);

            // Criar gráfico para o ponto central
            var centerGraphic = new Graphic(userLocation, centerMarkerSymbol);
            _userLocationOverlay.Graphics.Add(centerGraphic);

            // Criar símbolo para a borda branca do ponto central
            var borderMarkerSymbol = new SimpleMarkerSymbol(
                SimpleMarkerSymbolStyle.Circle,
                System.Drawing.Color.FromArgb(0, 0, 0, 0), // Transparente
                8);
            borderMarkerSymbol.Outline = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid,
                System.Drawing.Color.White,
                2);

            // Criar gráfico para a borda
            var borderGraphic = new Graphic(userLocation, borderMarkerSymbol);
            _userLocationOverlay.Graphics.Add(borderGraphic);
        }

        private async Task<Location> GetUserLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(
                    GeolocationAccuracy.Best,
                    TimeSpan.FromSeconds(10));

                var location = await Geolocation.GetLocationAsync(request);

                if (location == null)
                {
                    location = await Geolocation.GetLastKnownLocationAsync();
                }

                return location;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao obter localização: {ex.Message}");
                return null;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Parar o rastreamento quando sair da página
            _locationUpdateCts?.Cancel();
        }
    }
}