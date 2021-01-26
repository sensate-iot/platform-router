/*
 * DbDataReader extension methods.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Data.Common;

namespace SensateIoT.Platform.Network.DataAccess.Extensions
{
	public static class DbDataReaderExtensions
	{
		public static string SafeGetString(this DbDataReader reader, int colIndex)
		{
			if(!reader.IsDBNull(colIndex)) {
				return reader.GetString(colIndex);
			}

			return null;
		}
	}
}
