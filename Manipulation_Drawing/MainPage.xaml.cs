using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using MappingUtilities;
using Manipulation_Drawing.TileSources;

namespace Manipulation_Drawing
{
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      InitTileComboBox();
    }

    // -------------------- BASIC MANIPULATION -----------------------

    private async void InitialLocation(object sender, RoutedEventArgs e)
    {
      await MyMap.TrySetViewAsync(new Geopoint(new BasicGeoposition { Latitude = 52.181427, Longitude = 5.399780 }),
        16, RotationSlider.Value, PitchSlider.Value, MapAnimationKind.Bow);

      //MyMap.Center = new Geopoint(new BasicGeoposition {Latitude = 52.181427, Longitude = 5.399780});
      //MyMap.ZoomLevel = 16;
    }

    private void ZoomOut(object sender, RoutedEventArgs e)
    {
      MyMap.TryZoomOutAsync();
     // MyMap.ZoomLevel -= MyMap.ZoomLevel > MyMap.MinZoomLevel ? 1 : 0;
    }

    private void ZoomIn(object sender, RoutedEventArgs e)
    {
       MyMap.TryZoomInAsync();
      //MyMap.ZoomLevel += MyMap.ZoomLevel < MyMap.MaxZoomLevel ? 1 : 0;
    }

    // -------------------- 'ADVANCED' MANIPULATION -----------------------

    // Not used now - replaced by databinding
    private void RotationSliderOnValueChanged(object sender, 
                 RangeBaseValueChangedEventArgs e)
    {
      MyMap.TryRotateToAsync(e.NewValue);

      //Same result, sans the cool animation.
      MyMap.Heading = e.NewValue;

      //Rotates BY a certain value.
      MyMap.TryRotateAsync(e.NewValue);
    }

    // Not used now - replaced by databinding
    private void PitchSliderOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      //Only effective on ZoomLevel 7 and higher!
      MyMap.DesiredPitch = e.NewValue;

      // Does not work - read only
      // MyMap.Pitch = e.NewValue;
    }

    private MapScene lastScene;

    private void SaveScene(object sender, RoutedEventArgs e)
    {
      lastScene = MapScene.CreateFromCamera(MyMap.ActualCamera);
    }

    private void RestoreScene(object sender, RoutedEventArgs e)
    {
      if (lastScene != null)
      {
        MyMap.TrySetSceneAsync(lastScene);
      }
    }

    
    private void ToggleOption(object sender, RoutedEventArgs e)
    {
      MyMap.BusinessLandmarksVisible = ToggleBusiness.IsChecked.Value;
      MyMap.LandmarksVisible = ToggleLandMark.IsChecked.Value;
      MyMap.PedestrianFeaturesVisible = TogglePedestrian.IsChecked.Value;
      MyMap.TrafficFlowVisible = ToggleTraffic.IsChecked.Value;
      MyMap.TransitFeaturesVisible = ToggleTransit.IsChecked.Value;
    }

    private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
    {
      MyMap.ColorScheme = ThemeToggle.IsOn ? MapColorScheme.Light : MapColorScheme.Dark;
    }
    
    // -------------------- DRAWING STUFF ON THE MAP -----------------------

    private async void DrawPoints(object sender, RoutedEventArgs e)
    {
      // How to draw a new MapIcon with a label, anchorpoint and custom icon.
      // Icon comes from project assets
      var anchorPoint = new Point(0.5, 0.5);
      var image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/wplogo.png"));

      // Helper extension method
      try
      {
        var area = MyMap.GetViewArea();

        // PointList is just a helper class that gives 'some data'
        var points = PointList.GetRandomPoints(
          new Geopoint(area.NorthwestCorner), new Geopoint(area.SoutheastCorner),
          50);
        foreach (var dataObject in points)
        {
          var shape = new MapIcon
          {
            Title = dataObject.Name,
            Location = new Geopoint(dataObject.Points.First()),
            NormalizedAnchorPoint = anchorPoint,
            Image = image,
            CollisionBehaviorDesired = 
              onCollisionShow? MapElementCollisionBehavior.RemainVisible : 
                               MapElementCollisionBehavior.Hide,

            ZIndex = 3,
          };
          shape.AddData(dataObject);

          MyMap.MapElements.Add(shape);
        }
      }
      catch (Exception)
      {
        var dialog = new MessageDialog("GetViewArea error");
        await dialog.ShowAsync();
      }
    }

    private void DrawLines(object sender, RoutedEventArgs e)
    {
      if (!DeleteShapesFromLevel(2))
      {
        var strokeColor = Colors.Green;
        strokeColor.A = 200;

        //Drawing lines, notice the use of Geopath. Consists out of BasicGeopositions
        foreach (var dataObject in PointList.GetLines())
        {
          var shape = new MapPolyline
          {
            StrokeThickness = 9,
            StrokeColor = strokeColor,
            StrokeDashed = false,
            ZIndex = 2,
            Path = new Geopath(dataObject.Points)
          };
          shape.AddData(dataObject);

          MyMap.MapElements.Add(shape);
        }
      }
    }

    private void DrawShapes(object sender, RoutedEventArgs e)
    {
      if (!DeleteShapesFromLevel(1))
      {
        var strokeColor = Colors.DarkBlue;
        strokeColor.A = 100;
        var fillColor = Colors.Blue;
        fillColor.A = 100;

        foreach (var dataObject in PointList.GetAreas())
        {
          var shape = new MapPolygon
          {
            StrokeThickness = 3,
            StrokeColor = strokeColor,
            FillColor = fillColor,
            StrokeDashed = true,
            ZIndex = 1,
            Path = new Geopath(dataObject.Points)
          };
          shape.AddData(dataObject);

          MyMap.MapElements.Add(shape);
        }
      }
    }

    private bool onCollisionShow = false;
    private void CollisionToggle_Toggled(object sender, RoutedEventArgs e)
    {
      onCollisionShow = !onCollisionShow;
      var icons = MyMap.MapElements.OfType<MapIcon>();
      foreach (var icon in icons)
      {
        icon.CollisionBehaviorDesired = onCollisionShow
          ? MapElementCollisionBehavior.RemainVisible
          : MapElementCollisionBehavior.Hide;
      }
    }

    // -------------------- DELETING STUFF FROM THE MAP -----------------------

    private void DeleteAll(object sender, RoutedEventArgs e)
    {
      MyMap.MapElements.Clear();
    }

    private bool DeleteShapesFromLevel(int zIndex)
    {
      // Delete shapes by z-index 
      var shapesOnLevel = MyMap.MapElements.Where(p => p.ZIndex == zIndex);
      if (shapesOnLevel.Any())
      {
        foreach (var shape in shapesOnLevel.ToList())
        {
          MyMap.MapElements.Remove(shape);
        }
        return true;
      }
      return false;
    }

    // -------------------- QUERYING THE MAP -----------------------

    private async void OnMapElementClick(MapControl sender, MapElementClickEventArgs args)
    {
      var resultText = new StringBuilder();

      // Yay! Goodbye string.Format!
      resultText.AppendLine($"Position={args.Position.X},{args.Position.Y}");
      resultText.AppendLine($"Location={args.Location.Position.Latitude:F9},{args.Location.Position.Longitude:F9}");

      foreach (var mapObject in args.MapElements)
      {
        resultText.AppendLine("Found: " + mapObject.ReadData<PointList>().Name);
      }
      var dialog = new MessageDialog(resultText.ToString(),
        args.MapElements.Any() ? "Found the following objects" : "No data found");
      await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        async () => await dialog.ShowAsync());
    }

    // -------------------- MISC -----------------------


    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
      Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
    }

    private void MapSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var selectedItem = e.AddedItems.OfType<ComboBoxItem>().FirstOrDefault();
      if (selectedItem != null)
      {
        MapStyle mapStyle;
        if (Enum.TryParse(selectedItem.Content.ToString(), out mapStyle))
        {
          MyMap.Style = mapStyle;
        }
      }
    }

    // --- LATER --

    //  -------------------- TMS -----------------------
    private void TileComboChanged(object sender, SelectionChangedEventArgs e)
    {
      MyMap.TileSources.Clear();
      var tileSource = TileCombo.SelectedItem as ITileSource;
      if (tileSource != null)
      {
        MyMap.TileSources.Add(new MapTileSource(tileSource.TileSource));
      }
    }

    private void InitTileComboBox()
    {
      TileCombo.Items.Add("None");
      TileCombo.Items.Add(new OpenStreetMapSource());
      TileCombo.Items.Add(new GoogleMapSource("y", "Google Hybrid"));
      TileCombo.Items.Add(new WmsTileSource("NOAA Radar", "http://nowcoast.noaa.gov/arcgis/services/nowcoast/radar_meteo_imagery_nexrad_time/MapServer/WmsServer?", new[] { "1" }, "1.3.0", "CRS"));
      TileCombo.Items.Add(new WmsTileSource("NOAA Visible Img", "http://nowcoast.noaa.gov/arcgis/services/nowcoast/sat_meteo_imagery_goes_time/MapServer/WMSServer?", new[] { "9" }, "1.3.0", "CRS"));
      TileCombo.Items.Add(new WmsTileSource("RWS NWB", "http://geodata.nationaalgeoregister.nl/nwbwegen/ows?service=WMS", new[] { "wegvakken", "hectopunten" }));
      TileCombo.Items.Add(new WmsTileSource("RWS NWB 4326", "http://geodata.nationaalgeoregister.nl/nwbwegen/ows?service=WMS", new[] { "wegvakken", "hectopunten"},"1.3.0", "CRS", 4326));

      TileCombo.SelectedIndex = 0;
    }
  }
}
