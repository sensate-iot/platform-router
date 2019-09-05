/*
 * Object helper functions.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SensateService.Helpers
{
	public static class ObjectHelper
	{
		public static byte[] ToByteArray(this object obj)
		{
			BinaryFormatter formatter;

			if(obj == null)
				return null;

			formatter = new BinaryFormatter();
			using(var stream = new MemoryStream()) {
				formatter.Serialize(stream, obj);
				return stream.ToArray();
			}
		}

		public static T FromByteArray<T>(this byte[] bytes) where T : class
		{
			BinaryFormatter formatter;

			if(bytes == null)
				return null;

			formatter = new BinaryFormatter();
			using(var stream = new MemoryStream(bytes)) {
				var data = formatter.Deserialize(stream);
				return data as T;
			}
		}

		public static void Populate<T>(this T[] ary, T value)
		{
			for(var idx = 0; idx < ary.Length; ++idx)
				ary[idx] = value;
		}
	}
}
