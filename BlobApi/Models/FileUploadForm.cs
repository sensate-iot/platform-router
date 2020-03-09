/*
 * Form upload viewmodel.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SensateService.BlobApi.Models
{
	public class FileUploadForm
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public IFormFile File { get; set; }
		[Required]
		public string SensorId { get; set; }
	}
}
