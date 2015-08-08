using System;
using Windows.UI.Xaml.Controls.Maps;

namespace Manipulation_Drawing.TileSources
{
  public class OpenStreetMapSource : BaseHttpTileSource
  {

    private readonly static string[] TilePathPrefixes = { "a", "b", "c" };

    protected override void MapUriRequested(HttpMapTileDataSource sender, MapTileUriRequestedEventArgs args)
    {
      var deferral = args.Request.GetDeferral();
      // TilePathPrefixes - load balancing + caching
      args.Request.Uri = new Uri($"http://{TilePathPrefixes[args.Y % 3]}.tile.openstreetmap.org/{args.ZoomLevel}/{args.X}/{args.Y}.png");
      deferral.Complete();
    }

    public override string ToString()
    {
      return "OpenStreetMap";
    }
  }
}
