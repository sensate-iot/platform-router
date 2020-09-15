﻿/*
 * Blob storage settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateService.Common.Data.Models;

namespace SensateService.Common.Config.Settings
{
	public class BlobStorageSettings
	{
		public string StoragePath { get; set; }
		public StorageType StorageType { get; set; }
	}
}