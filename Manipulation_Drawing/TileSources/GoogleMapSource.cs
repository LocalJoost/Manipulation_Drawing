using System;
using Windows.UI.Xaml.Controls.Maps;

namespace Manipulation_Drawing.TileSources
{
  /// <summary>
  /// WARNING. This violates Google's TOS. Do not use in production environments. 
  /// </summary>
  public class GoogleMapSource : BaseHttpTileSource
  {

    private string mapPrefix;
    private string mapName;
    public GoogleMapSource(string prefix, string name = "Google") :base()
    {
      mapPrefix = prefix;
      mapName = name;
    }

    protected override void MapUriRequested(HttpMapTileDataSource sender, MapTileUriRequestedEventArgs args)
    {
      var deferral = args.Request.GetDeferral();
      args.Request.Uri = new Uri($"http://mt{(args.X % 2) + (2 * (args.Y % 2))}.google.com/vt/lyrs={mapPrefix}&z={args.ZoomLevel}&x={args.X}&y={args.Y}");
      deferral.Complete();
    }

    public override string ToString()
    {
      return mapName;
    }
  }
}
