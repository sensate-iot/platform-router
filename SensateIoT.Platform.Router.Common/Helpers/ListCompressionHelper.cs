﻿/*
 * Compress generic arrays of data.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IO;
using System.IO.Compression;

namespace SensateIoT.Platform.Router.Common.Helpers
{
	public static class ListCompressionHelper
	{
		public static string Compress(this byte[] bytes)
		{
			string rv;
			MemoryStream msi, mso;
			GZipStream gzip;

			msi = null;
			mso = null;

			try {
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
