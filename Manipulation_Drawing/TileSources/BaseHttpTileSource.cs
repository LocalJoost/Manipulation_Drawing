using Windows.UI.Xaml.Controls.Maps;

namespace Manipulation_Drawing.TileSources
{
  public abstract class BaseHttpTileSource : ITileSource
  {
    protected BaseHttpTileSource()
    {
      var t = new HttpMapTileDataSource();
      t.UriRequested += MapUriRequested;
      TileSource = t;
    }

    public MapTileDataSource TileSource { get; private set; }

    protected abstract void MapUriRequested(HttpMapTileDataSource sender, MapTileUriRequestedEventArgs args);

  }
}
