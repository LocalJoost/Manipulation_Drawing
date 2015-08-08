using Windows.UI.Xaml.Controls.Maps;

namespace Manipulation_Drawing.TileSources
{
  interface ITileSource
  {
    MapTileDataSource TileSource { get; }
  }
}
