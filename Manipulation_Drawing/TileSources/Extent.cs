namespace Manipulation_Drawing.TileSources
{
  public struct Extent
  {
    private double x1;
    public double XLow
    {
      get
      {
        return (x1 > x2 ? x2 : x1);
      }
      set
      {
        x1 = value;
      }

    }

    private double x2;
    public double XHigh
    {
      get
      {
        return (x1 < x2 ? x2 : x1);
      }
      set { x2 = value; }
    }

    private double y1;
    public double YLow
    {
      get
      {
        return (y1 > y2 ? y2 : y1);
      }
      set { y1 = value; }
    }

    private double y2;
    public double YHigh
    {
      get
      {
        return (y1 < y2 ? y2 : y1);
      }
      set { y2 = value; }
    }
  }
}
