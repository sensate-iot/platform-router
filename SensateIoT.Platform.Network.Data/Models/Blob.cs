/*
 * Blob storage model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace SensateIoT.Platform.Network.Data.Models
{
	public enum StorageType
	{
		FileSystem,
		DigitalOceanSpaces,
		FTP
	}

	public class Blob
	{
		[Required]
		public long ID { get; set; }
		[Required, StringLength(24, MinimumLength = 24), JsonProperty("sensorId")]
		public string SensorID { get; set; }
		[Required]
		public string FileName { get; set; }
		[Required, JsonIgnore]
		public string Path { get; set; }
		[Required, JsonIgnore]
		public StorageType StorageType { get; set; }
		public int FileSize { get; set; }
		[Required]
		public DateTime Timestamp { get; set; }
	}
}
