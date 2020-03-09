/*
 * Blob storage settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateService.Models;

namespace SensateService
{
	public class BlobStorageSettings
	{
		public string StoragePath { get; set; }
		public StorageType StorageType { get; set; }
	}
}
