using Microsoft.Maui.Controls;
using TCC.Models;

// ArcGis packages
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace TCC.Views;

public partial class ViagemPage : ContentPage
{
    public ViagemPage()
    {
        InitializeComponent();
    }
}