using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using TCC.Models;
using TCC.Services;

namespace TCC.Views
{
    public partial class ViagemPage : ContentPage
    {
        private readonly Driver _driver;
        private readonly List<Passenger> _passageiros;
        private readonly string _localDestino;
        private readonly DatabaseService _databaseService;

        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _completedRouteOverlay; // NOVO: para rota já percorrida
        private GraphicsOverlay _stopsOverlay;
        private GraphicsOverlay _driverOverlay;

        private Graphic _driverGraphic;
        private Graphic _activeRouteGraphic; // NOVO: gráfico da rota ativa
        private Polyline _fullRouteLine; // NOVO: rota completa original
        private List<MapPoint> _allRoutePoints; // NOVO: todos os pontos da rota detalhada

        private bool _isTrackingLocation = false;

        public ViagemPage(Driver driver, List<Passenger> passageiros, string localDestino)
        {
            InitializeComponent();

            _driver = driver;
            _passageiros = passageiros;
            _localDestino = localDestino;
            _databaseService = new DatabaseService();

            InitializeMap();
        }

        private async void InitializeMap()
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                // Configurar o mapa
                MyMapView.Map = new Esri.ArcGISRuntime.Mapping.Map(BasemapStyle.ArcGISStreets);

                // Criar overlays
                _completedRouteOverlay = new GraphicsOverlay(); // NOVO: rota já percorrida (cinza)
                _routeOverlay = new GraphicsOverlay(); // Rota restante (vermelha)
                _stopsOverlay = new GraphicsOverlay();
                _driverOverlay = new GraphicsOverlay();

                MyMapView.GraphicsOverlays.Add(_completedRouteOverlay); // Adiciona primeiro (fica embaixo)
                MyMapView.GraphicsOverlays.Add(_routeOverlay);
                MyMapView.GraphicsOverlays.Add(_stopsOverlay);
                MyMapView.GraphicsOverlays.Add(_driverOverlay);

                // Inicializar localização do motorista
                await InitializeDriverLocation();

                // Criar a rota
                await CreateRoute();

                // Adicionar marcadores dos passageiros
                AddPassengerMarkers();

                // Centralizar o mapa
                CenterMapOnRoute();

                // Atualizar labels
                DestinationLabel.Text = _localDestino;
                PassengersLabel.Text = $"{_passageiros.Count} passageiros na rota";

                // Iniciar rastreamento de localização
                await StartLocationTracking();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao inicializar o mapa: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async Task InitializeDriverLocation()
        {
            try
            {
                MapPoint driverLocation;

                // PRIORIDADE 1: Tentar obter localização atual do GPS
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(10)
                });

                if (location != null)
                {
                    driverLocation = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);

                    // Atualizar no banco de dados
                    await _databaseService.UpdateDriverLocationAsync(_driver.Id, location.Latitude, location.Longitude);

                    // Atualizar o objeto driver local também
                    _driver.Latitude = location.Latitude;
                    _driver.Longitude = location.Longitude;
                }
                // PRIORIDADE 2: Usar localização salva no banco
                else if (_driver.Latitude != 0 && _driver.Longitude != 0)
                {
                    driverLocation = new MapPoint(_driver.Longitude, _driver.Latitude, SpatialReferences.Wgs84);
                }
                // PRIORIDADE 3: Calcular ponto médio entre os passageiros
                else if (_passageiros.Any(p => p.Latitude != 0 && p.Longitude != 0))
                {
                    var passengersWithLocation = _passageiros.Where(p => p.Latitude != 0 && p.Longitude != 0).ToList();
                    double avgLat = passengersWithLocation.Average(p => p.Latitude);
                    double avgLong = passengersWithLocation.Average(p => p.Longitude);

                    driverLocation = new MapPoint(avgLong, avgLat, SpatialReferences.Wgs84);

                    await DisplayAlert("Aviso",
                        "GPS não disponível. Usando localização estimada baseada nos passageiros.",
                        "OK");
                }
                // ÚLTIMO RECURSO: Localização padrão (Brasil)
                else
                {
                    driverLocation = new MapPoint(-47.8821, -21.2162, SpatialReferences.Wgs84);

                    await DisplayAlert("Aviso",
                        "GPS não disponível e sem dados de localização. Usando localização padrão.",
                        "OK");
                }

