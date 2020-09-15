

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace SensateService.Helpers
{
	public static class ListCompressionHelper
	{
		public static string Compress<T>(this IList<T> measurements)
		{
			string rv;
			MemoryStream msi, mso;
			GZipStream gzip;

			msi = null;
			mso = null;

			try {
				var data = JsonConvert.SerializeObject(measurements);
				var bytes = Encoding.UTF8.GetBytes(data);

				msi = new MemoryStream(bytes);
				mso = new MemoryStream();
				gzip = new GZipStream(mso, CompressionMode.Compress);

				msi.CopyTo(gzip);
				gzip.Dispose();

				rv = Convert.ToBase64String(mso.ToArray());
			} finally {
				mso?.Dispose();
				msi?.Dispose();
			}

			return rv;
		}
	}
}