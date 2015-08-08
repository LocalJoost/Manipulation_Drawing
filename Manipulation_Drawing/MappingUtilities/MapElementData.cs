using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace MappingUtilities
{
	/// <summary>
	/// Helper class to attach data to map shapes. Well, to any DependencyObject, but that's to make 
	/// it fit into the PCL
	/// </summary>
	public static class MapElementData
	{
		#region Attached Dependency Property ObjectData
		public static readonly DependencyProperty ObjectDataProperty =
			 DependencyProperty.RegisterAttached("ObjectData",
			 typeof(object),
			 typeof(MapElementData),
			 new PropertyMetadata(default(object), null));

		public static object GetObjectData(MapElement obj)
		{
		  return obj.GetValue(ObjectDataProperty);
		}

		public static void SetObjectData(
		   DependencyObject obj,
		   object value)
		{
			obj.SetValue(ObjectDataProperty, value);
		}
		#endregion

		public static void AddData(this MapElement element, object data)
		{
			SetObjectData(element, data);
		}

		public static T ReadData<T>(this MapElement element) where T : class
		{
			return GetObjectData(element) as T;
		}
  }
}
