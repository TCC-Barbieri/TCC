using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using System.Timers;
using Esri.ArcGISRuntime.Symbology;

namespace TCC.Views;

public partial class ViagemPage : ContentPage
{
    private GraphicsOverlay _overlay;
    private Graphic _driverGraphic;
    private System.Timers.Timer _locationTimer;

    public ViagemPage()
    {
        InitializeComponent();
        InitializeMap();
        StartLocationUpdates();
    }

    private void InitializeMap()
    {
        MyMapView.Map = new Esri.ArcGISRuntime.Mapping.Map(BasemapStyle.ArcGISStreets);

        _overlay = new GraphicsOverlay();
        MyMapView.GraphicsOverlays.Add(_overlay);
    }

    private void StartLocationUpdates()
    {
        _locationTimer = new System.Timers.Timer(5000); // a cada 5 segundos
        _locationTimer.Elapsed += async (s, e) => await UpdateDriverLocation();
        _locationTimer.Start();
    }

    private async Task UpdateDriverLocation()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                var mapPoint = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_driverGraphic == null)
                    {
                        _driverGraphic = new Graphic(mapPoint, new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 15));
                        _overlay.Graphics.Add(_driverGraphic);
                    }
                    else
                    {
                        _driverGraphic.Geometry = mapPoint;
                    }

                    MyMapView.SetViewpointCenterAsync(mapPoint, 10000);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao obter localização: {ex.Message}");
        }
    }
}