                // Criar símbolo do motorista (círculo azul)
                var driverSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 15);
                driverSymbol.Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 2);

                _driverGraphic = new Graphic(driverLocation, driverSymbol);
                _driverOverlay.Graphics.Add(_driverGraphic);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao inicializar localização: {ex.Message}", "OK");

                // Em caso de erro, usar localização padrão
                var defaultLocation = new MapPoint(-47.8821, -21.2162, SpatialReferences.Wgs84);
                var driverSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 15);
                driverSymbol.Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 2);
                _driverGraphic = new Graphic(defaultLocation, driverSymbol);
                _driverOverlay.Graphics.Add(_driverGraphic);
            }
        }

        private async Task CreateRoute()
        {
            try
            {
                // Limpa rotas anteriores
                _routeOverlay.Graphics.Clear();
                _completedRouteOverlay.Graphics.Clear();

                // Lista de pontos de rota (waypoints)
                var routeWaypoints = new List<MapPoint>();

                // Ponto inicial: motorista
                var driverPoint = _driverGraphic.Geometry as MapPoint;
                routeWaypoints.Add(driverPoint);

                // Ponto final: destino
                var destinationPoint = GetDestinationCoordinates(_localDestino);
                if (destinationPoint == null)
                {
                    await DisplayAlert("Erro", "Destino inválido.", "OK");
                    return;
                }

                // Adiciona passageiros com localização válida como paradas intermediárias
                foreach (var passenger in _passageiros.Where(p => p.Latitude != 0 && p.Longitude != 0))
                {
                    routeWaypoints.Add(new MapPoint(passenger.Longitude, passenger.Latitude, SpatialReferences.Wgs84));
                }

                routeWaypoints.Add(destinationPoint);

                // Serviço de rota do ArcGIS Online
                var routeTask = await RouteTask.CreateAsync(
                    new Uri("https://route.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World")
                );

                // Obter parâmetros padrão de rota
                var routeParams = await routeTask.CreateDefaultParametersAsync();

                // Adicionar paradas (waypoints)
                var stops = new List<Stop>();
                foreach (var point in routeWaypoints)
                    stops.Add(new Stop(point));

                routeParams.SetStops(stops);

                // Configurações opcionais
                routeParams.ReturnDirections = true;
                routeParams.ReturnRoutes = true;
                routeParams.OutputSpatialReference = SpatialReferences.Wgs84;

                // Calcular rota
                var routeResult = await routeTask.SolveRouteAsync(routeParams);

                if (routeResult?.Routes?.Any() == true)
                {
                    var route = routeResult.Routes.First();
                    _fullRouteLine = route.RouteGeometry as Polyline;

                    // NOVO: Extrair todos os pontos da polilinha para controle detalhado
                    _allRoutePoints = new List<MapPoint>();
                    foreach (var part in _fullRouteLine.Parts)
                    {
                        foreach (var point in part.Points)
                        {
                            _allRoutePoints.Add(point);
                        }
                    }

                    // Desenhar rota completa inicialmente (vermelha)
                    var routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 4);
                    _activeRouteGraphic = new Graphic(_fullRouteLine, routeSymbol);
                    _routeOverlay.Graphics.Add(_activeRouteGraphic);

                    // Centralizar o mapa na rota
                    await MyMapView.SetViewpointGeometryAsync(route.RouteGeometry, 100);
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível gerar uma rota com base nas ruas.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao criar rota: {ex.Message}", "OK");
            }
        }

        private void AddPassengerMarkers()
        {
            try
            {
                var passengerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Green, 10);
                passengerSymbol.Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 1);

                int passengersWithLocation = 0;
                int passengersWithoutLocation = 0;

                foreach (var passenger in _passageiros)
                {
                    if (passenger.Latitude != 0 && passenger.Longitude != 0)
                    {
                        var passengerPoint = new MapPoint(passenger.Longitude, passenger.Latitude, SpatialReferences.Wgs84);

                        // Marcador do passageiro
                        var passengerGraphic = new Graphic(passengerPoint, passengerSymbol);
                        passengerGraphic.Attributes["Name"] = passenger.Name;
                        passengerGraphic.Attributes["Id"] = passenger.Id;
                        _stopsOverlay.Graphics.Add(passengerGraphic);

                        // Label acima do marcador
                        var labelSymbol = new TextSymbol(passenger.Name, System.Drawing.Color.Black, 12,
                                                         Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                                                         Esri.ArcGISRuntime.Symbology.VerticalAlignment.Bottom)
                        {
                            OffsetY = -15,
                            HaloColor = System.Drawing.Color.White,
                            HaloWidth = 2
                        };

                        var labelGraphic = new Graphic(passengerPoint, labelSymbol);
                        _stopsOverlay.Graphics.Add(labelGraphic);

                        passengersWithLocation++;
                    }
                    else
                    {
                        passengersWithoutLocation++;
                        System.Diagnostics.Debug.WriteLine($"⚠️ Passageiro sem localização: {passenger.Name} (ID: {passenger.Id})");
                    }
                }

                // Mostrar aviso se houver passageiros sem localização
                if (passengersWithoutLocation > 0)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Aviso",
                            $"{passengersWithoutLocation} passageiro(s) não possui(em) localização cadastrada e não aparecerá(ão) no mapa.\n\n" +
                            $"Passageiros no mapa: {passengersWithLocation}",
                            "OK");
                    });
                }

                // Marcador do destino
                var destinationPoint = GetDestinationCoordinates(_localDestino);
                if (destinationPoint != null)
                {
                    var destinationSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, System.Drawing.Color.Red, 12);
                    var destinationGraphic = new Graphic(destinationPoint, destinationSymbol);
                    destinationGraphic.Attributes["Name"] = _localDestino;
                    _stopsOverlay.Graphics.Add(destinationGraphic);

                    // Label do destino
                    var destinationLabel = new TextSymbol(_localDestino, System.Drawing.Color.DarkRed, 13,
                                                          Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                                                          Esri.ArcGISRuntime.Symbology.VerticalAlignment.Bottom)
                    {
                        OffsetY = -18,
                        HaloColor = System.Drawing.Color.White,
                        HaloWidth = 2
                    };

                    _stopsOverlay.Graphics.Add(new Graphic(destinationPoint, destinationLabel));
                }

                // Atualizar label com informação correta
                PassengersLabel.Text = $"{passengersWithLocation} passageiros na rota";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar marcadores: {ex.Message}");
            }
        }

        [Obsolete]
        private async Task StartLocationTracking()
        {
            try
            {
                _isTrackingLocation = true;

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (_isTrackingLocation)
                    {
                        UpdateDriverLocation();
                        return true;
                    }
                    return false;
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao iniciar rastreamento: {ex.Message}", "OK");
            }
        }

        private async void UpdateDriverLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(1)
                });

                if (location != null)
                {
                    var newDriverLocation = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);
                    _driverGraphic.Geometry = newDriverLocation;

                    await _databaseService.UpdateDriverLocationAsync(_driver.Id, location.Latitude, location.Longitude);

                    // NOVO: Atualizar rota progressivamente
                    UpdateRouteProgress(newDriverLocation);

                    await MyMapView.SetViewpointCenterAsync(newDriverLocation, 5000);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar localização: {ex.Message}");
            }
        }

        // MÉTODO COMPLETAMENTE REESCRITO
        private void UpdateRouteProgress(MapPoint currentLocation)
        {
            try
            {
                if (_allRoutePoints == null || _allRoutePoints.Count == 0)
                    return;

                // Encontrar o ponto mais próximo na rota
                int closestIndex = 0;
                double minDistance = double.MaxValue;

                for (int i = 0; i < _allRoutePoints.Count; i++)
                {
                    double distance = GeometryEngine.Distance(currentLocation, _allRoutePoints[i]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestIndex = i;
                    }
                }

                // Só atualiza se estiver suficientemente próximo da rota (menos de 50 metros)
                if (minDistance > 50)
                    return;

                // Dividir a rota em: percorrida (cinza) e restante (vermelha)
                var completedPoints = _allRoutePoints.Take(closestIndex + 1).ToList();
                var remainingPoints = _allRoutePoints.Skip(closestIndex).ToList();

                // Incluir a posição atual do motorista na rota restante
                remainingPoints.Insert(0, currentLocation);

                // Limpar overlays
                _completedRouteOverlay.Graphics.Clear();
                _routeOverlay.Graphics.Clear();

                // Desenhar rota já percorrida (CINZA e mais fina)
                if (completedPoints.Count > 1)
                {
                    var completedBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
                    completedBuilder.AddPoints(completedPoints);
                    var completedLine = completedBuilder.ToGeometry();

                    var completedSymbol = new SimpleLineSymbol(
                        SimpleLineSymbolStyle.Solid,
                        System.Drawing.Color.FromArgb(180, 128, 128, 128), // Cinza semi-transparente
                        3
                    );

                    var completedGraphic = new Graphic(completedLine, completedSymbol);
                    _completedRouteOverlay.Graphics.Add(completedGraphic);
                }

                // Desenhar rota restante (VERMELHA)
                if (remainingPoints.Count > 1)
                {
                    var remainingBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
                    remainingBuilder.AddPoints(remainingPoints);
                    var remainingLine = remainingBuilder.ToGeometry();

                    var remainingSymbol = new SimpleLineSymbol(
                        SimpleLineSymbolStyle.Solid,
                        System.Drawing.Color.Red,
                        4
                    );

                    _activeRouteGraphic = new Graphic(remainingLine, remainingSymbol);
                    _routeOverlay.Graphics.Add(_activeRouteGraphic);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar progresso: {ex.Message}");
            }
        }

        private void CenterMapOnRoute()
        {
            try
            {
                if (_fullRouteLine != null)
                {
                    var envelope = _fullRouteLine.Extent;
                    MyMapView.SetViewpoint(new Viewpoint(envelope));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao centralizar mapa: {ex.Message}");
            }
        }

        private MapPoint GetDestinationCoordinates(string localDestino)
        {
            switch (localDestino)
            {
                case "ETEC Joaquim Ferreira do Amaral":
                    return new MapPoint(-48.5629, -22.2935, SpatialReferences.Wgs84);
                case "UNESP":
                    return new MapPoint(-49.0331, -22.3532, SpatialReferences.Wgs84);
                case "Unisagrado":
                    return new MapPoint(-49.05334, -22.3274, SpatialReferences.Wgs84);
                case "UNOESTE":
                    return new MapPoint(-48.6050, -22.3045, SpatialReferences.Wgs84);
                default:
                    return new MapPoint(-48.5548, -22.0050, SpatialReferences.Wgs84);
            }
        }

        private async void OnFinishTripClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Finalizar Viagem",
                "Deseja finalizar a viagem?",
                "Sim",
                "Não"
            );

            if (confirm)
            {
                _isTrackingLocation = false;

                await DisplayAlert(
                    "Viagem Finalizada!",
                    $"Passageiros na rota: {_passageiros.Count}",
                    "OK"
                );

                await Navigation.PopAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isTrackingLocation = false;
        }
    }
}