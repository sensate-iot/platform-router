/*
 * Object helper functions.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
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
	}
}
