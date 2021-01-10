/*
 * Blob storage settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.Common.Config.Settings
{
	public class BlobStorageSettings
	{
		public string StoragePath { get; set; }
		public StorageType StorageType { get; set; }
	}
}
