using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Maps;

namespace Manipulation_Drawing.TileSources
{
  public class WmsTileSource : BaseHttpTileSource
  {
    public WmsTileSource(string name, string serviceUrl, IList<string> layers, string version = "1.3.0", 
      string referenceSystemName = "CRS", int epsg = 3857, string imageType = "image/png") : base()
    {
      Name = name;
      ServiceUrl = serviceUrl;
      Layers = layers;
      ImageType = imageType;
      Version = version;
      Epsg = epsg;
      ReferenceSystemName = referenceSystemName;
    }

    public string ServiceUrl { get; private set; }
    public string Name { get; private set; }
    public string Version { get; private set; }
    public int Epsg { get; private set; }
    public string ImageType { get; private set; }
    public string ReferenceSystemName { get; private set; }
    public IList<string> Layers { get; private set; }

    private const string WmsPostFix =
     @"BBOX={0},{1},{2},{3}&styles=&WIDTH={4}&HEIGHT={4}&{8}=EPSG:{5}&version={7}&service=WMS&FORMAT={9}&TRANSPARENT=TRUE&request=getmap&Layers={6}";

    private const int TileSize = 256;

    private Extent GetTileExtent(int x, int y, int zoom)
    {
      var tiles = (long)Math.Pow(2, zoom);
      var circumference = TileSize * tiles;
      var radius = circumference / (2 * Math.PI);

      var extent = new Extent
      {
        XLow = XToLong((x - (tiles / 2.0)) * TileSize, radius),
        YLow = YToLat(((tiles / 2.0) - y - 1) * TileSize, radius),
        XHigh = XToLong((x - (tiles / 2.0) + 1) * TileSize, radius),
        YHigh = YToLat(((tiles / 2.0) - y) * TileSize, radius)
      };

      if (Version == "1.3.0" && Epsg == 4326)
      {
        var newTypeExtent = new Extent
        {
          XLow = extent.YLow,
          YLow = extent.XLow,
          XHigh = extent.YHigh,
          YHigh = extent.XHigh
        };
        extent = newTypeExtent;
      }

      if (Epsg == 3857 || Epsg == 900913 )
      {
        extent = ToSphericalMercator(extent);
      }

      return extent;
    }

    private const double OriginShift = 20037508.34;

    private Extent ToSphericalMercator(Extent orgExent)
    {
      var p1 = ToSphericalMercator(orgExent.XLow, orgExent.YLow);
      var p2 = ToSphericalMercator(orgExent.XHigh, orgExent.YHigh);

      return new Extent
      {
        XLow = p1.X,
        YLow = p1.Y,
        XHigh = p2.X,
        YHigh = p2.Y
      };
    }

    protected override void MapUriRequested(HttpMapTileDataSource sender, MapTileUriRequestedEventArgs args)
    {
      var deferral = args.Request.GetDeferral();

      if (!(args.ZoomLevel < 4 && Epsg == 4326))
      {
        var extent = GetTileExtent(args.X, args.Y, args.ZoomLevel);
        var urlTemplate = string.Concat(ServiceUrl,
          ServiceUrl.EndsWith("?")
            ? string.Empty
            : ServiceUrl.EndsWith("&") ? string.Empty : (ServiceUrl.Contains("?") ? "&" : "?"), WmsPostFix);
        var url = string.Format(CultureInfo.InvariantCulture,
          urlTemplate, extent.XLow, extent.YLow, extent.XHigh, extent.YHigh, TileSize, Epsg,
          string.Join(",", Layers),
          Version, ReferenceSystemName, ImageType);

#if DEBUG
        Debug.WriteLine("Tile x={0}, y={1}, zoom={2}", args.X, args.Y, args.ZoomLevel);
        Debug.WriteLine("adding uri {0}", url);
#endif
        args.Request.Uri = new Uri(url);
      }
      deferral.Complete();
    }

    #region Translation routines

    // For explanation of these routines, see 
    // http://cfis.savagexi.com/articles/2006/05/03/google-maps-deconstructed


    private static double XToLong(double x, double radius)
    {
      var longRadians = x / radius;
      var longDegrees = longRadians * (180 / Math.PI);
      return longDegrees;
    }

    private static double YToLat(double y, double radius)
    {
      var latitude = (Math.PI / 2) - (2 * Math.Atan(Math.Exp(-1 * y / radius)));
      return latitude * (180 / Math.PI);
    }

    private Point ToSphericalMercator(double lon, double lat)
    {
      var x = lon * OriginShift / 180;
      var y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
      y = y * OriginShift / 180;
      return new Point(x, y);
    }

    #endregion

    public override string ToString()
    {
      return Name;
    }
  }
}
