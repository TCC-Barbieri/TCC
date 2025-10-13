using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace TCC.Views
{
    public partial class ViagemPage : ContentPage
    {
        public ViagemPage()
        {
            InitializeComponent();
            InitializeMapAsync();
        }

        private async Task InitializeMapAsync()
        {
            try
            {
                // Solicitar permiss�o de localiza��o
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status == PermissionStatus.Granted)
                {
                    // Obter localiza��o atual do usu�rio
                    var location = await GetUserLocationAsync();

                    if (location != null)
                    {
                        // Criar mapa com Esri OpenStreetMap Basemap
                        var map = new Esri.ArcGISRuntime.Mapping.Map(BasemapStyle.ArcGISStreets);

                        // Atribuir o mapa ao MapView
                        MyMapView.Map = map;

                        // Criar ponto com as coordenadas do usu�rio
                        var userLocation = new MapPoint(
                            location.Longitude,
                            location.Latitude,
                            SpatialReferences.Wgs84);

                        // Navegar para a localiza��o do usu�rio
                        await MyMapView.SetViewpointCenterAsync(userLocation, 50000); // 50000 � o zoom level
                    }
                    else
                    {
                        await DisplayAlert("Erro", "N�o foi poss�vel obter a localiza��o", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Permiss�o Negada", "� necess�ria permiss�o de localiza��o para usar o mapa", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao inicializar o mapa: {ex.Message}", "OK");
            }
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
                    // Se n�o conseguir em tempo real, tentar com a �ltima localiza��o conhecida
                    location = await Geolocation.GetLastKnownLocationAsync();
                }

                return location;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter localiza��o: {ex.Message}");
                return null;
            }
        }
    }
}